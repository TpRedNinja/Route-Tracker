using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace Route_Tracker
{
    public unsafe class AC4GameStats : GameStatsBase
    {
        #region Memory Offsets Arrays
        // Offsets for direct memory reads (special cases)
        private readonly int[] characterPtrOffsets = []; // has no offsets
        private readonly int[] mainMenuPtrOffsets = []; // has no offsets
        private readonly int[] percentFtPtrOffsets = [0x74];
        private readonly int[] legendaryShipPtrOffsets = [0x170];
        private readonly int[] percentPtrOffsets = [0x284];
        private readonly int[] loadingPtrOffsets = [0x7D8];
        private readonly int[] fortsPtrOffsets = [0x7F0, 0xD68, 0xD70, 0x30];
        
        #endregion

        #region Game State Flag
        // Tracks if the game is currently loading or at the main menu
        private bool isLoading = false;
        private bool isMainMenu = false;
        #endregion

        #region Base Addresses
        // Pre-calculated base addresses for memory reads
        private readonly nint collectiblesBaseAddress;
        private readonly nint missionBaseAddress;
        private readonly nint resourcesBaseAddress;
        #endregion

        #region Collectible/Progress Offsets
        // Unique third offsets for each collectible type
        private const int ViewpointsThirdOffset = -0x1B30;
        private const int MyanThirdOffset = -0x1B1C;
        private const int TreasureThirdOffset = -0xBB8;
        private const int FragmentsThirdOffset = -0x1B58;
        private const int AssassinThirdOffset = -0xDD4;
        private const int NavalThirdOffset = 0x1950;
        private const int HeroUpgradeThirdOffset = -0x19F0; // Reserved for future use
        private const int LettersThirdOffset = -0x04EC;
        private const int ManuscriptsThirdOffset = -0x334;
        private const int MusicThirdOffset = 0x424;

        // Shared offsets for pointer traversal
        private const int FirstOffset = 0x2D0;
        private const int SecondOffset = 0x8BC;
        private const int LastOffset = 0x18;
        private const int OffsetStep = 0x14;
        // Shared offsets for different Missions
        private const int MissionFirstOffset = -0x850;
        private const int MissionEndOffset = -0x3D0;
        private const int TemplarHuntFirstOffset = -0x370;
        private const int TemplarHuntEndOffset = -0x250;
        private const int MissionOffsetStep = 0x60;

        // Ranges for flag-based collectibles
        private const int ChestStartOffset = 0x67C;
        private const int ChestEndOffset = 0xA8C;
        private const int TavernStartOffset = 0x319C;
        private const int TavernEndOffset = 0x3228;
        private const int TreasureMapsStartOffset = 0x3250;
        private const int TreasureMapsEndOffset = 0x3408;
        private static readonly int [] SpecialTreasureMapOffsets = [0x33F4]; // Special treasure map offset we dont count
        #endregion

        #region Progress and Upgrade Tracking
        // Special activity counters (updated via memory or logic)
        private int completedStoryMissions = 0;
        private int totalTemplarHunts = 0;
        private int legendaryShips = 0;
        private int treasuremaps = 0;
        private int modernDayMissions = 0;

        // Modern Day mission tracking
        // Tracks the previous value of the 'character' variable to detect transitions in and out of modern day missions.
        // Default is 0 (not in modern day). When entering modern day, value becomes 1. When returning to main game (1 -> 0),
        // this is used to increment the completed modern day mission count and update the state for the next transition.
        private int oldcharacter = 0;
        private int totalUpgrades = 0;

        // for windmill fragment check
        public bool isWindmillFragment = false; // this is used to check if we have collected the windmill fragment
        private DateTime lastViewpointUpdate = DateTime.MinValue;
        private DateTime lastFragmentUpdate = DateTime.MinValue;
        private DateTime lastBuriedUpdate = DateTime.MinValue;
        private const int EXPECTED_FRAGMENT_COUNT = 87; // This is the expected fragment count for windmill check
        #endregion

        // ==========FORMAL COMMENT=========
        // Initializes a new game statistics tracker for Assassin's Creed 4
        // Configures memory access and calculates the base address for collectibles
        // ==========MY NOTES==============
        // Sets up our tracker to read AC4's memory
        // Calculates the starting point for finding all the collectibles
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public AC4GameStats(IntPtr processHandle, IntPtr baseAddress)
    : base(processHandle, baseAddress)
        {
            this.collectiblesBaseAddress = (nint)baseAddress + 0x026BEAC0;
            this.resourcesBaseAddress = (nint)baseAddress + 0x00E88810;
            this.missionBaseAddress = (nint)baseAddress + 0x00A0E21C;
        }

        #region Memory Reading Methods

        // ==========FORMAL COMMENT=========
        // Helper method to read collectibles using shared memory pattern
        // Uses common base address and offset structure with specific third offset
        // ==========MY NOTES==============
        // Simplifies reading collectibles that follow the standard pattern
        // Makes the code cleaner by removing duplicated memory reading logic
        private int ReadCollectible(int thirdOffset)
        {
            string cacheKey = $"collectible_{thirdOffset}";
            return ReadWithCache<int>(cacheKey, collectiblesBaseAddress, [FirstOffset, SecondOffset, thirdOffset, LastOffset]);
        }

        // function made by me
        private int ReadMission(int missionOffset)
        {   
            string cacheKey = $"mission_{missionOffset}";
            return ReadWithCache<int>(cacheKey, missionBaseAddress, [missionOffset]);
        }

        // ==========FORMAL COMMENT=========
        // Helper method to count collectibles that are stored as individual flags
        // Iterates through a range of third offsets and sums their values
        // ==========MY NOTES==============
        // Used for things like chests and taverns that have many individual locations
        // Each location has its own memory address with a predictable pattern
        // startOffset and endOffset are comes from getstats() we pass them through lol
        private int CountCollectibles(int startOffset, int endOffset)
        {
            string cacheKey = $"count_{startOffset}_{endOffset}";

            // Try to get from cache using the base class method
            if (TryGetCachedValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            // Count if not in cache
            int count = 0;
            for (int thirdOffset = startOffset; thirdOffset <= endOffset; thirdOffset += OffsetStep)
            {
                // skip these maps as the user should have them collected already or the map isnt used anyways
                if (SpecialTreasureMapOffsets.Contains(thirdOffset)) continue;
                count += ReadCollectible(thirdOffset);
            }

            // Store in cache
            StoreInCache(cacheKey, count);
            return count;
        }

        // startOffset and endOffset are comes from getstats() we pass them through lol
        // function made by me
        private int CountMissions(int startOffset, int endOffset) 
        {
            string cacheKey = $"count_{startOffset}_{endOffset}";

            // Try to get from cache using the base class method
            if (TryGetCachedValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            // Count if not in cache
            int count = 0;
            for (int missionOffset = startOffset; missionOffset <= endOffset; missionOffset += MissionOffsetStep)
            {
                count += ReadMission(missionOffset);
            }

            // Store in cache
            StoreInCache(cacheKey, count);
            return count;
        }

        // ==========FORMAL COMMENT=========
        // Retrieves current game statistics by reading from multiple memory locations
        // Collects data on completion percentage, collectibles, and other trackable items
        // Also processes percentage changes to detect special activities
        // ==========MY NOTES==============
        // The main method that gets all the stats from the game
        // Reads everything from memory: percentage, collectibles, etc.
        // Also checks if you've completed any special activities
        public (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
            int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music,
            int Forts, int Taverns, int TotalChests) GetStats()
        {
            // Reading collectibles using existing methods
            int percent = ReadWithCache<int>("percent",(nint)baseAddress + 0x49D9774, percentPtrOffsets);
            float percentFloat = ReadWithCache<float>("percentFloat", (nint)baseAddress + 0x049F1EE8, percentFtPtrOffsets);
            int forts = ReadWithCache<int>("forts", (nint)baseAddress + 0x026C0A28, fortsPtrOffsets);

            // for other functions
            int mainmenu = ReadWithCache<int>("mainmenu", (nint)baseAddress + 0x49D2204, mainMenuPtrOffsets);
            bool loading = ReadWithCache<bool>("loading", (nint)baseAddress + 0x04A1A6CC, loadingPtrOffsets);
            int character = ReadWithCache<int>("character", (nint)baseAddress + 0x23485C0, characterPtrOffsets);

            // Read all other collectibles
            int viewpoints = ReadCollectible(ViewpointsThirdOffset);
            int myan = ReadCollectible(MyanThirdOffset);
            int treasure = ReadCollectible(TreasureThirdOffset);
            int fragments = ReadCollectible(FragmentsThirdOffset);
            int assassin = ReadCollectible(AssassinThirdOffset);
            int naval = ReadCollectible(NavalThirdOffset);
            int letters = ReadCollectible(LettersThirdOffset);
            int manuscripts = ReadCollectible(ManuscriptsThirdOffset);
            int music = ReadCollectible(MusicThirdOffset);
            int taverns = CountCollectibles(TavernStartOffset, TavernEndOffset);
            int totalChests = CountCollectibles(ChestStartOffset, ChestEndOffset);
            totalUpgrades = ReadCollectible(HeroUpgradeThirdOffset);

            // legendary ship,templar hunt, storymissions, and treasuremaps counts
            legendaryShips = ReadWithCache<int>("legendaryships", (nint)baseAddress + 0x00A0E21C, legendaryShipPtrOffsets);
            totalTemplarHunts = CountMissions(TemplarHuntFirstOffset, TemplarHuntEndOffset);
            completedStoryMissions = CountMissions(MissionFirstOffset, MissionEndOffset);
            treasuremaps = CountCollectibles(TreasureMapsStartOffset, TreasureMapsEndOffset);

            // register stats and update last update times
            RegisterStat("Buried", treasure);
            RegisterStat("Fragments", fragments);
            RegisterStat("Viewpoints", viewpoints);

            // may move these to a separate method if i can
            if (Current.Viewpoints > Old.Viewpoints)
            {
                lastViewpointUpdate = DateTime.Now;
            }
            else if (Current.Fragments > Old.Fragments)
            {
                lastFragmentUpdate = DateTime.Now;
            }
            else if (Current.Buried > Old.Buried)
            {
                lastBuriedUpdate = DateTime.Now;
            }

            // Detect modern day missions
            DetectModernDayMissions(character, loading);

            // DetectStatuses
            DetectStatuses(mainmenu, loading);

            // call windmill fragment check
            Windmillfragment();

            // Return all the stats (including the basic ones that we got from memory)
            return (percent, percentFloat, viewpoints, myan, treasure, fragments, assassin, naval,
                letters, manuscripts, music, forts, taverns, totalChests);
        }
        #endregion

        #region State Detection Methods
        // ==========FORMAL COMMENT=========
        // Methods that analyze game memory to detect specific events or states
        // Includes activity detection, upgrade tracking, and game status monitoring
        // ==========MY NOTES==============
        // These methods figure out what the player is doing or has done
        // They watch for changes in memory values that indicate game activities
        // Used to track progress through the route
        private void DetectModernDayMissions(int character, bool loading)
        {
            if (character > 0 && oldcharacter != character && !loading)
            {
                oldcharacter = character;
            }
            else if(character == 0 && oldcharacter > 0 && !loading)
            {
                modernDayMissions++;
                oldcharacter = character;
                
            }
        }

        private void DetectStatuses(int mainmenu, bool loading)
        {
            //loading status
            if(loading)
            {
                isLoading = true;
            }
            else
            {
                isLoading = false;
            }

            //main menu status
            if(mainmenu == 65540)
            {
                isMainMenu = true;
            }
            else
            {
                isMainMenu = false;
            }
        }

        // function to check if we have collected the windmill fragment
        private void Windmillfragment()
        {
            // case 1: We get viewpoint first then fragment within 10 seconds
            if (Current.Fragments > Old.Fragments &&
            (DateTime.Now - lastViewpointUpdate).TotalSeconds <= 10)
            {
                isWindmillFragment = true;
            } else if (Current.Viewpoints > Old.Viewpoints &&
            (DateTime.Now - lastFragmentUpdate).TotalSeconds <= 10)
            // Case 2: Fragment then viewpoint within 10 seconds
            {
                isWindmillFragment = true;
            } else if (Current.Buried == 1 && Current.Fragments > EXPECTED_FRAGMENT_COUNT)
            // Case 3: First chest (New Bone) collected, but fragments is higher than expected
            {
                isWindmillFragment = true;
            } else
            {
                isWindmillFragment = false;
            }
        }
        #endregion

        #region Public Stats Interface
        // ==========FORMAL COMMENT=========
        // Returns the current counters for special activities tracked directly from memory.
        // Provides access to story mission, templar hunt, legendary ship, and treasure map completion counts.
        // Used by the route tracking system to mark these activities as completed.
        // ==========MY NOTES==============
        // Tells the route tracker how many special activities we've completed.
        // This is how the UI knows when to check off story missions, legendary ships, and treasure maps.
        // Returns a tuple with all four counter values at once.
        public (int StoryMissions, int TemplarHunts, int LegendaryShips, int TreasureMaps) GetSpecialActivityCounts()
        {
            return (completedStoryMissions, totalTemplarHunts, legendaryShips, treasuremaps);
        }

        public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            return (isLoading, isMainMenu);
        }

        // ==========FORMAL COMMENT=========
        // Returns all game statistics as a dictionary for flexible usage
        // Gathers data from all memory reading methods into a single structure
        // ==========MY NOTES==============
        // This is the primary method that the UI uses to get all stats
        // Returns everything in a dictionary so different games can have different stats
        public override Dictionary<string, object> GetStatsAsDictionary()
        {
            // Get stats using existing methods
            var (Percent, PercentFloat, Viewpoints, Myan, Treasure, Fragments, Assassin, Naval, Letters, Manuscripts, Music, Forts, Taverns, TotalChests) = GetStats();
            var (StoryMissions, TemplarHunts, LegendaryShips, TreasureMaps) = GetSpecialActivityCounts();
            var (IsLoading, IsMainMenu) = GetGameStatus();

            // Return everything in a dictionary
            return new Dictionary<string, object>
            {
                // Core stats
                ["Completion Percentage"] = Percent,
                ["Exact Percentage"] = Math.Round(PercentFloat, 2),
                ["Viewpoints"] = Viewpoints,
                ["Myan Stones"] = Myan,
                ["Buried Treasure"] = Treasure,
                ["Animus Fragments"] = Fragments,
                ["Assassin Contracts"] = Assassin,
                ["Naval Contracts"] = Naval,
                ["Letter Bottles"] = Letters,
                ["Manuscripts"] = Manuscripts,
                ["Music Sheets"] = Music,
                ["Forts"] = Forts,
                ["Taverns"] = Taverns,
                ["Chests"] = TotalChests,

                // Special activities
                ["Story Missions"] = StoryMissions,
                ["Templar Hunts"] = TemplarHunts,
                ["Legendary Ships"] = LegendaryShips,
                ["Treasure Maps"] = TreasureMaps,

                // Other stats
                ["Hero Upgrades"] = totalUpgrades,
                ["Modern Day Missions"] = modernDayMissions,

                // Game state
                ["Is Loading"] = IsLoading,
                ["Is Main Menu"] = IsMainMenu
            };
        }

        #endregion
    }
}

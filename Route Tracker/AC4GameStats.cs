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
        private static readonly int [] SpecialTreasureMapOffsets = [0x33B8, 0x33CC, 0x33E0, 0x33F4]; // Special treasure map offset we dont count this one
        #endregion

        #region Resource Tracking
        // Offsets for tracking resources spent (used for upgrade detection)
        private const int RESOURCES_FIRST_OFFSET = 0x104;
        private readonly int[] moneySpentOffsets = [0xB0, 0xC, 0x58, 0x26C];
        private readonly int[] woodSpentOffsets = [0xA0, 0x3C, 0x80, 0x10C];
        private readonly int[] metalSpentOffsets = [0xB8, 0x4, 0x28, 0xAC];
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

        // Upgrade tracking
        private int lastMoneySpent = 0;
        private int lastWoodSpent = 0;
        private int lastMetalSpent = 0;
        private int lastHeroUpgradeValue = 0;
        private int totalUpgrades = 0;
        private readonly bool[] upgradePurchased = new bool[43]; // Each index = a specific upgrade
        private int skinPurchaseCheckpoint = 0; // Used for animal skin upgrade detection

        // Main ship upgrades (excluding hero and animal skin upgrades)
        private static readonly (int Index, int Money, int Wood, int Metal, string Name)[] MainUpgradeRequirements =
        [
            (0, 500, 0, 0, "Swords 1"),
            (3, 1000, 0, 0, "Hull 1"),
            (4, 900, 0, 0, "Round Shot Strength 1"),
            (5, 4000, 0, 0, "Round Shot Strength 2"),
            (6, 800, 0, 0, "Mortar 1"),
            (7, 900, 0, 0, "Heavy Shot 1"),
            (8, 6000, 0, 0, "Heavy Shot 2"),
            (9, 800, 0, 0, "Mortar Storage 1"),
            (10, 2000, 0, 0, "Mortar Storage 2"),
            (11, 0, 0, 70, "Cannons 1"),
            (12, 12000, 0, 0, "Round Shot Strength 3"),
            (13, 2500, 0, 0, "Chain Shot Strength 1"),
            (14, 6000, 0, 0, "Chain Shot Strength 2"),
            (15, 3000, 0, 0, "Fire Barrel Strength 1"),
            (16, 500, 0, 0, "Heavy Shot Storage 1"),
            (17, 1500, 0, 0, "Heavy Shot Storage 2"),
            (18, 3500, 0, 200, "Mortar 2"),
            (19, 700, 0, 0, "Swivel Strength 1"),
            (20, 14000, 0, 0, "Officers Rapiers"),
            (21, 9000, 0, 0, "Cannon-Barrel Pistols"),
            (34, 4000, 200, 100, "Hull Armor 2"),
            (35, 5000, 0, 0, "Diving Bell"),
            (36, 5000, 0, 0, "Mortar Storage 3"),
            (37, 500, 25, 0, "Ram Strength 1"),
            (38, 5000, 250, 150, "Ram Strength 2"),
            (39, 8000, 0, 300, "Mortar 3"),
            (40, 2000, 0, 100, "Broadside Cannons 2"),
            (41, 35000, 0, 0, "Round Shot Strength 4"),
            (42, 25000, 0, 0, "Heavy Shot 3"),
        ];

        // Animal skin upgrades
        private static readonly (int Index, int Cost, string Name)[] SkinUpgradeRequirements =
        [
            (21, 1400, "Rabbit Pelt"),
            (22, 1700, "Hutia Pelt"),
            (23, 2000, "Howler Monkey Skin"),
            (24, 4000, "Crocodile Leather"),
            (25, 6300, "Killer Whale Skin"),
            (26, 7000, "Humpback Whale Skin"),
        ];

        #endregion

        // ==========FORMAL COMMENT=========
        // Initializes a new game statistics tracker for Assassin's Creed 4
        // Configures memory access and calculates the base address for collectibles
        // ==========MY NOTES==============
        // Sets up our tracker to read AC4's memory
        // Calculates the starting point for finding all the collectibles
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0290: Use primary constructure",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "IDE0079:Remove unnecessary suppression",
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
        // Helper method to read resource expenditure values
        // Follows pointer paths to track spent resources
        // ==========MY NOTES==============
        // Similar to ReadCollectible but for resources spent
        // Combines the shared first offset with resource-specific offsets
        private int ReadResourceSpent(int[] uniqueOffsets)
        {
            string cacheKey = $"resource_{string.Join("_", uniqueOffsets)}";

            // Try to get from cache
            if (TryGetCachedValue(cacheKey, out int cachedValue))
            {
                return cachedValue;
            }

            // Combine the shared first offset with resource-specific offsets
            int[] fullOffsets = new int[uniqueOffsets.Length + 1];
            fullOffsets[0] = RESOURCES_FIRST_OFFSET;
            Array.Copy(uniqueOffsets, 0, fullOffsets, 1, uniqueOffsets.Length);

            try
            {
                // Read and cache
                int result = Read<int>(resourcesBaseAddress, fullOffsets);
                StoreInCache(cacheKey, result);
                return result;
            }
            catch (Exception)
            {
                return 0; // Return 0 if reading fails
            }
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
            int heroupgrades = ReadCollectible(HeroUpgradeThirdOffset);

            // Read resource spent values
            int moneySpent = ReadResourceSpent(moneySpentOffsets);
            int woodSpent = ReadResourceSpent(woodSpentOffsets);
            int metalSpent = ReadResourceSpent(metalSpentOffsets);

            // legendary ship,templar hunt, storymissions, and treasuremaps counts
            legendaryShips = ReadWithCache<int>("legendaryships", (nint)baseAddress + 0x00A0E21C, legendaryShipPtrOffsets);
            totalTemplarHunts = CountMissions(TemplarHuntFirstOffset, TemplarHuntEndOffset);
            completedStoryMissions = CountMissions(MissionFirstOffset, MissionEndOffset);
            treasuremaps = CountCollectibles(TreasureMapsStartOffset, TreasureMapsEndOffset);

            // Detect upgrades using the resource values
            HandleUpgradeCases(heroupgrades, moneySpent, woodSpent, metalSpent);

            // Detect modern day missions
            DetectModernDayMissions(character);

            // DetectStatuses
            DetectStatuses(mainmenu, loading);

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
        private void DetectModernDayMissions(int character)
        {
            if (character == 1 && character != oldcharacter)
            {
                oldcharacter = character;
            }
            else if(character == 0 && oldcharacter == 1)
            {
                oldcharacter = character;
                modernDayMissions++;
            }
        }

        // Implement HandleUpgradeCases:
        private void HandleUpgradeCases(int currentHeroUpgrade, int moneySpent, int woodSpent, int metalSpent)
        {
            // First call - establish baseline
            if (lastMoneySpent == 0)
            {
                lastMoneySpent = moneySpent;
                lastWoodSpent = woodSpent;
                lastMetalSpent = metalSpent;
                lastHeroUpgradeValue = currentHeroUpgrade;
                return;
            }

            // Detect hero upgrades
            if (currentHeroUpgrade > lastHeroUpgradeValue)
            {
                int newUpgrades = currentHeroUpgrade - lastHeroUpgradeValue;

                // Mark upgrades based on hero upgrade number
                // Using the upgrade list from upgrades.txt
                for (int i = 0; i < newUpgrades && lastHeroUpgradeValue + i < 8; i++)
                {
                    int upgradeIndex = -1;

                    // Map hero upgrade value to the correct upgrade in the list
                    switch (lastHeroUpgradeValue + i + 1) // +1 because hero upgrades start at 1
                    {
                        case 1: upgradeIndex = 1; break;  // Pistol Holster 2/Health 1
                        case 2: upgradeIndex = 2; break;  // Pistol Holster 2/Health 1 (second option)
                        case 3: upgradeIndex = 28; break; // Pistol Holster 3
                        case 4: upgradeIndex = 29; break; // Pistol Holster 4
                        case 5: upgradeIndex = 30; break; // Smoke Bomb Pouch 1
                        case 6: upgradeIndex = 31; break; // Smoke Bomb Pouch 2
                        case 7: upgradeIndex = 32; break; // Dart Pouch 1
                        case 8: upgradeIndex = 33; break; // Dart Pouch 2
                    }

                    if (upgradeIndex >= 0 && upgradeIndex < upgradePurchased.Length && !upgradePurchased[upgradeIndex])
                    {
                        upgradePurchased[upgradeIndex] = true;
                        totalUpgrades++;
                        Debug.WriteLine($"Hero upgrade detected: {upgradeIndex + 1}");
                    }
                }

                lastHeroUpgradeValue = currentHeroUpgrade;
            }

            // Check for specific resource expenditure patterns
            int moneyDelta = moneySpent - lastMoneySpent;
            int woodDelta = woodSpent - lastWoodSpent;
            int metalDelta = metalSpent - lastMetalSpent;

            // Only process if there's been a change
            if (moneyDelta > 0 || woodDelta > 0 || metalDelta > 0)
            {
                // Main upgrades (excluding hero and animal skin upgrades)
                foreach (var req in MainUpgradeRequirements)
                {
                    if (req.Index > 0 && !upgradePurchased[req.Index - 1])
                        continue;
                    
                    if (!upgradePurchased[req.Index]
                        && moneyDelta >= req.Money
                        && woodDelta >= req.Wood
                        && metalDelta >= req.Metal)
                    {
                        upgradePurchased[req.Index] = true;
                        totalUpgrades++;
                        Debug.WriteLine($"Detected {req.Name} upgrade");
                        // If only one upgrade should be detected per call, uncomment the next line:
                        // break;
                    }
                }

                // Animal skin upgrades (only after Cannon-Barrel Pistols)
                if (skinPurchaseCheckpoint > 0)
                {
                    int skinMoneyDelta = moneySpent - skinPurchaseCheckpoint;
                    foreach (var (index, cost, name) in SkinUpgradeRequirements)
                    {
                        if (skinMoneyDelta >= cost && !upgradePurchased[index])
                        {
                            upgradePurchased[index] = true;
                            totalUpgrades++;
                            Debug.WriteLine($"Detected {name} upgrade");
                            skinPurchaseCheckpoint = moneySpent;
                            break; // Only one skin upgrade per purchase
                        }
                    }
                }

                // Set the checkpoint for animal skin purchases after Cannon-Barrel Pistols
                if (moneyDelta >= 9000 && !upgradePurchased[20])
                {
                    skinPurchaseCheckpoint = moneySpent;
                }

                // Update the last spent values
                lastMoneySpent = moneySpent;
                lastWoodSpent = woodSpent;
                lastMetalSpent = metalSpent;
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
            return (completedStoryMissions + modernDayMissions, totalTemplarHunts, legendaryShips, treasuremaps);
        }

        public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            return (isLoading, isMainMenu);
        }

        // ==========FORMAL COMMENT=========
        // Returns the total number of upgrades purchased
        // Used by the route tracker to mark completed upgrades
        // ==========MY NOTES==============
        // Exposes how many total upgrades have been purchased
        // UI uses this to check off items in the route
        public int GetUpgradeCount()
        {
            Debug.WriteLine("Total Upgrades: " + totalUpgrades);
            return totalUpgrades;
        }

        // ==========FORMAL COMMENT=========
        // Returns details about which specific upgrades have been purchased
        // Provides upgrade-by-upgrade tracking for the route
        // ==========MY NOTES==============
        // More detailed than just a counter, shows exactly which upgrades are done
        // Helps track progress through the specific upgrade path
        public bool[] GetPurchasedUpgrades()
        {
            return upgradePurchased;
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
                ["Total Upgrades"] = GetUpgradeCount(),
                ["Modern Day Missions"] = modernDayMissions,

                // Game state
                ["Is Loading"] = IsLoading,
                ["Is Main Menu"] = IsMainMenu
            };
        }

        #endregion
    }
}

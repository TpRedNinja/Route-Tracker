using System;
using System.Collections.Generic;
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
        // ==========FORMAL COMMENT=========
        // Memory offset arrays and constants for accessing game statistics
        // Special cases (percent, percentFloat, forts) use unique memory paths
        // Most collectibles share a common base address and offset pattern with varying third offsets
        // ==========MY NOTES==============
        // These offsets are our map to find values in the game's memory
        // Most collectibles follow the same pattern 
        // only third offset changes by a step (which is how much it increases by)
        // 0x14 or 20 bytes if you are a nerd
        // A few values like completion percentage and forts need special handling
        private readonly int[] percentPtrOffsets = [0x284];
        private readonly int[] percentFtPtrOffsets = [0x74];
        private readonly int[] fortsPtrOffsets = [0x7F0, 0xD68, 0xD70, 0x30];
        private readonly int[] loadingPtrOffsets = [0x7D8];

        // Pre-calculated base address for most collectibles to avoid repeated calculations
        private readonly nint collectiblesBaseAddress;

        // ==========FORMAL COMMENT=========
        // Fields tracking completion of special activities detected through percentage changes
        // Counters increment when specific percentage thresholds or ranges are detected
        // Values are exposed through GetSpecialActivityCounts() for route tracking
        // ==========MY NOTES==============
        // These keep track of the special activities we complete
        // They get updated when the game percentage changes by specific amounts
        // The UI uses these values to check off items in the route
        private float lastPercentageValue = 0f;
        private int completedStoryMissions = 0;
        private int completedTemplarHunts = 0;
        private int defeatedLegendaryShips = 0;

        // ==========FORMAL COMMENT=========
        // Constants defining the exact percentage values for detecting special activities
        // Precise values and ranges identified through testing and game memory analysis
        // Used to determine which activity was completed based on percentage change
        // ==========MY NOTES==============
        // These are the magic percentage values that tell us what the player just did
        // Each activity type increases the completion percentage by a specific amount
        // Ranges account for slight variations that may occur due to rounding or other factors
        private const float LEGENDARY_SHIP_PERCENT = 0.18750f;
        private const float TEMPLAR_HUNT_MIN = 0.38579f;
        private const float TEMPLAR_HUNT_MAX = 0.38582f;
        private const float STORY_MISSION_MIN = 0.66666f;
        private const float STORY_MISSION_MAX = 1.66668f;
        //private const float DETECTION_THRESHOLD = 0.00001f;

        // ==========FORMAL COMMENT=========
        // Memory offset constants for locating collectible counters in game memory
        // Each collectible type has a unique third offset for pointer traversal
        // Values determined through memory scanning and pattern analysis
        // ==========MY NOTES==============
        // These tell us where to find each collectible counter in memory
        // Each type of collectible is stored at a different offset
        // Finding these took a lot of memory scanning and testing
        private const int ViewpointsThirdOffset = -0x1B30;
        private const int MyanThirdOffset = -0x1B1C;
        private const int TreasureThirdOffset = -0xBB8;
        private const int FragmentsThirdOffset = -0x1B58;
        private const int AssassinThirdOffset = -0xDD4;
        private const int NavalThirdOffset = 0x1950;
        private const int HeroUpgradeThirdOffset = -0x19F0; // Not used yet, but defined for future use
        private const int LettersThirdOffset = -0x04EC;
        private const int ManuscriptsThirdOffset = -0x334;
        private const int MusicThirdOffset = 0x424;

        // ==========FORMAL COMMENT=========
        // Common offset constants used in pointer paths for collectibles
        // Shared first, second, and last offsets in the memory traversal pattern
        // ==========MY NOTES==============
        // Most collectibles follow this same pattern of memory addresses
        // This makes it easier to find all the different types of items
        private const int FirstOffset = 0x2D0;
        private const int SecondOffset = 0x8BC;
        private const int LastOffset = 0x18;
        private const int OffsetStep = 0x14;

        // ==========FORMAL COMMENT=========
        // Memory offset ranges for chest and tavern collectibles
        // Defines the start and end offsets for iterating through these collections
        // ==========MY NOTES==============
        // Chests and taverns are stored differently - as individual flags
        // These ranges tell us where all those individual flags are located
        private const int ChestStartOffset = 0x67C;
        private const int ChestEndOffset = 0xA8C;
        private const int TavernStartOffset = 0x319C;
        private const int TavernEndOffset = 0x3228;

        // ==========FORMAL COMMENT=========
        // Memory offsets for resource expenditure tracking
        // These point to counters that track total resources spent
        // ==========MY NOTES==============
        // These track how much money, wood, and metal have been spent
        // Help detect specific upgrade purchases based on resource use patterns
        private const int RESOURCES_FIRST_OFFSET = 0x104;
        private readonly int[] moneySpentOffsets = [0xB0, 0xC, 0x58, 0x26C];
        private readonly int[] woodSpentOffsets = [0xA0, 0x3C, 0x80, 0x10C];
        private readonly int[] metalSpentOffsets = [0xB8, 0x4, 0x28, 0xAC];

        // Base address for resource tracking
        private readonly nint resourcesBaseAddress;

        // Fields for tracking resources and upgrade progress
        private int lastMoneySpent = 0;
        private int lastWoodSpent = 0;
        private int lastMetalSpent = 0;
        private int lastHeroUpgradeValue = 0;
        private int totalUpgrades = 0;

        // Upgrade tracking - each entry corresponds to upgrades in the route
        private readonly bool[] upgradePurchased = new bool[38]; // 38 total upgrades in the route

        // ==========FORMAL COMMENT=========
        // Initializes a new game statistics tracker for Assassin's Creed 4
        // Configures memory access and calculates the base address for collectibles
        // ==========MY NOTES==============
        // Sets up our tracker to read AC4's memory
        // Calculates the starting point for finding all the collectibles
        public AC4GameStats(IntPtr processHandle, IntPtr baseAddress)
    : base(processHandle, baseAddress)
        {
            this.collectiblesBaseAddress = (nint)baseAddress + 0x026BEAC0;
            this.resourcesBaseAddress = (nint)baseAddress + 0x00E88810;
        }

        #region Public Methods

        // ==========FORMAL COMMENT=========
        // Retrieves current game statistics by reading from multiple memory locations
        // Collects data on completion percentage, collectibles, and other trackable items
        // Also processes percentage changes to detect special activities
        // ==========MY NOTES==============
        // The main method that gets all the stats from the game
        // Reads everything from memory: percentage, collectibles, etc.
        // Also checks if you've completed any special activities
        public override (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure,
            int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music,
            int Forts, int Taverns, int TotalChests) GetStats()
        {
            // Reading collectibles using existing methods
            int percent = Read<int>((nint)baseAddress + 0x49D9774, percentPtrOffsets);
            float percentFloat = Read<float>((nint)baseAddress + 0x049F1EE8, percentFtPtrOffsets);
            int forts = Read<int>((nint)baseAddress + 0x026C0A28, fortsPtrOffsets);
            bool loading = Read<bool>((nint)baseAddress + 0x04A1A6CC, loadingPtrOffsets);

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

            // Detect percentage-based activities
            HandlePercentageCases(percentFloat, loading);

            // Detect upgrades using the resource values
            HandleUpgradeCases(heroupgrades, moneySpent, woodSpent, metalSpent);

            // Return all the stats (including the basic ones that we got from memory)
            return (percent, percentFloat, viewpoints, myan, treasure, fragments, assassin, naval,
                letters, manuscripts, music, forts, taverns, totalChests);
        }

        // ==========FORMAL COMMENT=========
        // Returns the current counters for special activities detected through percentage changes
        // Provides access to story mission, templar hunt, and legendary ship completion counts
        // Used by route tracking system to mark these activities as completed
        // ==========MY NOTES==============
        // Tells the route tracker how many special activities we've completed
        // This is how the UI knows when to check off story missions and legendary ships
        // Returns a tuple with all three counter values at once
        public (int StoryMissions, int TemplarHunts, int LegendaryShips) GetSpecialActivityCounts()
        {
            return (completedStoryMissions, completedTemplarHunts, defeatedLegendaryShips);
        }

        // ==========FORMAL COMMENT=========
        // Returns the total number of upgrades purchased
        // Used by the route tracker to mark completed upgrades
        // ==========MY NOTES==============
        // Exposes how many total upgrades have been purchased
        // UI uses this to check off items in the route
        public int GetUpgradeCount()
        {
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

        #endregion

        #region Private Helper Methods

        // ==========FORMAL COMMENT=========
        // Helper method to read resource expenditure values
        // Follows pointer paths to track spent resources
        // ==========MY NOTES==============
        // Similar to ReadCollectible but for resources spent
        // Combines the shared first offset with resource-specific offsets
        private int ReadResourceSpent(int[] uniqueOffsets)
        {
            // Combine the shared first offset with resource-specific offsets
            int[] fullOffsets = new int[uniqueOffsets.Length + 1];
            fullOffsets[0] = RESOURCES_FIRST_OFFSET;
            Array.Copy(uniqueOffsets, 0, fullOffsets, 1, uniqueOffsets.Length);

            try
            {
                return Read<int>(resourcesBaseAddress, fullOffsets);
            }
            catch (Exception)
            {
                return 0; // Return 0 if reading fails
            }
        }

        // ==========FORMAL COMMENT=========
        // Helper method to read collectibles using shared memory pattern
        // Uses common base address and offset structure with specific third offset
        // ==========MY NOTES==============
        // Simplifies reading collectibles that follow the standard pattern
        // Makes the code cleaner by removing duplicated memory reading logic
        private int ReadCollectible(int thirdOffset)
        {
            return Read<int>(collectiblesBaseAddress, [FirstOffset, SecondOffset, thirdOffset, LastOffset]);
        }

        // ==========FORMAL COMMENT=========
        // Helper method to count collectibles that are stored as individual flags
        // Iterates through a range of third offsets and sums their values
        // ==========MY NOTES==============
        // Used for things like chests and taverns that have many individual locations
        // Each location has its own memory address with a predictable pattern
        private int CountCollectibles(int startOffset, int endOffset)
        {
            int count = 0;
            for (int thirdOffset = startOffset; thirdOffset <= endOffset; thirdOffset += OffsetStep)
            {
                count += ReadCollectible(thirdOffset);
            }
            return count;
        }

        // ==========FORMAL COMMENT=========
        // Detects special game activities based on percentage changes
        // Identifies legendary ships, templar hunts, story missions, and other collectibles
        // Prevents false detections during loading screens
        // ==========MY NOTES==============
        // Watches for specific percentage increases that happen when completing activities
        // Updates the baseline after each detection to catch rapid sequential completions
        // Has special handling for main activities and a catch-all for other collectibles
        private void HandlePercentageCases(float currentPercentage, bool isLoading)
        {
            // Round currentPercentage to 5 decimal places
            currentPercentage = (float)Math.Round(currentPercentage, 5);

            // Only process if percentage changed and we're not in a loading screen
            if (currentPercentage != lastPercentageValue && isLoading == false)
            {
                // Calculate the change since last reading
                float percentageDelta = currentPercentage - lastPercentageValue;

                // Detect legendary ships using EXACT equality
                if (currentPercentage == lastPercentageValue + LEGENDARY_SHIP_PERCENT)
                {
                    defeatedLegendaryShips ++;
                    // Update immediately after detection
                    lastPercentageValue = currentPercentage;
                }
                // Detect Templar hunts (within range)
                else if (percentageDelta >= TEMPLAR_HUNT_MIN && percentageDelta <= TEMPLAR_HUNT_MAX)
                {
                    completedTemplarHunts ++;
                    // Update immediately after detection
                    lastPercentageValue = currentPercentage;
                }
                // Detect story missions (within broader range)
                else if (percentageDelta >= STORY_MISSION_MIN && percentageDelta <= STORY_MISSION_MAX)
                {
                    completedStoryMissions ++;
                    // Update immediately after detection
                    lastPercentageValue = currentPercentage;
                }
                // Catch-all for other collectibles that don't match specific patterns
                else if (percentageDelta > 0)
                {
                    // Just update the baseline without specific tracking
                    lastPercentageValue = currentPercentage;
                }
            }
        }

        // Implement HandleUpgradeCases:
        private void HandleUpgradeCases(int currentHeroUpgrade, int moneySpent, int woodSpent, int metalSpent)
        {
            // Debug info
            if (moneySpent != lastMoneySpent || woodSpent != lastWoodSpent || metalSpent != lastMetalSpent)
            {
                Debug.WriteLine($"Resource changes - Money: {moneySpent - lastMoneySpent}, " +
                                $"Wood: {woodSpent - lastWoodSpent}, Metal: {metalSpent - lastMetalSpent}");
            }

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
                        case 1: upgradeIndex = 0; break;  // Pistol Holster 2/Health 1
                        case 2: upgradeIndex = 1; break;  // Pistol Holster 2/Health 1 (second option)
                        case 3: upgradeIndex = 27; break; // Pistol Holster 3
                        case 4: upgradeIndex = 28; break; // Pistol Holster 4
                        case 5: upgradeIndex = 29; break; // Smoke Bomb Pouch 1
                        case 6: upgradeIndex = 30; break; // Smoke Bomb Pouch 2
                        case 7: upgradeIndex = 31; break; // Dart Pouch 1
                        case 8: upgradeIndex = 32; break; // Dart Pouch 2
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
                // Check for specific upgrade patterns based on resource costs
                // Hull 1 (upgrade #3) - 1000 money
                if (moneyDelta >= 1000 && !upgradePurchased[2])
                {
                    upgradePurchased[2] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Hull 1 upgrade");
                }

                // Round Shot Strength 1 (upgrade #4) - 900 money
                else if (moneyDelta >= 900 && !upgradePurchased[3])
                {
                    upgradePurchased[3] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Round Shot Strength 1 upgrade");
                }

                // Round Shot Strength 2 (upgrade #5) - 4000 money
                else if (moneyDelta >= 4000 && !upgradePurchased[4])
                {
                    upgradePurchased[4] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Round Shot Strength 2 upgrade");
                }

                // Mortar 1 (upgrade #6) - 800 money
                else if (moneyDelta >= 800 && !upgradePurchased[5])
                {
                    upgradePurchased[5] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Mortar 1 upgrade");
                }

                // Heavy Shot 1 (upgrade #7) - 900 money
                else if (moneyDelta >= 900 && !upgradePurchased[6])
                {
                    upgradePurchased[6] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Heavy Shot 1 upgrade");
                }

                // Heavy Shot 2 (upgrade #8) - 6000 money
                else if (moneyDelta >= 6000 && !upgradePurchased[7])
                {
                    upgradePurchased[7] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Heavy Shot 2 upgrade");
                }

                // Mortar Storage 1 (upgrade #9) - 800 money
                else if (moneyDelta >= 800 && !upgradePurchased[8])
                {
                    upgradePurchased[8] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Mortar Storage 1 upgrade");
                }

                // Mortar Storage 2 (upgrade #10) - 2000 money
                else if (moneyDelta >= 2000 && !upgradePurchased[9])
                {
                    upgradePurchased[9] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Mortar Storage 2 upgrade");
                }

                // Cannons 1 (upgrade #11) - 70 metal
                else if (metalDelta >= 70 && !upgradePurchased[10])
                {
                    upgradePurchased[10] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Cannons 1 upgrade");
                }

                // Round Shot Strength 3 (upgrade #12) - 12000 money
                else if (moneyDelta >= 12000 && !upgradePurchased[11])
                {
                    upgradePurchased[11] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Round Shot Strength 3 upgrade");
                }

                // Chain Shot Strength 1 (upgrade #13) - 2500 money
                else if (moneyDelta >= 2500 && !upgradePurchased[12])
                {
                    upgradePurchased[12] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Chain Shot Strength 1 upgrade");
                }

                // Chain Shot Strength 2 (upgrade #14) - 6000 money
                else if (moneyDelta >= 6000 && !upgradePurchased[13])
                {
                    upgradePurchased[13] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Chain Shot Strength 2 upgrade");
                }

                // Fire Barrel Strength 1 (upgrade #15) - 3000 money
                else if (moneyDelta >= 3000 && !upgradePurchased[14])
                {
                    upgradePurchased[14] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Fire Barrel Strength 1 upgrade");
                }

                // Heavy Shot Storage 1 (upgrade #16) - 500 money
                else if (moneyDelta >= 500 && !upgradePurchased[15])
                {
                    upgradePurchased[15] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Heavy Shot Storage 1 upgrade");
                }

                // Heavy Shot Storage 2 (upgrade #17) - 1500 money
                else if (moneyDelta >= 1500 && !upgradePurchased[16])
                {
                    upgradePurchased[16] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Heavy Shot Storage 2 upgrade");
                }

                // Mortar 2 (upgrade #18) - 3500 money, 200 metal
                else if (moneyDelta >= 3500 && metalDelta >= 200 && !upgradePurchased[17])
                {
                    upgradePurchased[17] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Mortar 2 upgrade");
                }

                // Swivel Strength 1 (upgrade #19) - 700 money
                else if (moneyDelta >= 700 && !upgradePurchased[18])
                {
                    upgradePurchased[18] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Swivel Strength 1 upgrade");
                }

                // Officers Rapiers (upgrade #20) - 14000 money
                else if (moneyDelta >= 14000 && !upgradePurchased[19])
                {
                    upgradePurchased[19] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Officers Rapiers upgrade");
                }

                // Cannon-Barrel Pistols (upgrade #21) - 9000 money
                else if (moneyDelta >= 9000 && !upgradePurchased[20])
                {
                    upgradePurchased[20] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Cannon-Barrel Pistols upgrade");
                }

                // Rabbit Pelt (upgrade #22) - 1400 money (700 × 2)
                else if (moneyDelta >= 1400 && !upgradePurchased[21])
                {
                    upgradePurchased[21] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Rabbit Pelt upgrade");
                }

                // Hutia Pelt (upgrade #23) - 1700 money (850 × 2)
                else if (moneyDelta >= 1700 && !upgradePurchased[22])
                {
                    upgradePurchased[22] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Hutia Pelt upgrade");
                }

                // Howler Monkey Skin (upgrade #24) - 2000 money (1000 × 2)
                else if (moneyDelta >= 2000 && !upgradePurchased[23])
                {
                    upgradePurchased[23] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Howler Monkey Skin upgrade");
                }

                // Crocodile Leather (upgrade #25) - 4000 money (2000 × 2)
                else if (moneyDelta >= 4000 && !upgradePurchased[24])
                {
                    upgradePurchased[24] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Crocodile Leather upgrade");
                }

                // Killer Whale Skin (upgrade #26) - 6300 money (3150 × 2)
                else if (moneyDelta >= 6300 && !upgradePurchased[25])
                {
                    upgradePurchased[25] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Killer Whale Skin upgrade");
                }

                // Humpback Whale Skin (upgrade #27) - 7000 money (3500 × 2)
                else if (moneyDelta >= 7000 && !upgradePurchased[26])
                {
                    upgradePurchased[26] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Humpback Whale Skin upgrade");
                }

                // Note: Hero upgrades #28-33 are handled earlier in the hero upgrade detection section

                // Hull Armor 2 (upgrade #34) - 4000 money, 100 metal, 200 wood
                else if (moneyDelta >= 4000 && metalDelta >= 100 && woodDelta >= 200 && !upgradePurchased[33])
                {
                    upgradePurchased[33] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Hull Armor 2 upgrade");
                }

                // Diving Bell (upgrade #35) - 5000 money
                else if (moneyDelta >= 5000 && !upgradePurchased[34])
                {
                    upgradePurchased[34] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Diving Bell upgrade");
                }

                // Mortar Storage 3 (upgrade #36) - 5000 money
                else if (moneyDelta >= 5000 && !upgradePurchased[35])
                {
                    upgradePurchased[35] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Mortar Storage 3 upgrade");
                }

                // Ram Strength 1 (upgrade #37) - 500 money, 25 wood
                else if (moneyDelta >= 500 && woodDelta >= 25 && !upgradePurchased[36])
                {
                    upgradePurchased[36] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Ram Strength 1 upgrade");
                }

                // Ram Strength 2 (upgrade #38) - 5000 money, 150 metal, 250 wood
                else if (moneyDelta >= 5000 && metalDelta >= 150 && woodDelta >= 250 && !upgradePurchased[37])
                {
                    upgradePurchased[37] = true;
                    totalUpgrades++;
                    Debug.WriteLine("Detected Ram Strength 2 upgrade");
                }

                // Update the last spent values
                lastMoneySpent = moneySpent;
                lastWoodSpent = woodSpent;
                lastMetalSpent = metalSpent;
            }
        }
        #endregion
    }
}

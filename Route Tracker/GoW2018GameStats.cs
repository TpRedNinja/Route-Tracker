using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Policy;

namespace Route_Tracker
{
    public unsafe class GoW2018GameStats : GameStatsBase
    {
        // Pre-calculated base address for most collectibles to avoid repeated calculations
        private readonly nint collectiblesBaseAddress;
        private readonly nint ObjBaseAddress;
        private readonly nint LoadBaseAddress;
        private readonly nint MenuBaseAddress;

        private readonly int[] noOffsets = [];

        private bool isLoading = false;
        private bool isMainMenu = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public GoW2018GameStats(IntPtr processHandle, IntPtr baseAddress)
    :   base(processHandle, baseAddress)
        {
            // Will be properly implemented later
            this.collectiblesBaseAddress = (nint)baseAddress + 0x014261C0;
            this.ObjBaseAddress = (nint)baseAddress + 0x22C6904;
            this.LoadBaseAddress = (nint)baseAddress + 0x22E9DB0;
            this.MenuBaseAddress = (nint)baseAddress + 0x22E9DB4;
            //Debug.WriteLine("GoW2018GameStats initialized - actual implementation pending");
        }

        // Method to read game stats for God of War 2018
        public (int objective, int load, int mainmenu, int skapslag, 
        int anchor, int artifacts, int helmets, int muspel, int nifl, int orl,
        int rtl, int tooth) GetStatsGOW2018()
        {
            int objective = ReadWithCache64Bit<int>("Objective", this.ObjBaseAddress, noOffsets);
            int load = ReadWithCache64Bit<int>("Load", this.LoadBaseAddress, noOffsets);
            int mainmenu = ReadWithCache64Bit<int>("MainMenu", this.MenuBaseAddress, noOffsets);

            DetectStatuses(mainmenu, load);

            // For now, return zeros for everything
            //Debug.WriteLine("GoW2018GameStats.GetStats() called - not yet implemented");
            Debug.WriteLine($"GoW Objective Value: {objective}");
            return (objective, load, mainmenu, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private void DetectStatuses(int mainmenu, int loading)
        {
            if (loading > 0)
            {
                isLoading = true;
            }
            else
            {
                isLoading = false;
            }

            if (mainmenu == 1)
            {
                isMainMenu = true;
            }
            else
            {
                isMainMenu = false;
            }
        }

        // Optionally override GetStatsAsDictionary to provide GoW-specific labels
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059",
        Justification = "NO")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0079",
        Justification = "because i said so")]
        public override Dictionary<string, object> GetStatsAsDictionary()
        {
            var (objective, load, mainmenu, skapslag, anchor, artifacts, helmets, muspel, nifl, orl, rtl, tooth) = GetStatsGOW2018();
            var (IsLoading, IsMainMenu) = GetGameStatus();

            return new Dictionary<string, object>
            {
                ["Objective"] = objective,

                ["Is Loading"] = IsLoading,
                ["Is Main Menu"] = IsMainMenu
            };
        }

        public override (bool IsLoading, bool IsMainMenu) GetGameStatus()
        {
            // Return the current loading and main menu status
            return (isLoading, isMainMenu);
        }
    }
}
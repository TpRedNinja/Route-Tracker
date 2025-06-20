using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Assassin_s_Creed_Route_Tracker
{
    // ==========FORMAL COMMENT=========
    // Class that interfaces with game memory to read player statistics
    // Handles memory address calculation and data extraction for all tracked stats
    // ==========MY NOTES==============
    // This reads all the game stats from memory by using addresses and offsets
    // It's the core piece that makes the tracker actually work
    public unsafe class GameStats
    {
        private readonly IntPtr processHandle;
        private readonly IntPtr baseAddress;

        // ==========FORMAL COMMENT=========
        // Memory offset arrays for various game statistics
        // Each array represents the chain of offsets needed to reach the target memory value
        // ==========MY NOTES==============
        // These are all the memory paths to find different stats
        // Each game value requires following a trail of memory addresses
        private readonly int[] percentPtrOffsets = [0x284];
        private readonly int[] percentFtPtrOffsets = [0x74];
        private readonly int[] viewpointsPtrOffsets = [0xC0, 0x5C8, 0x18];
        private readonly int[] myanPtrOffsets = [0xC0, 0x5DC, 0x18];
        private readonly int[] treasurePtrOffsets = [0x2D0, 0x85C, 0x690, 0x18];
        private readonly int[] fragmentsPtrOffsets = [0x520, 0xB14, 0xF78, 0x18];
        private readonly int[] assassinPtrOffsets = [0x2D0, 0x858, 0x488, 0x18];
        private readonly int[] navalPtrOffsets = [0xD5C, 0x968, 0x410, 0x18];
        private readonly int[] lettersPtrOffsets = [0x2D0, 0x86C, 0xD0C, 0x18];
        private readonly int[] manuscriptsPtrOffsets = [0x320, 0x100, 0x780, 0x18];
        private readonly int[] musicPtrOffsets = [0x320, 0x100, 0xED8, 0x18];
        private readonly int[] fortsPtrOffsets = [0x9A0, 0x3B8, 0x140, 0x30];
        private readonly int[] tavernsPtrOffsets = [0x988, 0xD60, 0x438, 0x30];

        //Constants for the chest pointers
        private const int FirstOffset = 0x2D0;
        private const int SecondOffset = 0x8BC;
        private const int LastOffset = 0x18;
        private const int OffsetStep = 0x14;
        private const int StartOffset = 0x67C;
        private const int EndOffset = 0xA8C;

        /*
        Commented all this out in case what im about to do doesnt work
        private readonly int[][] chestPtrOffsets =
        [
                //Dry Tortuga Chests
                [0x2D0, 0x8BC, 0x744, 0x18], // Havana
                [0x2D0, 0x884, 0xB90, 0x18], // CapeBatavistia
                [0x2D0, 0x8A4, 0xA3C, 0x18], // DryTortuga
                [0x2D0, 0x8BC, 0x898, 0x18], // SaltKey
                [0x2D0, 0x8BC, 0x80C, 0x18], // Matanzas
                [0x2D0, 0x884, 0xBA4, 0x18], // Flordia
                //Eluethra Chests
                [0x2D0, 0x8A0, 0x8D4, 0x18], // Nassua
                [0x2D0, 0x8B8, 0x9EC, 0x18], // Eleuthra
                [0x2D0, 0x87C, 0x910, 0x18], // Andreas
                [0x2D0, 0x8BC, 0x6CC, 0x18], // Cat
                [0x2D0, 0x87C, 0x7D0, 0x18], // AbacoIsland
                //Gibra Chests
                [0x2D0, 0x8AC, 0x7A8, 0x18], // Hideout
                [0x2D0, 0x8BC, 0x9EC, 0x18], // Gibra
                [0x2D0, 0x884, 0x834, 0x18], // Crooked
                [0x2D0, 0x8A0, 0x820, 0x18], // Jiguey
                [0x2D0, 0x87C, 0x938, 0x18], // Mariguana
                [0x2D0, 0x874, 0xA14, 0x18], // SaltLagoon
                //Punta Guarico Chests
                [0x2D0, 0x8BC, 0x884, 0x18], // Principe
                [0x2D0, 0x8BC, 0xA14, 0x18], // Punta
                [0x2D0, 0x8B4, 0x960, 0x18], // Tortuga
                [0x2D0, 0x8B4, 0x85C, 0x18], // Petite
                [0x2D0, 0x8B0, 0x820, 0x18], // Cumberland
                //Conttoyor Chests
                [0x2D0, 0x8B4, 0x730, 0x18], // Tulum
                [0x2D0, 0x8B8, 0x9B0, 0x18], // Conttoyor
                //Navassa Chests
                [0x2D0, 0x8BC, 0xA00, 0x18], // Navassa
                [0x2D0, 0x8A8, 0x7D0, 0x18], // IlleAVache
                //Charlotte Chests
                [0x2D0, 0x8BC, 0x7BC, 0x18], // Kingston
                [0x2D0, 0x88C, 0x960, 0x18], // Observatory
                [0x2D0, 0x8AC, 0x9C4, 0x18], // Charlotte
                [0x2D0, 0x8BC, 0x6B8, 0x18], // Annatto
                //Serranillia Chests
                [0x2D0, 0x880, 0x8AC, 0x18], // Isla
                [0x2D0, 0x8AC, 0xA78, 0x18], // Serranillia
                [0x2D0, 0x8A4, 0x898, 0x18], // Misteriosa
                [0x2D0, 0x8BC, 0x85C, 0x18], // NewBone
                //Chinchorro Chests
                [0x2D0, 0x8BC, 0x988, 0x18], // Chinchorro
                [0x2D0, 0x8BC, 0x8C0, 0x18], // Santanillas
                [0x2D0, 0x898, 0x7A8, 0x18], // Corozal
                [0x2D0, 0x8B8, 0x690, 0x18], // Ambergis
                //Castillo De Jagua Chests
                [0x2D0, 0x8A8, 0x9C4, 0x18], // Castillo
                [0x2D0, 0x8A8, 0x80C, 0x18], // Pinos
                [0x2D0, 0x8BC, 0x6A4, 0x18], // Arrayos
                [0x2D0, 0x894, 0x7A8, 0x18], // Cayman
                //Cruz Chests
                [0x2D0, 0x884, 0xAC8, 0x18], // Cruz
                [0x2D0, 0x8BC, 0x730, 0x18], // SanJuan
                [0x2D0, 0x8BC, 0x924, 0x18], // GrandCayman
            ];

        private readonly int[] waterChestsBaseAddress = [0x30C, 0x58C, 0x18]; //waterchests
        private readonly int[] unchartedChestsBaseAddress = [0x2E8, 0x8BC, 0x94C, 0x18]; //unchartedchests
        */

        // ==========FORMAL COMMENT=========
        // Initializes a new GameStats instance with process handle and base address
        // Stores references needed to access game memory later
        // ==========MY NOTES==============
        // Sets up the object with the info needed to read game memory
        public GameStats(IntPtr processHandle, IntPtr baseAddress)
        {
            this.processHandle = processHandle;
            this.baseAddress = baseAddress;
        }

        // ==========FORMAL COMMENT=========
        // Retrieves all game statistics from memory in a single operation
        // Returns a tuple containing all available game progress metrics
        // ==========MY NOTES==============
        // Reads all the different stats at once and returns them as one package
        // This is the main method that gets called when displaying stats
        public (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure, int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music, int Forts, int Taverns, int TotalChests) GetStats()
        {
            int percent = Read<int>((nint)baseAddress + 0x49D9774, percentPtrOffsets);
            float percentfloat = Read<float>((nint)baseAddress + 0x049F1EE8, percentFtPtrOffsets);
            int viewpoints = Read<int>((nint)baseAddress + 0x026BEC04, viewpointsPtrOffsets);
            int myan = Read<int>((nint)baseAddress + 0x026BEC04, myanPtrOffsets);
            int treasure = Read<int>((nint)baseAddress + 0x026BEAC0, treasurePtrOffsets);
            int fragments = Read<int>((nint)baseAddress + 0x026BE520, fragmentsPtrOffsets);
            int assassin = Read<int>((nint)baseAddress + 0x026BEAC0, assassinPtrOffsets);
            int naval = Read<int>((nint)baseAddress + 0x00453734, navalPtrOffsets);
            int letters = Read<int>((nint)baseAddress + 0x026BEAC0, lettersPtrOffsets);
            int manuscripts = Read<int>((nint)baseAddress + 0x026BEC04, manuscriptsPtrOffsets);
            int music = Read<int>((nint)baseAddress + 0x026BEC04, musicPtrOffsets);
            int forts = Read<int>((nint)baseAddress + 0x026BE51C, fortsPtrOffsets);
            int taverns = Read<int>((nint)baseAddress + 0x026BE51C, tavernsPtrOffsets);

            int totalChests = 0;

            //for loop automatically adding offsets and reading stuff
            for (int ThirdOffset = StartOffset; ThirdOffset <= EndOffset; ThirdOffset += OffsetStep)
            {
                totalChests += Read<int>((nint)baseAddress + 0x026BEAC0, [FirstOffset, SecondOffset, ThirdOffset, LastOffset]);
            }

            return (percent, percentfloat, viewpoints, myan, treasure, fragments, assassin, naval, letters, manuscripts, music, forts, taverns, totalChests);
        }

        // ==========FORMAL COMMENT=========
        // Generic method to read values from game memory
        // Follows chains of pointers using the provided offsets
        // ==========MY NOTES==============
        // This is the low-level function that actually reads from memory
        // It follows the trail of addresses to find the specific value we want
        private unsafe T Read<T>(nint baseAddress, int[] offsets) where T : unmanaged
        {
            nint address = baseAddress;
            Debug.WriteLine($"Reading memory at base address: {baseAddress:X}");

            int pointer_size = 4;
            foreach (int offset in offsets)
            {
                if (!ReadProcessMemory(processHandle, address, &address, pointer_size, out nint bytesReads) || bytesReads != pointer_size || address == IntPtr.Zero)
                {
                    Debug.WriteLine($"Failed to read memory address at offset {offset:X}");
                    return default;
                }
                address += offset;
                Debug.WriteLine($"Address after applying offset {offset:X}: {address:X}");
            }

            T value;
            int size = sizeof(T);
            if (!ReadProcessMemory(processHandle, address, &value, size, out nint bytesRead) || bytesRead != size)
            {
                Debug.WriteLine($"Failed to read value from memory at address {address:X}");
                return default;
            }

            Debug.WriteLine($"Read value from memory at address {address:X}: {value}");
            return value;
        }

        // ==========FORMAL COMMENT=========
        // Windows API import for reading data from the memory of another process
        // Required for accessing the game's memory space
        // ==========MY NOTES==============
        // This is the Windows function that lets us peek into the game's memory
        // Without this, we couldn't read any stats
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            nint lpBaseAddress,
            void* lpBuffer,
            nint nSize,
            out nint lpNumberOfBytesRead);
    }
}


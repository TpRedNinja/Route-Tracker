using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assassin_s_Creed_Route_Tracker
{
    public unsafe class GameStats
    {
        private IntPtr processHandle;
        private IntPtr baseAddress;

        private int[] percentPtrOffsets = { 0x284 };
        private int[] percentFtPtrOffsets = { 0x74 };
        private int[] viewpointsPtrOffsets = { 0xC0, 0x5C8, 0x18 };
        private int[] myanPtrOffsets = { 0xC0, 0x5DC, 0x18 };
        private int[] treasurePtrOffsets = { 0x2D0, 0x85C, 0x690, 0x18 };
        private int[] fragmentsPtrOffsets = { 0x520, 0xB14, 0xF78, 0x18 };
        private int[] assassinPtrOffsets = { 0x2D0, 0x858, 0x488, 0x18 };
        private int[] navalPtrOffsets = { 0xD5C, 0x968, 0x410, 0x18 };
        private int[] lettersPtrOffsets = { 0x2D0, 0x86C, 0xD0C, 0x18 };
        private int[] manuscriptsPtrOffsets = { 0x320, 0x100, 0x780, 0x18 };
        private int[] musicPtrOffsets = { 0x320, 0x100, 0xED8, 0x18 };
        private int[] fortsPtrOffsets = { 0x9A0, 0x3B8, 0x140, 0x30 };
        private int[] tavernsPtrOffsets = { 0x988, 0xD60, 0x438, 0x30 };

        private int[][] chestPtrOffsets =
        {
                new int[] { 0x2D0, 0x8BC, 0x744, 0x18 }, // Havana
                new int[] { 0x2D0, 0x884, 0xB90, 0x18 }, // CapeBatavistia
                new int[] { 0x2D0, 0x8A4, 0xA3C, 0x18 }, // DryTortuga
                new int[] { 0x2D0, 0x8BC, 0x898, 0x18 }, // SaltKey
                new int[] { 0x2D0, 0x8BC, 0x80C, 0x18 }, // Matanzas
                new int[] { 0x2D0, 0x884, 0xBA4, 0x18 }, // Flordia
                new int[] { 0x2D0, 0x8A0, 0x8D4, 0x18 }, // Nassua
                new int[] { 0x2D0, 0x8B8, 0x9EC, 0x18 }, // Eleuthra
                new int[] { 0x2D0, 0x87C, 0x910, 0x18 }, // Andreas
                new int[] { 0x2D0, 0x8BC, 0x6CC, 0x18 }, // Cat
                new int[] { 0x2D0, 0x87C, 0x7D0, 0x18 }, // AbacoIsland
                new int[] { 0x2D0, 0x8AC, 0x7A8, 0x18 }, // Hideout
                new int[] { 0x2D0, 0x8BC, 0x9EC, 0x18 }, // Gibra
                new int[] { 0x2D0, 0x884, 0x834, 0x18 }, // Crooked
                new int[] { 0x2D0, 0x8A0, 0x820, 0x18 }, // Jiguey
                new int[] { 0x2D0, 0x87C, 0x938, 0x18 }, // Mariguana
                new int[] { 0x2D0, 0x874, 0xA14, 0x18 }, // SaltLagoon
                new int[] { 0x2D0, 0x8BC, 0x884, 0x18 }, // Principe
                new int[] { 0x2D0, 0x8BC, 0xA14, 0x18 }, // Punta
                new int[] { 0x2D0, 0x8B4, 0x960, 0x18 }, // Tortuga
                new int[] { 0x2D0, 0x8B4, 0x85C, 0x18 }, // Petite
                new int[] { 0x2D0, 0x8B0, 0x820, 0x18 }, // Cumberland
                new int[] { 0x2D0, 0x8B4, 0x730, 0x18 }, // Tulum
                new int[] { 0x2D0, 0x8B8, 0x9B0, 0x18 }, // Conttoyor
                new int[] { 0x2D0, 0x8BC, 0xA00, 0x18 }, // Navassa
                new int[] { 0x2D0, 0x8A8, 0x7D0, 0x18 }, // IlleAVache
                new int[] { 0x2D0, 0x8BC, 0x7BC, 0x18 }, // Kingston
                new int[] { 0x2D0, 0x88C, 0x960, 0x18 }, // Observatory
                new int[] { 0x2D0, 0x8AC, 0x9C4, 0x18 }, // Charlotte
                new int[] { 0x2D0, 0x8BC, 0x6B8, 0x18 }, // Annatto
                new int[] { 0x2D0, 0x880, 0x8AC, 0x18 }, // Isla
                new int[] { 0x2D0, 0x8AC, 0xA78, 0x18 }, // Serranillia
                new int[] { 0x2D0, 0x8A4, 0x898, 0x18 }, // Misteriosa
                new int[] { 0x2D0, 0x8BC, 0x85C, 0x18 }, // NewBone
                new int[] { 0x2D0, 0x8BC, 0x988, 0x18 }, // Chinchorro
                new int[] { 0x2D0, 0x8BC, 0x8C0, 0x18 }, // Santanillas
                new int[] { 0x2D0, 0x898, 0x7A8, 0x18 }, // Corozal
                new int[] { 0x2D0, 0x8B8, 0x690, 0x18 }, // Ambergis
                new int[] { 0x2D0, 0x8A8, 0x9C4, 0x18 }, // Castillo
                new int[] { 0x2D0, 0x8A8, 0x80C, 0x18 }, // Pinos
                new int[] { 0x2D0, 0x8BC, 0x6A4, 0x18 }, // Arrayos
                new int[] { 0x2D0, 0x894, 0x7A8, 0x18 }, // Cayman
                new int[] { 0x2D0, 0x884, 0xAC8, 0x18 }, // Cruz
                new int[] { 0x2D0, 0x8BC, 0x730, 0x18 }, // SanJuan
                new int[] { 0x2D0, 0x8BC, 0x924, 0x18 }, // GrandCayman
            };

        private int[] waterChestsBaseAddress = { 0x30C, 0x58C, 0x18 }; //waterchests
        private int[] unchartedChestsBaseAddress = { 0x2E8, 0x8BC, 0x94C, 0x18 }; //unchartedchests

        public GameStats(IntPtr processHandle, IntPtr baseAddress)
        {
            this.processHandle = processHandle;
            this.baseAddress = baseAddress;
        }

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
            totalChests += Read<int>((nint)baseAddress + 0x026BEC04, waterChestsBaseAddress);
            totalChests += Read<int>((nint)baseAddress + 0x026BEAB8, unchartedChestsBaseAddress);

            foreach (var chestOffsets in chestPtrOffsets)
            {
                totalChests += Read<int>((nint)baseAddress + 0x026BEAC0, chestOffsets);
            }

            return (percent, percentfloat, viewpoints, myan, treasure, fragments, assassin, naval, letters, manuscripts, music, forts, taverns, totalChests);
        }

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


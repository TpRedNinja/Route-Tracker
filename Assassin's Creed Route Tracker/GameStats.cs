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

        private int[] percentPtrOffsets = {0x284};
        private int[] percentFtPtrOffsets = { 0x74 };
        private int[] viewpointsPtrOffsets = { 0x1A8, 0x28, 0x18 };
        private int[] myanPtrOffsets = { 0x1A8, 0x3C, 0x18 };
        private int[] treasurePtrOffsets = { 0x1A8, 0xFA0, 0x18 };
        private int[] fragmentsPtrOffsets = { 0x1A8, 0x0, 0x18 };
        private int[] assassinPtrOffsets = { 0x1A8, 0xD84, 0x18 };
        private int[] navalPtrOffsets = { 0xD50 };
        private int[] lettersPtrOffsets = { 0x450, 0x0, 0x18 };
        private int[] manuscriptsPtrOffsets = { 0x7C0, 0x0, 0x18 };
        private int[] musicPtrOffsets = { 0xB08, 0x424, 0x18 };
        private int[] fortsPtrOffsets = { 0xED0 };
        private int[] tavernsPtrOffsets = { 0xFF0 };

        private int[][] chestPtrOffsets =
        {
            new int[] { 0x1A8, 0x64, 0x18 }, // WaterChests
            new int[] { 0x450, 0xE38, 0x18 }, // UnchartedChests
            new int[] { 0xB08, 0x744, 0x18 }, // Havana
            new int[] { 0xB08, 0xA78, 0x18 }, // CapeBatavistia
            new int[] { 0xB08, 0x9C4, 0x18 }, // DryTortuga
            new int[] { 0xB08, 0x898, 0x18 }, // SaltKey
            new int[] { 0xB08, 0x80C, 0x18 }, // Matanzas
            new int[] { 0xB08, 0xA8C, 0x18 }, // Flordia
            new int[] { 0xB08, 0x848, 0x18 }, // Nassua
            new int[] { 0xB08, 0x9D8, 0x18 }, // Eleuthra
            new int[] { 0xB08, 0x7D0, 0x18 }, // Andreas
            new int[] { 0xB08, 0x6CC, 0x18 }, // Cat
            new int[] { 0xB08, 0x690, 0x18 }, // AbacoIsland
            new int[] { 0xB08, 0x758, 0x18 }, // Hideout
            new int[] { 0xB08, 0x9EC, 0x18 }, // Gibra
            new int[] { 0xB08, 0x794, 0x18 }, // Jiguey
            new int[] { 0xB08, 0x8AC, 0x18 }, // SaltLagoon
            new int[] { 0xB08, 0x71C, 0x18 }, // Crooked
            new int[] { 0xB08, 0x7F8, 0x18 }, // Mariguana
            new int[] { 0xB08, 0x884, 0x18 }, // Principe
            new int[] { 0xB08, 0xA14, 0x18 }, // Punta
            new int[] { 0xB08, 0x938, 0x18 }, // Tortuga
            new int[] { 0xB08, 0x834, 0x18 }, // Cumberland
            new int[] { 0xB08, 0x7E4, 0x18 }, // Petite
            new int[] { 0xB08, 0x708, 0x18 }, // Tulum
            new int[] { 0xB08, 0x99C, 0x18 }, // Conttoyor
            new int[] { 0xB08, 0xA00, 0x18 }, // Navassa
            new int[] { 0xB08, 0x76C, 0x18 }, // IlleAVache
            new int[] { 0xB08, 0x7BC, 0x18 }, // Kingston
            new int[] { 0xB08, 0x870, 0x18 }, // Observatory
            new int[] { 0xB08, 0x974, 0x18 }, // Charlotte
            new int[] { 0xB08, 0x6B8, 0x18 }, // Annatto
            new int[] { 0xB08, 0x780, 0x18 }, // Isla
            new int[] { 0xB08, 0xA28, 0x18 }, // Serranillia
            new int[] { 0xB08, 0x85C, 0x18 }, // NewBone
            new int[] { 0xB08, 0x820, 0x18 }, // Misteriosa
            new int[] { 0xB08, 0x988, 0x18 }, // Chinchorro
            new int[] { 0xB08, 0x6F4, 0x18 }, // Corozal
            new int[] { 0xB08, 0x67C, 0x18 }, // Ambergis
            new int[] { 0xB08, 0x8C0, 0x18 }, // Santanillas
            new int[] { 0xB08, 0x960, 0x18 }, // Castillo
            new int[] { 0xB08, 0x6A4, 0x18 }, // Arrayos
            new int[] { 0xB08, 0x7A8, 0x18 }, // Pinos
            new int[] { 0xB08, 0x6E0, 0x18 }, // Cayman
            new int[] { 0xB08, 0x9B0, 0x18 }, // Cruz
            new int[] { 0xB08, 0x924, 0x18 }, // SanJuan
            new int[] { 0xB08, 0x730, 0x18 }, // GrandCayman
        };

        public GameStats(IntPtr processHandle, IntPtr baseAddress)
        {
            this.processHandle = processHandle;
            this.baseAddress = baseAddress;
        }

        public (int Percent, float PercentFloat, int Viewpoints, int Myan, int Treasure, int Fragments, int Assassin, int Naval, int Letters, int Manuscripts, int Music, int Forts, int Taverns, int TotalChests) GetStats()
        {
            int percent = Read<int>((nint)baseAddress + 0x49D9774, percentPtrOffsets);
            float percentfloat = Read<float>((nint)baseAddress + 0x049F1EE8, percentFtPtrOffsets);
            int viewpoints = Read<int>((nint)baseAddress + 0x0002E8D0, viewpointsPtrOffsets);
            int myan = Read<int>((nint)baseAddress + 0x0002E8D0, myanPtrOffsets);
            int treasure = Read<int>((nint)baseAddress + 0x0002E8D0, treasurePtrOffsets);
            int fragments = Read<int>((nint)baseAddress + 0x0002E8D0, fragmentsPtrOffsets);
            int assassin = Read<int>((nint)baseAddress + 0x0002E8D0, assassinPtrOffsets);
            int naval = Read<int>((nint)baseAddress + 0x002FE2CC, navalPtrOffsets);
            int letters = Read<int>((nint)baseAddress + 0x0002E8D0, lettersPtrOffsets);
            int manuscripts = Read<int>((nint)baseAddress + 0x0002E8D0, manuscriptsPtrOffsets);
            int music = Read<int>((nint)baseAddress + 0x0002E8D0, musicPtrOffsets);
            int forts = Read<int>((nint)baseAddress + 0x002FE2CC, fortsPtrOffsets);
            int taverns = Read<int>((nint)baseAddress + 0x002FE2CC, tavernsPtrOffsets);

            int totalChests = 0;
            foreach (var chestOffsets in chestPtrOffsets)
            {
                totalChests += Read<int>((nint)baseAddress + 0x0002E8D0, chestOffsets);
            }

            return (percent, percentfloat, viewpoints, myan, treasure, fragments, assassin, naval, letters, manuscripts, music, forts, taverns, totalChests);
        }

        private unsafe T Read<T>(nint baseAddress, int[] offsets) where T : unmanaged
        {
            nint deref = baseAddress;

            foreach (int offset in offsets)
            {
                if (!ReadProcessMemory(processHandle, deref, &deref, 4, out nint bytesReadOuter) || bytesReadOuter != 4)
                {
                    throw new Win32Exception();
                }
                deref += offset;
            }

            T result;
            if (!ReadProcessMemory(processHandle, deref, &result, Marshal.SizeOf<T>(), out nint bytesReadInner) || bytesReadInner != Marshal.SizeOf<T>())
            {
                throw new Win32Exception();
            }

            return result;
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


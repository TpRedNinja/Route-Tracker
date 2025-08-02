﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Route_Tracker;

namespace Route_Tracker
{
    public static class AC4CollectibleOffsets
    {
        // Chest Dictionary
        public static readonly Dictionary<int, string> ChestOffsetToLocation = new()
        {
            [0x67C] = "Ambergris Key",
            [0x690] = "Abaco Island",
            [0x6A4] = "Arroyos",
            [0x6B8] = "Anotto Bay",
            [0x6CC] = "Cat Island",
            [0x6E0] = "Cayman Sound",
            [0x6F4] = "Corozal",
            [0x708] = "Tulum",
            [0x71C] = "Crooked Island",
            [0x730] = "Grand Cayman",
            [0x744] = "Havana",
            [0x758] = "Great Igauna",
            [0x76C] = "Ille A Vache",
            [0x780] = "Isla Providencia",
            [0x794] = "Jiguey",
            [0x7A8] = "Pinos Isle",
            [0x7BC] = "Kingston",
            [0x7D0] = "Andreas Island",
            [0x7E4] = "Cumberland Bay",
            [0x7F8] = "Mariguana",
            [0x80C] = "Matanzas",
            [0x820] = "Misteriosa",
            [0x834] = "Petite Caverne",
            [0x848] = "Nassua",
            [0x85C] = "New Bone",
            [0x870] = "Long Bay",
            [0x884] = "Principe",
            [0x898] = "Salt Key Bank",
            [0x8AC] = "Salt Lagoon",
            [0x8C0] = "Santanillas",
            [0x8D4] = "Kabah Ruins",
            [0x8E8] = "Devils Eyes Cavern",
            [0x8FC] = "La Concepcion",
            [0x910] = "The Blue Hole",
            [0x924] = "San Juan",
            [0x938] = "Tortuga",
            [0x94C] = "Uncharted",
            [0x960] = "Castilo de Jagua",
            [0x974] = "Charlotte",
            [0x988] = "Chinchorro",
            [0x99C] = "Conttoyor",
            [0x9B0] = "Caba De Cruz",
            [0x9C4] = "Dry Tortuga",
            [0x9D8] = "Eleuthera",
            [0x9EC] = "Gibra",
            [0xA00] = "Navassa",
            [0xA14] = "Punta Guarico",
            [0xA28] = "Serranilla",
            [0xA3C] = "San Ignacio",
            [0xA50] = "Antocha Wreck",
            [0xA64] = "The Black Trench",
            [0xA78] = "Cape Bonavista",
            [0xA8C] = "Florida"
        };

        // Fragment Dictionary
        public static readonly Dictionary<int, string> FragmentOffsetToLocation = new()
        {
            [-0xAA0] = "Ambergris Key",
            [-0xA8C] = "Abaco Island",
            [-0xA78] = "Arroyos",
            [-0xA64] = "Anotto Bay",
            [-0xA50] = "Cape Bonavista",
            [-0xA3C] = "Castilo de Jagua",
            [-0xA28] = "Cat Island",
            [-0xA14] = "Cayman Sound",
            [-0xA00] = "Charlotte",
            [-0x9EC] = "Chinchorro",
            [-0x9D8] = "Conttoyor",
            [-0x9C4] = "Corozal",
            [-0x9B0] = "Tulum",
            [-0x99C] = "Crooked Island",
            [-0x988] = "Caba De Cruz",
            [-0x974] = "Dry Tortuga",
            [-0x960] = "Eleuthera",
            [-0x94C] = "Florida",
            [-0x938] = "Gibra",
            [-0x924] = "Grand Cayman",
            [-0x910] = "Havana",
            [-0x8FC] = "Great Igauna",
            [-0x8E8] = "Ille A Vache",
            [-0x8D4] = "Isla Providencia",
            [-0x8C0] = "Jiguey",
            [-0x8AC] = "Pinos Isle",
            [-0x898] = "Kingston",
            [-0x884] = "Andreas Island",
            [-0x870] = "Mariguana",
            [-0x85C] = "Cumberland Bay",
            [-0x848] = "Matanzas",
            [-0x834] = "Misteriosa",
            [-0x820] = "Petite Caverne",
            [-0x80C] = "Nassua",
            [-0x7F8] = "Navassa",
            [-0x7E4] = "New Bone",
            [-0x7D0] = "Long Bay",
            [-0x7BC] = "Punta Guarico",
            [-0x7A8] = "Principe",
            [-0x794] = "Salt Key Bank",
            [-0x780] = "Salt Lagoon",
            [-0x76C] = "Santanillas",
            [-0x758] = "Serranilla",
            [-0x744] = "San Juan",
            [-0x730] = "Kabah Ruins",
            [-0x71C] = "Devils Eyes Cavern",
            [-0x708] = "La Concepcion",
            [-0x6F4] = "The Blue Hole",
            [-0x6E0] = "San Ignacio",
            [-0x6CC] = "Antocha Wreck",
            [-0x6B8] = "The Black Trench",
            [-0x6A4] = "Tortuga",
            [-0x690] = "Uncharted",
        };

        // Tavern Dictionary
        public static readonly Dictionary<int, string> TavernOffsetToLocation = new()
        {
            [0x319C] = "Kingston",
            [0x31B0] = "Salt Key Bank",
            [0x31C4] = "Grand Cayman",
            [0x31D8] = "Crooked Island",
            [0x31EC] = "Arroyos",
            [0x3200] = "Andreas Island",
            [0x3214] = "Ille A Vache",
            [0x3228] = "Corozal"
        };

        // Treasure Map Dictionary
        public static readonly Dictionary<int, string> TreasureMapOffsetToLocation = new()
        {
            [0x3250] = "Havana",
            [0x3264] = "Great Igauna",
            [0x3278] = "Abaco Island",
            [0x328C] = "Petite Caverne",
            [0x32A0] = "Andreas Island",
            [0x32B4] = "Anotto Bay",
            [0x32C8] = "Misteriosa",
            [0x32DC] = "Pinos Isle",
            [0x32F0] = "Tortuga",
            [0x3304] = "Ambergris Key",
            [0x3318] = "Corozal",
            [0x332C] = "Ille A Vache",
            [0x3340] = "Isla Providencia",
            [0x3354] = "Salt Lagoon",
            [0x3368] = "Santanillas",
            [0x337C] = "Cumberland Bay",
            [0x3390] = "Mariguana",
            [0x33A4] = "Cayman Sound",
            [0x33B8] = "Uncharted1",
            [0x33CC] = "Uncharted2",
            [0x33E0] = "Uncharted",
            [0x33F4] = "Special",
            [0x3408] = "Cape Bonavista"
        };

        // Viewpoint Dictionary
        public static readonly Dictionary<int, string> ViewpointOffsetToLocation = new()
        {
            [0x2BAC] = "Havana",
            [0x2BC0] = "Kingston",
            [0x2BD4] = "Nassua",
            [0x2BE8] = "Abaco Island",
            [0x2BFC] = "Cayman Sound",
            [0x2C10] = "Corozal",
            [0x2C24] = "Florida",
            [0x2C38] = "Ille A Vache",
            [0x2C4C] = "Andreas Island",
            [0x2C60] = "Mariguana",
            [0x2C74] = "Cumberland Bay",
            [0x2C88] = "Salt Lagoon",
            [0x2C9C] = "Cat Island",
            [0x2CB0] = "Matanzas",
            [0x2CC4] = "New Bone",
            [0x2CD8] = "Tortuga",
            [0x2CEC] = "Pinos Isle",
            [0x2D00] = "Misteriosa",
            [0x2D14] = "Santanillas",
            [0x2D28] = "Arroyos",
            [0x2D3C] = "Crooked Island",
            [0x2D50] = "Grand Cayman",
            [0x2D64] = "Salt Key Bank",
            [0x2D78] = "Great Igauna",
            [0x2E90] = "Isla Providencia",
            [0x2EA4] = "Principe",
            [0x2EB8] = "Long Bay",
            [0x2ECC] = "Tulum",
            [0x2EE0] = "Cape Bonavista"
        };
    }
}

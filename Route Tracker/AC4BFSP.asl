//Assassins Creed IV Black Flag Version: 2.0.0 Autosplitter made by TpRedNinja
//Thanks for the help from tasz for testing all versions of my autosplitter and getting the percentage values for everything

state("AC4BFSP")
{
    //--stuff for loading & starting stuff--
        int MainMenu: 0x49D2204; //65540 Main Menu anything else is in save file
        int Character: 0x23485C0; //1 for modern day 0 for not
        int location: 0x26C1F80; //for land lock Major locations the number is the same such as Cape & Principe everything else changes. 
        //For secondary locations as of now it stays the same number. To do: write down all numbers for all locations
        int loading: 0x04A1A6CC, 0x7D8; //0 when not loading 1 when u are loading
    //--stuff that affects Edward--
        int Money: 0x049E3788, 0xA0, 0x90; //Shows your current money when in the animus.
        int Health: 0x049E3788, 0x34, 0x84; //shows you current health works with upgrades.
        float oxygen: 0x25E080C; //current oxygen 1 for full and anything lower means the bar is going down. Except for exiting a shipwreck. 
    //--for knowing how far your are and for splitting--
        int Percentage: 0x049D9774, 0x284; //Shows the total current percentage
        float PercentageF: 0x049F1EE8, 0x74; //Shows the total current percentage but in a float
    //--General Counters--
        int Viewpoints: 0x026BEC04, 0xC0, 0x5C8, 0x18; //Tracks total number of viewpoints completed
        int MyanStele: 0x026BEC04, 0xC0, 0x5DC, 0x18; //Tracks total number of MyanStele completed
        int BuriedTreasure: 0x026BEAC0, 0x2D0, 0x85C, 0x690, 0x18; //Tracks total number of BuriedTreasure collected
        int animusfragments: 0x026BE520, 0x520, 0xB14, 0xF78, 0x18; //Tracks total number of animus fragments collected
        int AssassinContracts: 0x026BEAC0, 0x2D0, 0x858, 0x488, 0x18; //Tracks the total number of Assassin Contracts completed
        int NavalContracts: 0x00453734, 0xD5C, 0x968, 0x410, 0x18; //Tracks total number of Naval Contracts completed
        int Letters: 0x026BEAC0, 0x2D0, 0x86C, 0xD0C, 0x18; //Tracks total number of letters in a bottle collected
        int Manuscripts: 0x026BEC04, 0x320, 0x100, 0x780, 0x18; //Tracks total number of Manuscripts collected
        int MusicSheets: 0x026BEC04, 0x320, 0x100, 0xED8, 0x18; //Tracks total number of MusicSheets collected
        int Forts: 0x026BE51C, 0x9A0, 0x3B8, 0x140, 0x30; //Tracks total number of forts captured
        int UnchartedFragments: 0x026BEC04, 0x320, 0x100, 0x424, 0x18; //Tracks total number of uncharted animusfragments collected
        int UnchartedSecrets: 0x026BEAC0, 0x2D0, 0x824, 0xB18, 0x18; //Tracks total number of uncharted secrets collected 
    //Taverns bools
        int Taverns: 0x026BE51C, 0x968, 0xD60, 0x438, 0x30; //Tracks total number of taverns completed
        int KingstonCrown: 0x003A8888, 0xA14, 0x664, 0xA0, 0x18; //Tracks wether the Kingston Tavern is completed 0 for not 1 for completed
        int ArroyosArms: 0x0006D0B8, 0xD80, 0x2E4, 0x18; //Tracks wether the Arroyos Tavern is completed 0 for not 1 for completed
        int SaltKeyBanter: 0x000133B4, 0x1A4, 0x34, 0x14, 0x18; //Tracks wether the Salt Key Tavern is completed 0 for not 1 for completed
        int CrookedIslandCanter: 0x000133B4, 0x174, 0x154, 0x62C, 0x18; //Tracks wether the Crooked Island Tavern is completed 0 for not 1 for completed
        int TheRandyCayman: 0x000133B4, 0x170, 0x340, 0x0, 0x18; //Tracks wether the The Randy Cayman Tavern is completed 0 for not 1 for completed
        int TheAndreasInn: 0x0193F154, 0x2A8, 0x0, 0x18; //Tracks wether the The Andreas Inn Tavern is completed 0 for not 1 for completed
        int CorozalTavern: 0x000133B4, 0x100, 0x34, 0x0, 0x18; //Tracks wether the Corozal Tavern is completed 0 for not 1 for completed
        int VinoAVache: 0x000133B4, 0x114, 0x34, 0x1CC, 0x18; //Tracks wether the Vino A Vache Tavern is completed 0 for not 1 for completed
    //Chest Counters--
        int WaterChests: 0x026BEC04, 0x30C, 0x58C, 0x18; //Tracks total number of chests underwater collected
        int UnchartedChests: 0x026BEAB8, 0x2E8, 0x8BC, 0x94C, 0x18; //Tracks total number of uncharted chests collected
        int Havana: 0x026BEAC0, 0x2D0, 0x8BC, 0x744, 0x18; // Tracks total number of chests collected at Havana
        int CapeBatavistia: 0x026BEAC0, 0x2D0, 0x884, 0xB90, 0x18; // Tracks total number of chests collected at Cape Bonavistia
        int DryTortuga: 0x026BEAC0, 0x2D0, 0x8A4, 0xA3C, 0x18; // Tracks total number of chests collected at Dry Tortuga Fort
        int SaltKey: 0x026BEAC0, 0x2D0, 0x8BC, 0x898, 0x18; // Tracks total number of chests collected at Salt Key Bay
        int Matanzas: 0x026BEAC0, 0x2D0, 0x8BC, 0x80C, 0x18; // Tracks total number of chests collected at Matanzas
        int Flordia: 0x026BEAC0, 0x2D0, 0x884, 0xBA4, 0x18; // Tracks total number of chests collected at Florida
        int Nassua: 0x026BEAC0, 0x2D0, 0x8A0, 0x8D4, 0x18; // Tracks total number of chests collected at Nassau
        int Eleuthra: 0x026BEAC0, 0x2D0, 0x8B8, 0x9EC, 0x18; // Tracks total number of chests collected at Eleuthra Fort
        int Andreas: 0x026BEAC0, 0x2D0, 0x87C, 0x910, 0x18; // Tracks total number of chests collected at Andreas Island
        int Cat: 0x026BEAC0, 0x2D0, 0x8BC, 0x6CC, 0x18; // Tracks total number of chests collected at Cat Island
        int AbacoIsland: 0x026BEAC0, 0x2D0, 0x87C, 0x7D0, 0x18; // Tracks total number of chests collected at Abaco Island
        int Hideout: 0x026BEAC0, 0x2D0, 0x8AC, 0x7A8, 0x18; // Tracks total number of chests collected at Hideout
        int Gibra: 0x026BEAC0, 0x2D0, 0x8BC, 0x9EC, 0x18; // Tracks total number of chests collected at Gibra
        int Crooked: 0x026BEAC0, 0x2D0, 0x884, 0x834, 0x18; // Tracks total number of chests collected at Crooked Island
        int Jiguey: 0x026BEAC0, 0x2D0, 0x8A0, 0x820, 0x18; // Tracks total number of chests collected at Jiguey
        int Mariguana: 0x026BEAC0, 0x2D0, 0x87C, 0x938, 0x18; // Tracks total number of chests collected at Mariguana
        int SaltLagoon: 0x026BEAC0, 0x2D0, 0x874, 0xA14, 0x18; // Tracks total number of chests collected at Salt Lagoon
        int Principe: 0x026BEAC0, 0x2D0, 0x8BC, 0x884, 0x18; // Tracks total number of chests collected at Principe
        int Punta: 0x026BEAC0, 0x2D0, 0x8BC, 0xA14, 0x18; // Tracks total number of chests collected at Punta Guarico
        int Tortuga: 0x026BEAC0, 0x2D0, 0x8B4, 0x960, 0x18; // Tracks total number of chests collected at Tortuga
        int Petite: 0x026BEAC0, 0x2D0, 0x8B4, 0x85C, 0x18; // Tracks total number of chests collected at Petite Cavern
        int Cumberland: 0x026BEAC0, 0x2D0, 0x8B0, 0x820, 0x18; // Tracks total number of chests collected at Cumberland Bay
        int Tulum: 0x026BEAC0, 0x2D0, 0x8B4, 0x730, 0x18; // Tracks total number of chests collected at Tulum
        int Conttoyor: 0x026BEAC0, 0x2D0, 0x8B8, 0x9B0, 0x18; // Tracks total number of chests collected at Conttoyor
        int Navassa: 0x026BEAC0, 0x2D0, 0x8BC, 0xA00, 0x18; // Tracks total number of chests collected at Navassa
        int IlleAVache: 0x026BEAC0, 0x2D0, 0x8A8, 0x7D0, 0x18; // Tracks total number of chests collected at Ille A Vache
        int Kingston: 0x026BEAC0, 0x2D0, 0x8BC, 0x7BC, 0x18; // Tracks total number of chests collected at Kingston
        int Observatory: 0x026BEAC0, 0x2D0, 0x88C, 0x960, 0x18; // Tracks total number of chests collected at Observatory
        int Charlotte: 0x026BEAC0, 0x2D0, 0x8AC, 0x9C4, 0x18; // Tracks total number of chests collected at Charlotte
        int Annatto: 0x026BEAC0, 0x2D0, 0x8BC, 0x6B8, 0x18; // Tracks total number of chests collected at Annatto Bay
        int Isla: 0x026BEAC0, 0x2D0, 0x880, 0x8AC, 0x18; // Tracks total number of chests collected at Isla
        int Serranillia: 0x026BEAC0, 0x2D0, 0x8AC, 0xA78, 0x18; // Tracks total number of chests collected at Serranillia
        int Misteriosa: 0x026BEAC0, 0x2D0, 0x8A4, 0x898, 0x18; // Tracks total number of chests collected at Misteriosa
        int NewBone: 0x026BEAC0, 0x2D0, 0x8BC, 0x85C, 0x18; // Tracks total number of chests collected at New Bone
        int Chinchorro: 0x026BEAC0, 0x2D0, 0x8BC, 0x988, 0x18; // Tracks total number of chests collected at Chinchorro
        int Santanillas: 0x026BEAC0, 0x2D0, 0x8BC, 0x8C0, 0x18; // Tracks total number of chests collected at Santanillas
        int Corozal: 0x026BEAC0, 0x2D0, 0x898, 0x7A8, 0x18; // Tracks total number of chests collected at Corozal
        int Ambergis: 0x026BEAC0, 0x2D0, 0x8B8, 0x690, 0x18; // Tracks total number of chests collected at Ambergis Bay
        int Castillo: 0x026BEAC0, 0x2D0, 0x8A8, 0x9C4, 0x18; // Tracks total number of chests collected at Castillo
        int Pinos: 0x026BEAC0, 0x2D0, 0x8A8, 0x80C, 0x18; // Tracks total number of chests collected at Pinos Isle
        int Arrayos: 0x026BEAC0, 0x2D0, 0x8BC, 0x6A4, 0x18; // Tracks total number of chests collected at Arrayos
        int Cayman: 0x026BEAC0, 0x2D0, 0x894, 0x7A8, 0x18; // Tracks total number of chests collected at Cayman Sound
        int Cruz: 0x026BEAC0, 0x2D0, 0x884, 0xAC8, 0x18; // Tracks total number of chests collected at Cruz
        int SanJuan: 0x026BEAC0, 0x2D0, 0x8BC, 0x730, 0x18; // Tracks total number of chests collected at San Juan
        int GrandCayman: 0x026BEAC0, 0x2D0, 0x8BC, 0x924, 0x18; // Tracks total number of chests collected at Grand Cayman

}

startup
{
    //set text taken from Poppy Playtime C2
    Action<string, string> SetTextComponent = (id, text) => {
        var textSettings = timer.Layout.Components.Where(x => x.GetType().Name == "TextComponent").Select(x => x.GetType().GetProperty("Settings").GetValue(x, null));
        var textSetting = textSettings.FirstOrDefault(x => (x.GetType().GetProperty("Text1").GetValue(x, null) as string) == id);
        if (textSetting == null)
        {
            var textComponentAssembly = Assembly.LoadFrom("Components\\LiveSplit.Text.dll");
            var textComponent = Activator.CreateInstance(textComponentAssembly.GetType("LiveSplit.UI.Components.TextComponent"), timer);
            timer.Layout.LayoutComponents.Add(new LiveSplit.UI.Components.LayoutComponent("LiveSplit.Text.dll", textComponent as LiveSplit.UI.Components.IComponent));

            textSetting = textComponent.GetType().GetProperty("Settings", BindingFlags.Instance | BindingFlags.Public).GetValue(textComponent, null);
            textSetting.GetType().GetProperty("Text1").SetValue(textSetting, id);
        }

        if (textSetting != null)
            textSetting.GetType().GetProperty("Text2").SetValue(textSetting, text);
    };
    vars.SetTextComponent = SetTextComponent;

    //Settings for splits
    settings.Add("Splits", true, "Splitting Options");
    settings.Add("100%", false, "100%", "Splits");
    settings.Add("Everything", false, "Everything", "Splits");
    settings.Add("Viewpoints", false, "Viewpoints", "Splits");
    settings.Add("MyanStele", false, "MyanStele", "Splits");
    settings.Add("BuriedTreasure", false, "BuriedTreasure", "Splits");
    settings.Add("Contracts", false, "Contracts", "Splits");
    settings.Add("Templar Hunts", false, "Templar Hunts", "Splits");
    settings.Add("Forts", false, "Forts", "Splits");
    settings.Add("Taverns", false, "Taverns", "Splits");
    settings.Add("Modern day", false, "Modern day", "Splits");
    settings.Add("Shipwrecks", false, "Shipwrecks", "Splits");
    settings.Add("Legendary Ships", false, "Legendary Ships", "Splits");

    settings.SetToolTip("Splits", "Gives options of where to split");
    settings.SetToolTip("Everything", "Splits on everything. Missions, collectibles, etc. \n" + "This is only recommended for 100% runs & if you have a split for everything");
    settings.SetToolTip("100%", "Splits after getting all uncharted Collectibles");
    settings.SetToolTip("Viewpoints", "Splits after syncing a viewpoint");
    settings.SetToolTip("MyanStele", "Splits after lotting the myan stele stone ");
    settings.SetToolTip("BuriedTreasure", "Splits after edwards opens the treasure chest");
    settings.SetToolTip("Contracts", "Splits when gaining money from completing a contract");
    settings.SetToolTip("Templar Hunts", "Splits when completing a templar hunt mission");
    settings.SetToolTip("Forts", "Splits when fort turns green in map");
    settings.SetToolTip("Taverns", "Splits on defeating all guards in for a tavern");
    settings.SetToolTip("Modern day", "Splits when entering the animus for the modern day missions");
    settings.SetToolTip("Shipwrecks", "Splits when getting a x amount of chests from the shipwrecks");
    settings.SetToolTip("Legendary Ships", "Splits when defeating legendary ships");

    //not splitting settings
    settings.Add("Percentage Display", false);
    settings.Add("Collectibles Display", false, "Collectibles Display");
    settings.Add("Uncharted Display", false);
    settings.Add("Debug", false, "Debug");
    settings.Add("Calculator", false, "Calculator","Debug");
    settings.SetToolTip("Debug", "This will show the current MainMenu value and loading.\n" + 
    "Along with the calculator if u use it");
    /*for any future settings i want to add
    settings.Add("", false, "", "Splits");
    settings.SetToolTip("", "Splits when ");
    */
    vars.completedsplits = new List<string>();
    vars.stopwatch = new Stopwatch();
    vars.SplitTime = null;
    vars.IsStopwatchStop = false;
}

init
{
    vars.DryTortugaChests = 0;
    vars.EleuthraChests = 0;
    vars.GibraChests = 0;
    vars.PuntaGuaricoChests = 0;
    vars.TwoLocationsChests = 0;
    vars.CharlotteChests = 0;
    vars.SerranilliaChests = 0;
    vars.ChinchorroChests = 0;
    vars.CastilloChests = 0;
    vars.CruzChests = 0;
    vars.TotalChests = 0;
    vars.Forts = 0;
    vars.Taverns = 0;
    vars.NavalContracts = 0;
    
    if (current.MainMenu == 65540 && current.loading == 0)
    {
        timer.IsGameTimePaused = true;
    }

    if (vars.IsStopwatchStop == true)
    {
        vars.stopwatch.Start();
        vars.IsStopwatchStop = false; 
    }
}

update
{
    if (current.Percentage < 100)
    {
        print("Rounded 5 places");
        current.PercentageF = Math.Round(current.PercentageF, 5);
    } else if (current.Percentage == 100 || settings["Everything"])
    {
        print("Rounded 2 places");
        current.PercentageF = Math.Round(current.PercentageF, 2);
    }
    
    vars.SplitTime = (int)vars.stopwatch.Elapsed.TotalSeconds;
    
    //Variables for all the chest Counters
    vars.MiscChests = current.WaterChests + current.UnchartedChests;
    vars.DryTortugaChests = current.Havana + current.CapeBatavistia + current.DryTortuga + current.SaltKey + current.Matanzas + current.Flordia;
    vars.EleuthraChests = current.Nassua + current.Eleuthra + current.Andreas + current.Cat + current.AbacoIsland;
    vars.GibraChests = current.Hideout + current.Gibra + current.Jiguey + current.SaltLagoon + current.Crooked + current.Mariguana;
    vars.PuntaGuaricoChests = current.Principe + current.Punta + current.Tortuga + current.Cumberland + current.Petite;
    vars.TwoLocationsChests = current.Tulum + current.Conttoyor + current.Navassa + current.IlleAVache;
    vars.CharlotteChests = current.Kingston + current.Observatory + current.Charlotte + current.Annatto;
    vars.SerranilliaChests = current.Isla + current.Serranillia + current.NewBone + current.Misteriosa;
    vars.ChinchorroChests = current.Chinchorro + current.Corozal + current.Ambergis + current.Santanillas;
    vars.CastilloChests = current.Castillo + current.Arrayos + current.Pinos + current.Cayman;
    vars.CruzChests = current.Cruz + current.SanJuan + current.GrandCayman;
    vars.TotalChests = vars.MiscChests + vars.DryTortugaChests + vars.EleuthraChests + vars.GibraChests + vars.PuntaGuaricoChests + vars.TwoLocationsChests + vars.CharlotteChests + vars.SerranilliaChests + vars.ChinchorroChests + vars.CastilloChests + vars.CruzChests;
    
    if (current.animusfragments == 200 && vars.Forts == 0 && vars.Taverns == 0 && vars.NavalContracts == 0)
    {
        vars.Forts = 10;
        vars.Taverns = 8;
        vars.NavalContracts = 15;
    }

    if (settings["Percentage Display"])
    {
        if (current.PercentageF != null){
            vars.SetTextComponent("Percentage Completion", current.PercentageF + "%");
        } else
        {
            vars.SetTextComponent("Percentage Completion", null + "%");
        }
    }

    if (settings["Uncharted Display"])
    {
        if (current.UnchartedChests != null && current.UnchartedFragments != null && current.UnchartedSecrets != null){
        vars.SetTextComponent("UnchartedChests Collected", current.UnchartedChests + "/46");
        vars.SetTextComponent("UnchartedFragments Collected", current.UnchartedFragments + "/30");
        vars.SetTextComponent("UnchartedSecrets Collected", current.UnchartedSecrets + "/3");   
        } else
        {
            vars.SetTextComponent("UnchartedChests Collected", null + "/46");
            vars.SetTextComponent("UnchartedFragments Collected", null + "/30");
            vars.SetTextComponent("UnchartedSecrets Collected", null + "/3");
        }
    }

    if (settings["Debug"])
    {
        //vars.SetTextComponent("", current. + "/"); for extras in the future
        if (current.MainMenu != null && current.loading != null)
        {
            vars.SetTextComponent("Current MainMenu Value", current.MainMenu + "/65540");
            vars.SetTextComponent("Current Loading", current.loading + "/1");
            vars.SetTextComponent("Time from Last Split", vars.SplitTime + "/2");
            if (settings["Calculator"])
            {
                if (current.PercentageF > old.PercentageF)
                {
                    vars.SetTextComponent("Percentage increased by ", current.PercentageF - old.PercentageF + "%");
                }
            }
        } else
        {
            vars.SetTextComponent("Current MainMenu Value", null + "N/A");
            vars.SetTextComponent("Current Loading", null + "N/A");
        }

    }

    if (settings["Collectibles Display"])
    {
        if (current.Viewpoints != null && current.MyanStele != null && current.BuriedTreasure != null && current.animusfragments != null 
        && current.AssassinContracts != null && current.Letters != null && current.Manuscripts != null 
        && current.MusicSheets != null)
        {
            
            vars.SetTextComponent("Viewpoints Synchronized", current.Viewpoints + "/58");
            vars.SetTextComponent("MyanStele Collected", current.MyanStele + "/16");
            vars.SetTextComponent("BuriedTreasure Found", current.BuriedTreasure + "/22");
            vars.SetTextComponent("Fragments Collected", current.animusfragments + "/200");
            vars.SetTextComponent("Total Chests Collected", vars.TotalChests + "/340");
            vars.SetTextComponent("Assassins Contracts Completed", current.AssassinContracts + "/30");
            vars.SetTextComponent("Naval Contracts Completed", vars.NavalContracts + "/15");
            vars.SetTextComponent("Letters Collected", current.Letters + "/20");
            vars.SetTextComponent("Manuscripts Collected", current.Manuscripts + "/20");
            vars.SetTextComponent("Shanties Collected", current.MusicSheets + "/24");
            vars.SetTextComponent("Forts captured", vars.Forts + "/10");
            vars.SetTextComponent("Taverns Unlocked", vars.Taverns + "/8");
        } else
        {
            vars.SetTextComponent("Viewpoints Synchronized", null + "/58");
            vars.SetTextComponent("MyanStele Collected", null + "/16");
            vars.SetTextComponent("BuriedTreasure Found", null + "/22");
            vars.SetTextComponent("Fragments Collected", null + "/200");
            vars.SetTextComponent("Total Chests Collected", null + "/214");
            vars.SetTextComponent("Assassins Contracts Completed", null + "/30");
            vars.SetTextComponent("Naval Contracts Completed", null + "/15");
            vars.SetTextComponent("Letters Collected", null + "/20");
            vars.SetTextComponent("Manuscripts Collected", null + "/20");
            vars.SetTextComponent("Shanties Collected", null + "/24");
            vars.SetTextComponent("Forts captures", null + "/10");
            vars.SetTextComponent("Taverns Unlocked", null + "/8");
        }
    }

    //print(modules.First().ModuleMemorySize.ToString());
}

start
{
    //should start after accepting the save file
    if (current.MainMenu == 131076 && old.MainMenu == 65540 && current.PercentageF < 1)
    {
        return true;
    }
}

onStart
{
    if (vars.SplitTime > 0)
    {
        vars.stopwatch.Restart();
    } else
    {
        vars.stopwatch.Start();
    } 
}

split
{
    //should work for most splits
    if ((current.PercentageF >= old.PercentageF + 0.66666 && current.PercentageF <= old.PercentageF + 1.66668) && current.loading == 0)
    {
        print("MainMission Split");
        vars.stopwatch.Restart();
        return true;
    }
    
    if (settings["Everything"])
    {
        if (current.PercentageF > old.PercentageF && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Splited");
            vars.stopwatch.Restart();
            return true;
        }
    }

    if (settings["100%"])
    {
        if (current.UnchartedChests == 46 && old.UnchartedChests == 45 && current.UnchartedFragments == 30 && current.UnchartedSecrets == 3)
        {
            print("all uncharted collectibles collected and splitted");
            vars.stopwatch.Restart();
            return true;
        }
    }

    if(settings["Viewpoints"])
    {
        //splits when syncing a viewpoint
        if (current.Viewpoints == old.Viewpoints + 1 && vars.SplitTime > 2 && current.loading == 0)
        {
            print("Viewpoints Split Version = Counter");
            vars.stopwatch.Restart();
            return true;
        } else if ((current.PercentageF == old.PercentageF + 0.03750 || current.PercentageF == old.PercentageF + 0.03333 || 
        current.PercentageF == old.PercentageF + 0.11250 || current.PercentageF == old.PercentageF + 0.03214 || 
        current.PercentageF == old.PercentageF + 0.05625) && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Viewpoints Split Version = %");
            vars.stopwatch.Restart();
            return true;
        }
        
    }

    if(settings["MyanStele"])
    {
        //splits when getting one myan stone
        if (current.MyanStele == old.MyanStele + 1 && vars.SplitTime > 2 && current.loading == 0)
        {
            print("MyanStele Split Version = Counter");
            vars.stopwatch.Restart();
            return true;
        } else if (((current.PercentageF >= old.PercentageF + 0.09288 && current.PercentageF <= old.PercentageF + 0.09291) || 
        (current.PercentageF >= old.PercentageF + 0.18577 && current.PercentageF <= old.PercentageF + 0.18579) || 
        (current.PercentageF >= old.PercentageF + 0.20641 && current.PercentageF <= old.PercentageF + 0.20643)) && current.loading == 0 
        && vars.SplitTime > 2)
        {
            print("MyanStele Split Version = %");
            vars.stopwatch.Restart();
            return true;
        }
    }

    if(settings["BuriedTreasure"])
    {
        //splits when opening a buried treasure
        if (current.BuriedTreasure == old.BuriedTreasure + 1 && vars.SplitTime > 2 && current.loading == 0)
        {
            print("BuriedTreasure Split Version = Counter");
            vars.stopwatch.Restart();
            return true;
        } else if (current.PercentageF == old.PercentageF + 0.20455 && current.loading == 0 && vars.SplitTime > 2)
        {
            print("BuriedTreasure Split Version = %");
            vars.stopwatch.Restart();
            return true;
        }
    }

    if (settings["Contracts"])
    {
        //splits for assassin contracts
        if (current.AssassinContracts == old.AssassinContracts + 1 && vars.SplitTime > 2 && current.loading == 0)
        {
            print("Assassin Contracts Split Version = Counter");
            vars.stopwatch.Restart();
            return true;
        } else if ((current.PercentageF >= old.PercentageF + 0.61727 && current.PercentageF <= old.PercentageF + 0.61730) 
        && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Assassin Contracts Split Version = %");
            vars.stopwatch.Restart();
            return true;
        }

        //splits for naval contracts
        if ((current.PercentageF >= old.PercentageF + 0.02056 && current.PercentageF <= old.PercentageF + 0.02060) && current.loading == 0 && vars.SplitTime > 4)
        {
            print("Naval Contracts Split Version = %");
            vars.NavalContracts ++;
            vars.stopwatch.Restart();
            return true;
        } else if((current.Money == old.Money + 1200 || current.Money == old.Money + 1800 || current.Money == old.Money + 2400) 
        && current.loading == 0 && vars.SplitTime > 4)
        {
            print("Naval Contracts Split Version = Money");
            vars.NavalContracts ++;
            vars.stopwatch.Restart();
            return true;
        }

    }

    if (settings["Forts"])
    {
        //splits when capturing a fort
        if (current.PercentageF == old.PercentageF + 0.22500 && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Forts Split version = %");
            vars.Forts ++;
            vars.stopwatch.Restart();
            return true;
        } else if ((current.Money == old.Money + 1000 || current.Money == old.Money + 3000 || current.Money == old.Money + 5000) 
        && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Forts Split version = Money");
            vars.Forts ++;
            vars.stopwatch.Restart();
            return true;
        }
    }

    if (settings["Taverns"])
    {
        //splits when completing a tavern
        if (((current.PercentageF >= old.PercentageF + 0.20223 && current.PercentageF <= old.PercentageF + 0.20226) || 
        (current.PercentageF >= old.PercentageF + 0.18538 && current.PercentageF <= old.PercentageF + 0.18541)) && current.loading == 0 && vars.SplitTime > 2)
        {
            print("Taverns Split");
            vars.Taverns ++;
            vars.stopwatch.Restart();
            return true;
        }
    }

    if(settings["Modern day"])
    {
        //splits when entering the animus
        if ((current.Money != null || current.Health != null) && current.MainMenu == 1311076 && current.Character == 0 && current.loading == 0)
        {
            print("Modern day Split");
            return true;
        }
    }

    if (settings["Shipwrecks"])
    {
        //splitting for shipwrecks
        if (current.WaterChests == 6 && !vars.completedsplits.Contains("san Ignacio"))
        {
            vars.completedsplits.Add("san Ignacio");
            return true;
        }

        if (current.WaterChests == 13 && !vars.completedsplits.Contains("blue hole"))
        {
            vars.completedsplits.Add("blue hole");
            return true;
        }

        if (current.WaterChests == 20 && !vars.completedsplits.Contains("antocha wreck"))
        {
            vars.completedsplits.Add("antocha wreck");
            return true;
        }

        if (current.WaterChests == 28 && !vars.completedsplits.Contains("Devils eye caverns"))
        {
            vars.completedsplits.Add("Devils eye caverns");
            return true;
        }

        if (current.WaterChests == 35 && !vars.completedsplits.Contains("La Concepcion"))
        {
            vars.completedsplits.Add("La Concepcion");
            return true;
        }

        if (current.WaterChests == 42 && !vars.completedsplits.Contains("Black trench"))
        {
            vars.completedsplits.Add("Black trench");
            return true;
        }

        if (current.WaterChests == 50 && !vars.completedsplits.Contains("Kabah ruins"))
        {
            vars.completedsplits.Add("Kabah ruins");
            return true;
        }
    }

    if (settings["Templar Hunts"])
    {
        if ((current.PercentageF >= old.PercentageF + 0.38579 && current.PercentageF <= old.PercentageF + 0.38582) 
        && current.loading == 0)
        {
            print("Templar Hunts Split");
            return true;
        }
    }

    if (settings["Legendary Ships"])
    {

        //splits when defeating one of the legendary ships
        if (current.PercentageF == old.PercentageF + 0.18750 && current.loading == 0)
        {
            print("Legendary Ships Split");
            return true;
        }
    }

}

onReset
{
    vars.completedsplits.Clear();
    vars.stopwatch.Reset();
    vars.Forts = 0;
    vars.Taverns = 0;
    vars.NavalContracts = 0;
}

isLoading
{
    return current.loading == 1 || current.MainMenu == 65540;
    
}

exit
{
    //pauses timer if the game crashes
	timer.IsGameTimePaused = true;
    vars.stopwatch.Stop();
    vars.IsStopwatchStop = true;
}

/*
--money stuff--
    /*stuff that splits on gaining money
    1- percentages wont be enough so money
    2- for doesn't matter percentages will be fine
    4- can use percentages and money for the split conditions
    5- wont use
    300-"Proper Defenses", 2
    350-Templar hunts, 2
    500-Templar hunts, "Mister Walpole, I Presume", 2
    700-Templar hunts, 2
    1000-Assassin contracts(4),Forts(4),chests shipwreck(5), "Unmanned"(2)
    1200-Naval contracts(4)
    1500-Assassin contracts(4),Buried treasure(4),Templar hunt(2)
    1800-Naval contracts (4)
    2400-Naval contracts (4)
    3000-Forts,Buried treasure(3)
    4000-Buried treasure(4)
    5000-Forts(4)
    10000-Legendary ships,buried treasure(3) (4)
    20000-Legendary ships(4)
*/
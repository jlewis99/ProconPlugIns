// BF4 Gunmaster Random Presets - Limit 1 of 1
// v3.0 - OnRoundOver - first_check
//

Thread gmrnd = new Thread(
    new ThreadStart(
        delegate
        {
            try
            {
                // USER SETTINGS
                //
                int iDelay = 0;
                int lastPreset = 0;
                bool ensureNight = true;
                bool allowStandard = true;
                bool allowClassic = true;
                bool allowPistol = true;
                bool allowDLC = true;
                bool allowTroll = true;
                bool allowNight = true;
                bool showChat = true;
                bool showYell = true;
                bool showProcon = true;
                //
                // END OF USER SETTINGS
                if (iDelay > 0)
                {
                    Thread.Sleep(iDelay * 1000);
                }
                plugin.SendGlobalYell("\n" + "NextGameMode:" + server.NextGamemode, 0);
                if (server.NextGamemode == "GunMaster0" || server.NextGamemode == "GunMaster1")
                {
                    bool bGetting = true;
                    int nextPreset = 0;
                    int maxPreset = 6;
                    Random rnd = new Random();
                    String lastKey = "_LASTGM_";
                    String[] presets = { "Standard",
                                         "Classic",
                                         "Pistol",
                                         "DLC",
                                         "Troll",
                                         "Night" };
                    String msg = "Next GunMaster preset will be: ";
                    if (server.Data.issetInt(lastKey)) lastPreset = server.Data.getInt(lastKey);
                    nextPreset = rnd.Next(maxPreset);
                    if (ensureNight && server.NextMapFileName == "XP5_Night_01")
                    {
                        nextPreset = 5;
                    }
                    else
                    {
                        while (bGetting)
                        {
                            nextPreset = rnd.Next(maxPreset);
                            // make sure that the next preset is not the same as the last unless all options are turned off -- JL 01/29/2016
                            if (Convert.ToInt32(allowStandard) + Convert.ToInt32(allowClassic) + Convert.ToInt32(allowPistol) + Convert.ToInt32(allowDLC) + Convert.ToInt32(allowTroll) + Convert.ToInt32(allowNight) > 1) // make sure more than one option is enabled
                                while (nextPreset == lastPreset)
                                    nextPreset = rnd.Next(maxPreset);
                            if (showProcon) plugin.PRoConChat("Finished Randomizing " + "^b^1" + presets[nextPreset] + "^0^n.");
                            if (showChat) plugin.SendGlobalMessage("Finished Randomizing " + presets[nextPreset]);

                            if (!allowStandard && nextPreset == 0) nextPreset = lastPreset;
                            if (!allowClassic && nextPreset == 1) nextPreset = lastPreset;
                            if (!allowPistol && nextPreset == 2) nextPreset = lastPreset;
                            if (!allowDLC && nextPreset == 3) nextPreset = lastPreset;
                            if (!allowTroll && nextPreset == 4) nextPreset = lastPreset;
                            if (!allowNight && nextPreset == 5) nextPreset = lastPreset;
                            if (nextPreset != lastPreset) bGetting = false;
                        }
                    }
                    plugin.ServerCommand("vars.gunMasterWeaponsPreset", nextPreset.ToString());
                    if (showChat) plugin.SendGlobalMessage(msg + presets[nextPreset]);
                    if (showYell) plugin.SendGlobalYell("\n" + msg + presets[nextPreset], 8);
                    if (showProcon) plugin.PRoConChat(msg + "^b^1" + presets[nextPreset] + "^0^n.");
                    server.Data.setInt(lastKey, nextPreset);
                }
            }
            catch (Exception e)
            {
                plugin.ConsoleException(e.ToString());
            }
        }
    )
);

gmrnd.Name = "GMPresetRandomizer";
gmrnd.Start();

return false;

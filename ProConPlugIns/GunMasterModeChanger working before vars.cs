/* 
 *   GunMaster Mode Changer
 *   https://forum.myrcon.com/showthread.php?
 *
 * Note: After starting, the mode displayed may be incorrect since the server could be on any gunmaster mode.
 *       It should be correct after then.
 * [00:26:32] vars.gunMasterWeaponsPreset 2
 * [00:26:36] mapList.restartRound
 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//using System.Drawing;


using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using System.Net;

namespace PRoConEvents
{
    public class pGunMasterRandomizer : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region Variables and Constructors
        private string LastGMMode = "_LastGunMasterMode_";

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;
        private bool m_isPluginEnabled;

        // User Settings
        int iDelay = 30;
        int nextPreset = 0;
        int lastPreset = 0;
        int maxRand = 4;

        string sLastLoadedLevel = "";
        bool doReset = false;

        //vars.gunMasterWeaponsPreset 0 = Classic
        //vars.gunMasterWeaponsPreset 1 = Standard
        //vars.gunMasterWeaponsPreset 2 = Pistols
        //vars.gunMasterWeaponsPreset 3 = DLC
        //vars.gunMasterWeaponsPreset 4 = Troll

        Random rnd = null;

        String[] Presets = { "Standard", "Classic", "Pistols", "DLC", "Troll" };
        #endregion
        
        public pGunMasterRandomizer()
        {
            this.m_isPluginEnabled = false;
        }

        public string GetPluginName()
        {
            return "Gun Master Preset Randomizer";
        }

        public string GetPluginVersion()
        {
            return "0.0.0.1";
        }

        public string GetPluginAuthor()
        {
            return "ByteMe!";
        }

        public string GetPluginWebsite()
        {
            return "www.nowhere.net";
        }

        public string GetPluginDescription()
        {
            return @"
<h2>Description</h2>
    <p>Quick and Diry Gun Master Preset Randomizer.</p>
";
        }

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;

            this.RegisterEvents(this.GetType().Name, "OnLevelLoaded","OnLoadingLevel");
        }

        public void OnPluginEnable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Preset Randomizer ^2Enabled!");

            this.m_isPluginEnabled = true;
            //this.RegisterAllCommands();

            // Initialize to mode 0 so we know where we are since we can't read the mode
            this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Mode is now in " + Presets[nextPreset] + " mode. ^2");
        }

        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Preset Randomizer ^1Disabled =(");

            this.m_isPluginEnabled = false;
            //this.UnregisterAllCommands();
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            return lstReturn;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
            base.OnLoadingLevel(mapFileName, roundsPlayed, roundsTotal);

            this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            base.OnLevelLoaded(mapFileName, gamemode, roundsPlayed, roundsTotal);

            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bmapFileName:" + mapFileName + ": ^2");
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bgamemode:" + gamemode + " ^2");


            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Level loaded.  gamemode:" + gamemode+" ^2");
            String sLevelLoaded = mapFileName + gamemode;
            doReset = false;
            if (sLastLoadedLevel != sLevelLoaded)
            {
                sLastLoadedLevel = sLevelLoaded;
                doReset = true;
            }

            if (gamemode == "GunMaster0"  || gamemode == "GunMaster1")
            {
                //this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster randomizing. ^2");
                lastPreset = nextPreset;
                //this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster nextPreset: " + nextPreset.ToString()+". ^2");
                if (doReset)
                {
                    if (rnd == null)
                        rnd = new Random();

                    // support night mode for Zavod Graveyard Shift (NOTE: The there are two versions of this but both have the same mapFileName so it should work on both)
                    if (mapFileName == "XP5_Night_01")
                    {
                        nextPreset = 5;
                    }
                    else
                    {
                        while (lastPreset == nextPreset)
                            nextPreset = rnd.Next(1, maxRand + 1);  // NOTE: We add one because when maxRand is hit, minRand is returned
                    }

                    this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
                    //this.ExecuteCommand("procon.protected.send", "admin.say", "GunMaster Mode is now in " + Presets[nextPreset]+" mode.", "all");
                    //this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Mode is now in " + Presets[nextPreset] + " mode. ^2");


                    this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                    doReset = false;
                }
                else
                {
                    // Only notify if we are currently starting GunMaster
                    this.ExecuteCommand("procon.protected.send", "admin.say", "GunMaster Mode is now in " + Presets[nextPreset] + " mode.", "all");
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Mode is now in " + Presets[nextPreset] + " mode. ^2");
                }
            }
        }
    }
}

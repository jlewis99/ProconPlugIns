/* 
 *   GunMaster Mode Changer
 *   https://forum.myrcon.com/showthread.php?
 *
 *   Version 1.0
 *   ByteMe  02/12/2016
 *   Tested with BATTLEFIELD 4
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

        //vars.gunMasterWeaponsPreset 0 = Classic
        //vars.gunMasterWeaponsPreset 1 = Standard
        //vars.gunMasterWeaponsPreset 2 = Pistols
        //vars.gunMasterWeaponsPreset 3 = DLC
        //vars.gunMasterWeaponsPreset 4 = Troll
        private enumBoolYesNo m_EnableClassicMode = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableStandardMode = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableTrollMode = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableNightOnlyGunsForNightGM = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnablePistolsMode = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableDLCMode = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableAllowConsecutiveGMModes = enumBoolYesNo.Yes;
        private enumBoolYesNo m_EnableRandomizedGMModes = enumBoolYesNo.No;

        // User Settings
        int iDelay = 30;
        int nextPreset = 0;
        int lastPreset = 0;
        int maxRand = 4;

        string sLastLoadedLevel = "";
        bool doReset = false;

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
            return "1.0.0.0";
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
    <p>If you liked my plugins, please show your support!</p>
    <form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
    <input type=""hidden"" name=""cmd"" value=""_s-xclick"">
    <input type=""hidden"" name=""hosted_button_id"" value=""QVJ35CRQQL994"">
    <input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
    <img alt="" "" border=""0"" src=""https://www.paypalobjects.com/en_US/i/scr/pixel.gif"" width=""1"" height=""1"">
    </form>
    </p>
    <h2>Description</h2>
    <p><h3>Gun Master Preset Randomizer.</h3></p>
    <p>This plugin is designed to use the GunMaster Presets on all BF4 gunmaster maps even if the server is not a dedicated GunMaster Only server.</p>
    <p>To be as flexable possible, you can control exactly which Gun Master modes you want to be available on the server by allowing or disallowing each one.</br>  These modes are as follows:</p>
    <p>
        <h3>Allow Standard Mode:</h3> If allowed, Standard mode will have the following weapon rotation:</br>
        Standard (Weapons Preset 0)
		<ul>
        <li>A-91 with Compensator and Stubby Grip</li>
        <li>M60-E4 with M145, Compensator and Ergo Grip</li>
        <li>AN-94 with PK-A, Heavy Barrel and Stubby Grip</li>
        <li>DEagle 44 with Compensator</li>
        <li>QBU-88 with JGM-4, Laser Sight, Muzzle Brake and Ergo Grip</li>
        <li>M249 with HOLO, Magnifier, Compensator and Bipod</li>
        <li>Saiga 12K with PKA-S and Ergo Grip</li>
        <li>QBZ-95-1 with Prisma, Green Laser Sight and Vertical Grip</li>
        <li>870 MCS with HD-33, Modified Choke and 12G Dart</li>
        <li>UMP-45 with HOLO, Laser Sight and Potato Grip</li>
        <li>M4 with HOLO and Angled Grip</li>
        <li>AS VAL with Kobra, Green Laser Sight and Angled Grip</li>
        <li>L96A1 with M145, Green Laser Sight, Flash Hider and Straight Pull Bolt</li>
        <li>Unica 6 with Mini</li>
        <li>G18 with Muzzle Brake</li>
        <li>P226 with Green Laser Sight</li>
        <li>Bayonet (Knife)</li>
		</ul>

    <p>
        <h3>Allow Classic Mode:</h3> If allowed, Classic mode will have the following weapon rotation:</br>
        Classic (Weapons Preset 1)
		<ul>
        <li>MP443</li>
        <li>93R</li>
        <li>.44 Magnum</li>
        <li>MP7</li>
        <li>P90</li>
        <li>SPAS-12</li>
        <li>USAS-12</li>
        <li>ACW-R</li>
        <li>MTAR-21</li>
        <li>AUG A3</li>
        <li>SAR-21</li>
        <li>LSAT</li>
        <li>L86A2</li>
        <li>SCAR-H</li>
        <li>JNG-90</li>
        <li>M320 LVG</li>
        <li>Bayonet (Knife)</li>
		</ul>
    <p>
        <h3>Allow Pistol Mode:</h3> If allowed, Pistol mode will have the following weapon rotation:</br>
        Pistol (Weapons Preset 2)
		<ul>
        <li>P226</li>
        <li>M9</li>
        <li>QSZ-92</li>
        <li>MP443</li>
        <li>Shorty 12G</li>
        <li>G18</li>
        <li>FN57</li>
        <li>M1911</li>
        <li>93R</li>
        <li>CZ-75</li>
        <li>.44 Magnum</li>
        <li>Compact 45</li>
        <li>M412 REX</li>
        <li>SW40</li>
        <li>Unica 6</li>
        <li>DEagle 44</li>
        <li>Mare's Leg</li>
        <li>Survival (Knife)</li>
		</ul>
    <p>
        <h3>Allow DLC Mode:</h3> If allowed, DLC mode will have the following weapon rotation:</br>
        DLC (Weapons Preset 3)
		<ul>
        <li>F2000</li>
        <li>DAO-12</li>
        <li>M60-E4</li>
        <li>GOL Magnum</li>
        <li>L85A2</li>
        <li>SR-2</li>
        <li>AWS</li>
        <li>SR338</li>
        <li>SW40</li>
        <li>MPX</li>
        <li>Bulldog</li>
        <li>CS5</li>
        <li>Unica 6</li>
        <li>DEagle 44</li>
        <li>Rorsch Mk-1</li>
        <li>Phantom</li>
        <li>Icicle (Knife)</li>
		</ul>
    <p>
        <h3>Allow Troll Mode:</h3> If allowed, Troll mode will have the following weapon rotation:</br>
        Troll (Weapons Preset 4)
		<ul>
        <li>Ballistic Shield</li>
        <li>Phantom</li>
        <li>Hawk 12G</li>
        <li>FAMAS</li>
        <li>AEK-971</li>
        <li>Shorty 12G</li>
        <li>Phantom</li>
        <li>RPK-12</li>
        <li>QBZ-95B-1</li>
        <li>M4</li>
        <li>ACW-R</li>
        <li>Phantom with Explosive Tip</li>
        <li>SVD-12</li>
        <li>M98B</li>
        <li>M1911</li>
        <li>Defibrillator</li>
        <li>C100 (Knife)</li>
		</ul>
    <p>
        <h3>Force Night Mode only for Night Maps</h3> mode will have the following weapon rotation on both Zavod 311 and Zavod: Graveyard Shift:</br>
        Has the following with INV, FLIR or flash light attachments
		<ul>
        <li>A-91 with Compensator and Stubby Grip</li>
        <li>M60-E4 with M145, Compensator and Ergo Grip</li>
        <li>AN-94 with PK-A, Heavy Barrel and Stubby Grip</li>
        <li>DEagle 44 with Compensator</li>
        <li>QBU-88 with JGM-4, Laser Sight, Muzzle Brake and Ergo Grip</li>
        <li>M249 with HOLO, Magnifier, Compensator and Bipod</li>
        <li>Saiga 12K with PKA-S and Ergo Grip</li>
        <li>QBZ-95-1 with Prisma, Green Laser Sight and Vertical Grip</li>
        <li>870 MCS with HD-33, Modified Choke and 12G Dart</li>
        <li>UMP-45 with HOLO, Laser Sight and Potato Grip</li>
        <li>M4 with HOLO and Angled Grip</li>
        <li>AS VAL with Kobra, Green Laser Sight and Angled Grip</li>
        <li>L96A1 with M145, Green Laser Sight, Flash Hider and Straight Pull Bolt</li>
        <li>Unica 6 with Mini</li>
        <li>G18 with Muzzle Brake</li>
        <li>P226 with Green Laser Sight</li>
        <li>Bayonet (Knife)</li>
		</ul>
        </p>
    <p><h3>Allow Consecutive GunMaster Modes:</h3> Turn this off to prevent the next Gun Master game from having the same Gun Master mode as the current Gun Master game.</p>
    <p><h3>Allow Randomized GunMaster Modes:</h3> This forces GunMaster modes to be selected at random.  If not allowed the Gun Master modes simply change modes in order from Standard to Classic to Pistols to DLC to Troll and then back to Standard.  If a mode is not allowed it will skip that mode.</p>
    <p><h4>As a side note:  If all modes are not allowed, the Standard mode is forced, even if not allowed since GunMaster is required to have atleast one mode.</h4></p>
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

            // Initialize to mode 0 so we know where we are since we can't read the mode
            this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Mode is now in " + Presets[nextPreset] + " mode. ^2");
        }

        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Preset Randomizer ^1Disabled =(");

            this.m_isPluginEnabled = false;
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstPluginVariables = new List<CPluginVariable>();
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Standard Mode", typeof(enumBoolYesNo), this.m_EnableStandardMode));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Classic Mode", typeof(enumBoolYesNo), this.m_EnableClassicMode));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Pistols Mode", typeof(enumBoolYesNo), this.m_EnablePistolsMode));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow DLC Mode", typeof(enumBoolYesNo), this.m_EnableDLCMode));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Troll Mode", typeof(enumBoolYesNo), this.m_EnableTrollMode));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Force Night Mode only for Night Maps", typeof(enumBoolYesNo), this.m_EnableNightOnlyGunsForNightGM));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Consecutive GunMaster Modes", typeof(enumBoolYesNo), this.m_EnableAllowConsecutiveGMModes));
            lstPluginVariables.Add(new CPluginVariable("Gunmaster Modes|Allow Randomized GunMaster Modes", typeof(enumBoolYesNo), this.m_EnableRandomizedGMModes));

            return lstPluginVariables;
        }

        // Lists all of the plugin variables.
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstPluginVariables = new List<CPluginVariable>();
            lstPluginVariables.Add(new CPluginVariable("Allow Standard Mode", typeof(enumBoolYesNo), this.m_EnableStandardMode));
            lstPluginVariables.Add(new CPluginVariable("Allow Classic Mode", typeof(enumBoolYesNo), this.m_EnableClassicMode));
            lstPluginVariables.Add(new CPluginVariable("Allow Pistols Mode", typeof(enumBoolYesNo), this.m_EnablePistolsMode));
            lstPluginVariables.Add(new CPluginVariable("Allow DLC Mode", typeof(enumBoolYesNo), this.m_EnableDLCMode));
            lstPluginVariables.Add(new CPluginVariable("Allow Troll Mode", typeof(enumBoolYesNo), this.m_EnableTrollMode));
            lstPluginVariables.Add(new CPluginVariable("Force Night Mode only for Night Maps", typeof(enumBoolYesNo), this.m_EnableNightOnlyGunsForNightGM));
            lstPluginVariables.Add(new CPluginVariable("Allow Consecutive GunMaster Modes", typeof(enumBoolYesNo), this.m_EnableAllowConsecutiveGMModes));
            lstPluginVariables.Add(new CPluginVariable("Allow Randomized GunMaster Modes", typeof(enumBoolYesNo), this.m_EnableRandomizedGMModes));

            return lstPluginVariables;
        }

        public void SetPluginVariable(string strVariable, string strValue)
        {
            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^bSetPluginVariable:" + strVariable + ": ^2");
            //this.ExecuteCommand("procon.protected.pluginconsole.write", "^b Value:" + strValue + ": ^2");

            if (strVariable.CompareTo("Allow Classic Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableClassicMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow Standard Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableStandardMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow Pistols Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnablePistolsMode = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow DLC Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableDLCMode= (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow Troll Mode") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableTrollMode= (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Force Night Mode only for Night Maps") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableNightOnlyGunsForNightGM = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow Consecutive GunMaster Modes") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableAllowConsecutiveGMModes = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("Allow Randomized GunMaster Modes") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.m_EnableRandomizedGMModes = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            };
        }

        public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal)
        {
            base.OnLoadingLevel(mapFileName, roundsPlayed, roundsTotal);

            this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
        }

        public override void OnLevelLoaded(string mapFileName, string gamemode, int roundsPlayed, int roundsTotal)
        {
            base.OnLevelLoaded(mapFileName, gamemode, roundsPlayed, roundsTotal);

            int iCountOfEnabledGameModes = CountOfEnabledGMModes();

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
                    if ((m_EnableNightOnlyGunsForNightGM == enumBoolYesNo.Yes) && (mapFileName == "XP5_Night_01"))
                    {
                        nextPreset = 5;
                    }
                    else if (iCountOfEnabledGameModes == 0)  // if all modes are not allowed, force classic anyways
                    {
                        nextPreset = 0;
                    }
                    else if (iCountOfEnabledGameModes == 1)  // Only 1 mode is enabled, Don't Randomize and force that mode
                    {
                        if (m_EnableStandardMode == enumBoolYesNo.Yes)
                            nextPreset = 0;
                        else if (m_EnableClassicMode == enumBoolYesNo.Yes)
                            nextPreset = 1;
                        else if (m_EnablePistolsMode == enumBoolYesNo.Yes)
                            nextPreset = 2;
                        else if (m_EnableDLCMode == enumBoolYesNo.Yes)
                            nextPreset = 3;
                        else if (m_EnableTrollMode == enumBoolYesNo.Yes)
                            nextPreset = 4;
                    }
                    else // now we know atleast 2 modes are allowed
                    {
                        bool bMapSelected = false;
                        if (m_EnableRandomizedGMModes == enumBoolYesNo.No)  // Rotate Mode
                        {
                            while (!bMapSelected)
                            {
                                nextPreset++;
                                if (nextPreset > 4)
                                    nextPreset = 0;

                                if ((nextPreset == 0) && (m_EnableStandardMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 1) && (m_EnableClassicMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 2) && (m_EnablePistolsMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 3) && (m_EnableDLCMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 4) && (m_EnableTrollMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((m_EnableAllowConsecutiveGMModes == enumBoolYesNo.No) && (lastPreset == nextPreset)) bMapSelected = false; // Force it to rerandomize
                            }
                        }
                        else // Randomize
                        {
                            while (!bMapSelected)
                            {
                                nextPreset = rnd.Next(0, maxRand + 1);  // NOTE: We add one because when maxRand is hit, minRand is returned

                                if ((nextPreset == 0) && (m_EnableStandardMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 1) && (m_EnableClassicMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 2) && (m_EnablePistolsMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 3) && (m_EnableDLCMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((nextPreset == 4) && (m_EnableTrollMode == enumBoolYesNo.Yes)) bMapSelected = true;
                                if ((m_EnableAllowConsecutiveGMModes == enumBoolYesNo.No) && (lastPreset == nextPreset)) bMapSelected = false; // Force it to rerandomize
                            }
                        }

                        //while (lastPreset == nextPreset)
                        //    nextPreset = rnd.Next(0, maxRand + 1);  // NOTE: We add one because when maxRand is hit, minRand is returned
                    }

                    this.ExecuteCommand("procon.protected.send", "vars.gunMasterWeaponsPreset", nextPreset.ToString());
                    //this.ExecuteCommand("procon.protected.send", "admin.say", "GunMaster Mode is now in " + Presets[nextPreset]+" mode.", "all");
                    this.ExecuteCommand("procon.protected.pluginconsole.write", "^bGunMaster Mode is now in " + Presets[nextPreset] + " mode. ^2");

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

        public int CountOfEnabledGMModes()
        {
            int iCount = 0;
            if (m_EnableStandardMode == enumBoolYesNo.Yes) iCount++;
            if (m_EnableClassicMode == enumBoolYesNo.Yes) iCount++;
            if (m_EnablePistolsMode == enumBoolYesNo.Yes) iCount++;
            if (m_EnableDLCMode == enumBoolYesNo.Yes) iCount++;
            if (m_EnableTrollMode == enumBoolYesNo.Yes) iCount++;
            return iCount;
        }
    }
}

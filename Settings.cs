using System.Collections.Generic;
using BepInEx.Configuration;
using EFT.Vaulting;

#pragma warning disable IDE0007

namespace NoBushESP
{
    internal class Settings
    {

        public static ConfigEntry<bool> DebugEnabled;


        public static void Init(ConfigFile Config)
        {
            const string GeneralSectionTitle = "General";

            var name = "Debug Enabled";
            var description = "Enable extra debug log messages";
            var defaultBool = false;

            DebugEnabled = Config.Bind(
                GeneralSectionTitle, name, defaultBool,
                new ConfigDescription(description, null,
                new ConfigurationManagerAttributes { }
                ));


        }

    }

}
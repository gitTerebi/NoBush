using BepInEx;
using BepInEx.Logging;
namespace NoBushESP
{

    [BepInPlugin("com.somtam.NoBush", "NoBush", "1.0.0")]
    class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        private void Awake()
        {
            LogSource = Logger;

            Settings.Init(Config);

            new NoBushPatch().Enable();
        }

        public void Start()
        {

        }

    }


}

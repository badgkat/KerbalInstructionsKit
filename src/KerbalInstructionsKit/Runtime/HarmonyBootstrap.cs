using UnityEngine;

namespace KerbalInstructionsKit.Runtime
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class HarmonyBootstrap : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            new HarmonyLib.Harmony("badgkat.kerbalinstructionskit").PatchAll();
            Debug.Log("[KIK] Harmony patches applied");
        }
    }
}

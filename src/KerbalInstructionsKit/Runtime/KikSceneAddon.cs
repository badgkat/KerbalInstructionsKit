using KerbalInstructionsKit.Rendering;
using ToolbarControl_NS;
using UnityEngine;

namespace KerbalInstructionsKit.Runtime
{
    internal static class KikToolbarHelper
    {
        internal const string Namespace = "KerbalInstructionsKit";
        internal const string ToolbarId = "KIK_button";
        internal const string LargeIcon = "KerbalInstructionsKit/Plugins/Icons/KIK_icon_38";
        internal const string SmallIcon = "KerbalInstructionsKit/Plugins/Icons/KIK_icon_24";
        internal const string Tooltip = "Kerbal Instructions Kit";

        internal static ToolbarControl CreateToolbar(MonoBehaviour host, System.Action onClick)
        {
            var toolbar = host.gameObject.AddComponent<ToolbarControl>();
            toolbar.AddToAllToolbars(
                () => onClick(),
                () => onClick(),
                KSP.UI.Screens.ApplicationLauncher.AppScenes.SPACECENTER
                | KSP.UI.Screens.ApplicationLauncher.AppScenes.FLIGHT
                | KSP.UI.Screens.ApplicationLauncher.AppScenes.VAB
                | KSP.UI.Screens.ApplicationLauncher.AppScenes.SPH
                | KSP.UI.Screens.ApplicationLauncher.AppScenes.TRACKSTATION,
                Namespace,
                ToolbarId,
                LargeIcon,
                SmallIcon,
                Tooltip);
            return toolbar;
        }
    }

    public abstract class KikSceneAddonBase : MonoBehaviour
    {
        private LessonPanel panel;
        private ToolbarControl toolbar;
        private KerbalInstructionsKitScenario scenario;

        protected abstract IPauseController CreatePauseController();

        public void Start()
        {
            scenario = FindObjectOfType<KerbalInstructionsKitScenario>();
            if (scenario == null)
            {
                Debug.LogWarning("[KIK] Scenario not found in scene, addon inactive");
                return;
            }

            panel = new LessonPanel(scenario, CreatePauseController());
            toolbar = KikToolbarHelper.CreateToolbar(this, () => panel.Toggle());

            scenario.PanelOpenLesson = id => panel.OpenLesson(id);
            scenario.PanelOpenArchive = () => panel.OpenArchive();
            scenario.PanelClose = () => panel.Close();
        }

        public void OnGUI()
        {
            panel?.Draw();
        }

        public void OnDestroy()
        {
            panel?.Dispose();
            panel = null;
            if (toolbar != null)
            {
                toolbar.OnDestroy();
                Destroy(toolbar);
            }
            if (scenario != null)
            {
                scenario.PanelOpenLesson = null;
                scenario.PanelOpenArchive = null;
                scenario.PanelClose = null;
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public sealed class KikSpaceCenterAddon : KikSceneAddonBase
    {
        protected override IPauseController CreatePauseController() => new NullPauseController();
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public sealed class KikFlightAddon : KikSceneAddonBase
    {
        protected override IPauseController CreatePauseController() => new FlightPauseController();
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public sealed class KikEditorAddon : KikSceneAddonBase
    {
        protected override IPauseController CreatePauseController() => new NullPauseController();
    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public sealed class KikTrackingStationAddon : KikSceneAddonBase
    {
        protected override IPauseController CreatePauseController() => new NullPauseController();
    }
}

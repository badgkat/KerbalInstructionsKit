using System;
using System.Reflection;
using KerbalInstructionsKit.Core;
using UnityEngine;

namespace KerbalInstructionsKit.Rendering
{
    public sealed class LinkRouter
    {
        private readonly Action<string> openLessonViaLink;

        public LinkRouter(Action<string> openLessonViaLink)
        {
            this.openLessonViaLink = openLessonViaLink;
        }

        public void Activate(Link link)
        {
            switch (link.Type)
            {
                case LinkType.Lesson:
                    openLessonViaLink?.Invoke(link.Target);
                    break;
                case LinkType.Kspedia:
                    OpenKspedia(link.Target);
                    break;
                case LinkType.Url:
                    OpenUrl(link.Target);
                    break;
            }
        }

        private void OpenKspedia(string page)
        {
            try
            {
                var asm = typeof(GameDatabase).Assembly;
                var type = asm.GetType("KSPAssets.KSPedia.KSPediaSpawner");
                if (type == null)
                {
                    Debug.LogWarning($"[KIK] KSPediaSpawner type not found; cannot open KSPedia ('{page}')");
                    return;
                }
                MethodInfo show = null;
                foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (m.Name != "Show") continue;
                    var p = m.GetParameters();
                    if (p.Length == 2 && p[0].ParameterType == typeof(string))
                    {
                        show = m;
                        break;
                    }
                }
                if (show == null)
                {
                    Debug.LogWarning($"[KIK] KSPediaSpawner.Show(string, Button) not found; cannot open KSPedia ('{page}')");
                    return;
                }
                show.Invoke(null, new object[] { page, null });
            }
            catch (Exception e) { Debug.LogWarning($"[KIK] failed to open KSPedia ('{page}'): {e.Message}"); }
        }

        private void OpenUrl(string url)
        {
            try { Application.OpenURL(url); }
            catch (Exception e) { Debug.LogWarning($"[KIK] failed to open URL '{url}': {e.Message}"); }
        }
    }
}

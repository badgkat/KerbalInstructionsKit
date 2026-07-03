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
                var spawnerType = asm.GetType("KSPAssets.KSPedia.KSPediaSpawner");
                if (spawnerType == null)
                {
                    Debug.LogWarning($"[KIK] KSPediaSpawner type not found; cannot open KSPedia ('{page}')");
                    return;
                }

                // Log available Show overloads so we can discover the right API
                foreach (var m in spawnerType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var parms = m.GetParameters();
                    var sig = string.Join(", ", Array.ConvertAll(parms, p => $"{p.ParameterType.Name} {p.Name}"));
                    Debug.Log($"[KIK] KSPediaSpawner.{m.Name}({sig}) -> {m.ReturnType.Name}");
                }

                // Try Show(string, Button) — the string may be a page reference
                MethodInfo show = null;
                foreach (var m in spawnerType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (m.Name != "Show") continue;
                    var p = m.GetParameters();
                    if (p.Length == 2 && p[0].ParameterType == typeof(string))
                    {
                        show = m;
                        break;
                    }
                }
                if (show != null)
                {
                    Debug.Log($"[KIK] calling KSPediaSpawner.Show('{page}', null)");
                    show.Invoke(null, new object[] { page, null });
                }
                else
                {
                    // Fallback: try no-arg Show
                    var showAny = spawnerType.GetMethod("Show", BindingFlags.Public | BindingFlags.Static);
                    if (showAny != null)
                    {
                        var parms = showAny.GetParameters();
                        Debug.Log($"[KIK] no Show(string,Button) found, calling Show with {parms.Length} null args");
                        showAny.Invoke(null, new object[parms.Length]);
                    }
                    else
                        Debug.LogWarning($"[KIK] no Show method found on KSPediaSpawner");
                }
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

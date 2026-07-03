using System;
using System.Collections;
using Contracts;
using HarmonyLib;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Triggers;
using TMPro;
using UnityEngine;
namespace KerbalInstructionsKit.Runtime
{
    [HarmonyPatch(typeof(KSP.UI.Screens.MissionControl), "UpdateInfoPanelContract")]
    public static class ContractDetailsPatch
    {
        internal const string LinkTag = "kik_lesson";

        public static void Postfix(KSP.UI.Screens.MissionControl __instance, Contract __0)
        {
            if (__0 == null) return;
            try
            {
                var lessonId = ResolveLessonId(__0);
                if (lessonId != null)
                    __instance.StartCoroutine(InjectAfterFrame(__instance, __0, lessonId));
            }
            catch (Exception e) { Debug.LogWarning($"[KIK] contract-window patch error: {e}"); }
        }

        private static string ResolveLessonId(Contract contract)
        {
            var typeName = CcIntegration.GetContractTypeName(contract);
            var bindings = AttachLessonRegistry.Get(typeName);
            if (bindings == null || bindings.Count == 0) return null;

            var state = InstructionsKit.State;
            if (state == null) return null;

            foreach (var binding in bindings)
            {
                if (!binding.ShowButton) continue;
                if (!state.IsUnlocked(binding.LessonId))
                {
                    if (ShouldBeUnlocked(contract, binding.UnlockOn))
                        state.Unlock(binding.LessonId);
                    else
                        continue;
                }
                return binding.LessonId;
            }
            return null;
        }

        private static IEnumerator InjectAfterFrame(
            KSP.UI.Screens.MissionControl mc, Contract contract, string lessonId)
        {
            yield return new WaitForEndOfFrame();
            yield return null;
            try { AppendLinkToDescription(mc, contract, lessonId); }
            catch (Exception e) { Debug.LogWarning($"[KIK] contract-link inject error: {e}"); }
        }

        private static void AppendLinkToDescription(
            KSP.UI.Screens.MissionControl mc, Contract contract, string lessonId)
        {
            var descriptionText = contract.Description;
            if (string.IsNullOrEmpty(descriptionText)) return;

            var texts = mc.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI target = null;
            foreach (var t in texts)
            {
                if (t.text != null && t.text.Contains(descriptionText))
                {
                    target = t;
                    break;
                }
            }

            if (target == null) return;
            if (target.text.Contains(LinkTag)) return;

            target.text += "\n\n" +
                $"<link=\"{LinkTag}\">" +
                "<color=#82B4E8><u>View Instructions</u></color>" +
                "</link>";
            target.ForceMeshUpdate();

            var handler = target.gameObject.GetComponent<ContractLinkClickHandler>()
                       ?? target.gameObject.AddComponent<ContractLinkClickHandler>();
            handler.Init(target, lessonId);
        }

        private static bool ShouldBeUnlocked(Contract c, ContractState requiredState)
        {
            switch (requiredState)
            {
                case ContractState.Offered:
                    return c.ContractState == Contract.State.Offered
                        || c.ContractState == Contract.State.Active
                        || c.ContractState == Contract.State.Completed;
                case ContractState.Accepted:
                    return c.ContractState == Contract.State.Active
                        || c.ContractState == Contract.State.Completed;
                case ContractState.Completed:
                    return c.ContractState == Contract.State.Completed;
                case ContractState.Failed:
                    return c.ContractState == Contract.State.Failed
                        || c.ContractState == Contract.State.DeadlineExpired;
                default: return false;
            }
        }
    }

    public sealed class ContractLinkClickHandler : MonoBehaviour
    {
        private TMP_Text textComponent;
        private string lessonId;
        private Camera uiCamera;

        public void Init(TMP_Text text, string lesson)
        {
            textComponent = text;
            lessonId = lesson;
            var canvas = text.GetComponentInParent<Canvas>();
            uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;
            Debug.Log($"[KIK] ContractLinkClickHandler initialized for lesson '{lesson}'");
        }

        public void Update()
        {
            if (textComponent == null || string.IsNullOrEmpty(lessonId)) return;
            if (!Input.GetMouseButtonDown(0)) return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                textComponent, Input.mousePosition, uiCamera);
            if (linkIndex < 0) return;

            var info = textComponent.textInfo.linkInfo[linkIndex];
            if (info.GetLinkID() == ContractDetailsPatch.LinkTag)
            {
                Debug.Log($"[KIK] View Instructions clicked for lesson '{lessonId}'");
                InstructionsKit.OpenLesson(lessonId);
            }
        }
    }
}

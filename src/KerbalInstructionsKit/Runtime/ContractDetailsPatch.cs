using System;
using System.Reflection;
using Contracts;
using HarmonyLib;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KerbalInstructionsKit.Runtime
{
    [HarmonyPatch(typeof(KSP.UI.Screens.MissionControl), "UpdateInfoPanelContract")]
    public static class ContractDetailsPatch
    {
        private const string LinkTag = "kik_lesson";

        public static void Postfix(KSP.UI.Screens.MissionControl __instance, Contract contract)
        {
            try { TryInjectLink(__instance, contract); }
            catch (Exception e) { Debug.LogWarning($"[KIK] contract-window patch error: {e}"); }
        }

        private static void TryInjectLink(KSP.UI.Screens.MissionControl mc, Contract contract)
        {
            if (contract == null) return;

            var typeName = CcIntegration.GetContractTypeName(contract);
            var bindings = AttachLessonRegistry.Get(typeName);
            if (bindings == null || bindings.Count == 0) return;

            var state = InstructionsKit.State;
            if (state == null) return;

            string lessonId = null;
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
                lessonId = binding.LessonId;
                break;
            }

            if (lessonId == null) return;

            AppendLinkToDescription(mc, contract, lessonId);
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
                "<color=#82B4E8><u>\U0001F4D6 View Instructions</u></color>" +
                "</link>";

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

    public sealed class ContractLinkClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text textComponent;
        private string lessonId;

        public void Init(TMP_Text text, string lesson)
        {
            textComponent = text;
            lessonId = lesson;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (textComponent == null || string.IsNullOrEmpty(lessonId)) return;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                textComponent, eventData.position, eventData.pressEventCamera);
            if (linkIndex < 0) return;
            var info = textComponent.textInfo.linkInfo[linkIndex];
            if (info.GetLinkID() == "kik_lesson")
                InstructionsKit.OpenLesson(lessonId);
        }
    }
}

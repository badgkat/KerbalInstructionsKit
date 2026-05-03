using System;
using KerbalInstructionsKit.Core;
using UnityEngine;

namespace KerbalInstructionsKit.Triggers
{
    public static class AttachLessonFactory
    {
        public static void TryRegister(LessonTriggerEngine engine)
        {
            if (!CcIntegration.IsAvailable) return;
            try { ScanContractLessonNodes(engine); }
            catch (Exception e) { Debug.LogError($"[KIK] failed to scan KIK_CONTRACT_LESSON nodes: {e}"); }
        }

        private static void ScanContractLessonNodes(LessonTriggerEngine engine)
        {
            AttachLessonRegistry.Clear();
            int generated = 0;
            foreach (var url in GameDatabase.Instance.GetConfigs("KIK_CONTRACT_LESSON"))
            {
                var cfg = url.config;
                var contractName = cfg.GetValue("contract");
                var lessonId = cfg.GetValue("lesson");
                if (string.IsNullOrEmpty(contractName) || string.IsNullOrEmpty(lessonId))
                {
                    Debug.LogWarning($"[KIK] KIK_CONTRACT_LESSON missing 'contract' or 'lesson' in {url.url}, skipping");
                    continue;
                }

                var unlockOnStr = (cfg.GetValue("unlockOn") ?? "OFFERED").ToUpperInvariant();
                var unlockOn = ParseState(unlockOnStr);

                bool showButton = true;
                if (cfg.HasValue("showButton") &&
                    bool.TryParse(cfg.GetValue("showButton"), out var sb))
                    showButton = sb;

                var binding = new AttachLessonBinding
                {
                    LessonId = lessonId,
                    UnlockOn = unlockOn,
                    ShowButton = showButton,
                };
                AttachLessonRegistry.Register(contractName, binding);

                engine.Register(new LessonTrigger
                {
                    Kind = TriggerKind.Contract,
                    ContractName = contractName,
                    ContractState = unlockOn,
                    LessonId = lessonId,
                });
                generated++;
            }
            Debug.Log($"[KIK] scanned {generated} KIK_CONTRACT_LESSON bindings");
        }

        private static ContractState ParseState(string s)
        {
            switch ((s ?? "").ToUpperInvariant())
            {
                case "ACCEPTED":  return ContractState.Accepted;
                case "COMPLETED": return ContractState.Completed;
                case "FAILED":    return ContractState.Failed;
                default:          return ContractState.Offered;
            }
        }
    }
}

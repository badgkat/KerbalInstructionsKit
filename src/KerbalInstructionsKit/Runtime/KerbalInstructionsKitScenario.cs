using System;
using System.Collections;
using Contracts;
using KerbalInstructionsKit.Config;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Triggers;
using KerbalInstructionsKit.Util.Expression;
using UnityEngine;

namespace KerbalInstructionsKit.Runtime
{
    [KSPScenario(
        ScenarioCreationOptions.AddToAllGames,
        GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT,
        GameScenes.TRACKSTATION)]
    public sealed class KerbalInstructionsKitScenario : ScenarioModule, IKikRuntime
    {
        public LessonRegistry Lessons { get; private set; }
        public LessonState State { get; private set; }
        public LessonTriggerEngine TriggerEngine { get; private set; }
        public IExpressionContext ExpressionContext { get; private set; }

        private GameEventBridge gameEventBridge;

        public event Action<string> LessonUnlocked;

        public Action<string> PanelOpenLesson;
        public Action PanelOpenArchive;
        public Action PanelClose;

        public override void OnAwake()
        {
            Lessons = LessonContentLoader.LoadAll();
            State = new LessonState();
            ExpressionContext = new StateExpressionContext(State);
            TriggerEngine = new LessonTriggerEngine(State);

            TriggerEngine.LessonUnlocked += id =>
            {
                Debug.Log($"[KIK] lesson unlocked: {id}");
                LessonUnlocked?.Invoke(id);
            };

            LessonContentLoader.LoadTriggers(TriggerEngine);

            CcIntegration.Detect();
            AttachLessonFactory.TryRegister(TriggerEngine);

            gameEventBridge = new GameEventBridge(TriggerEngine);

            InstructionsKit.Runtime = this;
        }

        public override void OnLoad(ConfigNode node)
        {
            State.Load(node);
            TriggerEngine.RunGameStartTriggers();
        }

        public void Start()
        {
            StartCoroutine(CheckExistingContractsWhenReady());
        }

        private IEnumerator CheckExistingContractsWhenReady()
        {
            int attempts = 0;
            while (ContractSystem.Instance == null && attempts < 30)
            {
                yield return null;
                attempts++;
            }
            CheckExistingContracts();
        }

        private void CheckExistingContracts()
        {
            var cs = ContractSystem.Instance;
            if (cs == null) return;
            foreach (var c in cs.Contracts)
            {
                var name = CcIntegration.GetContractTypeName(c);
                switch (c.ContractState)
                {
                    case Contract.State.Offered:
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Offered);
                        break;
                    case Contract.State.Active:
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Offered);
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Accepted);
                        break;
                    case Contract.State.Completed:
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Offered);
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Accepted);
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Completed);
                        break;
                    case Contract.State.Failed:
                    case Contract.State.DeadlineExpired:
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Offered);
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Accepted);
                        TriggerEngine.OnContractEvent(name, Triggers.ContractState.Failed);
                        break;
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            State.Save(node);
        }

        public void OpenLesson(string id)
        {
            State.LastViewedLesson = id;
            State.LastViewedPage = 0;
            PanelOpenLesson?.Invoke(id);
        }

        public void OpenArchive() => PanelOpenArchive?.Invoke();
        public void Close() => PanelClose?.Invoke();

        public void SetFlag(string name, bool value)
        {
            State.SetFlag(name, value);
            TriggerEngine.OnFlagSet(name, value);
        }

        public bool GetFlag(string name) => State.GetFlag(name);

        public void OnDestroy()
        {
            gameEventBridge?.Dispose();
            gameEventBridge = null;
            if (InstructionsKit.Runtime == (IKikRuntime)this)
                InstructionsKit.Runtime = null;
        }
    }
}

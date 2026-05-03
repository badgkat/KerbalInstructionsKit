using System;
using System.Collections.Generic;
using System.Reflection;
using Contracts;
using KerbalInstructionsKit.Triggers;
using UnityEngine;

namespace KerbalInstructionsKit.Runtime
{
    public sealed class GameEventBridge : IDisposable
    {
        private readonly LessonTriggerEngine engine;
        private readonly List<Action> unsubscribeActions = new List<Action>();

        public GameEventBridge(LessonTriggerEngine engine)
        {
            this.engine = engine;
            SubscribeBuiltIn();
            SubscribeDynamicEvents();
        }

        private void SubscribeBuiltIn()
        {
            EventVoid.OnEvent pauseHandler = () => engine.OnGameEvent("onGamePause");
            GameEvents.onGamePause.Add(pauseHandler);
            unsubscribeActions.Add(() => GameEvents.onGamePause.Remove(pauseHandler));

            EventVoid.OnEvent unpauseHandler = () => engine.OnGameEvent("onGameUnpause");
            GameEvents.onGameUnpause.Add(unpauseHandler);
            unsubscribeActions.Add(() => GameEvents.onGameUnpause.Remove(unpauseHandler));

            EventData<Contract>.OnEvent offeredHandler = c =>
                engine.OnContractEvent(CcIntegration.GetContractTypeName(c), Triggers.ContractState.Offered);
            GameEvents.Contract.onOffered.Add(offeredHandler);
            unsubscribeActions.Add(() => GameEvents.Contract.onOffered.Remove(offeredHandler));

            EventData<Contract>.OnEvent acceptedHandler = c =>
                engine.OnContractEvent(CcIntegration.GetContractTypeName(c), Triggers.ContractState.Accepted);
            GameEvents.Contract.onAccepted.Add(acceptedHandler);
            unsubscribeActions.Add(() => GameEvents.Contract.onAccepted.Remove(acceptedHandler));

            EventData<Contract>.OnEvent completedHandler = c =>
                engine.OnContractEvent(CcIntegration.GetContractTypeName(c), Triggers.ContractState.Completed);
            GameEvents.Contract.onCompleted.Add(completedHandler);
            unsubscribeActions.Add(() => GameEvents.Contract.onCompleted.Remove(completedHandler));

            EventData<Contract>.OnEvent failedHandler = c =>
                engine.OnContractEvent(CcIntegration.GetContractTypeName(c), Triggers.ContractState.Failed);
            GameEvents.Contract.onFailed.Add(failedHandler);
            unsubscribeActions.Add(() => GameEvents.Contract.onFailed.Remove(failedHandler));

            EventData<Contract>.OnEvent declinedHandler = c =>
                engine.OnContractEvent(CcIntegration.GetContractTypeName(c), Triggers.ContractState.Failed);
            GameEvents.Contract.onDeclined.Add(declinedHandler);
            unsubscribeActions.Add(() => GameEvents.Contract.onDeclined.Remove(declinedHandler));
        }

        private void SubscribeDynamicEvents()
        {
            var registeredNames = new HashSet<string>();
            foreach (var t in engine.All)
            {
                if (t.Kind != TriggerKind.GameEvent) continue;
                if (string.IsNullOrEmpty(t.EventName)) continue;
                if (t.EventName == "onGamePause" || t.EventName == "onGameUnpause") continue;
                if (!registeredNames.Add(t.EventName)) continue;

                var field = typeof(GameEvents).GetField(t.EventName,
                    BindingFlags.Public | BindingFlags.Static);
                if (field == null)
                {
                    Debug.LogWarning($"[KIK] GameEvents.{t.EventName} not found, trigger will not fire");
                    continue;
                }

                var val = field.GetValue(null);
                if (val is EventVoid ev)
                {
                    var eventName = t.EventName;
                    EventVoid.OnEvent handler = () => engine.OnGameEvent(eventName);
                    ev.Add(handler);
                    unsubscribeActions.Add(() => ev.Remove(handler));
                }
                else
                {
                    Debug.LogWarning($"[KIK] GameEvents.{t.EventName} is not EventVoid " +
                                     $"(type: {val?.GetType().Name ?? "null"}); only EventVoid triggers are supported");
                }
            }
        }

        public void Dispose()
        {
            foreach (var unsub in unsubscribeActions)
            {
                try { unsub(); } catch { }
            }
            unsubscribeActions.Clear();
        }
    }
}

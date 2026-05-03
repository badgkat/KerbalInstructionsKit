using System;
using System.Reflection;
using KerbalInstructionsKit.Core;
using UnityEngine;

namespace KerbalInstructionsKit.Triggers
{
    public sealed class AttachLessonBinding
    {
        public string LessonId;
        public ContractState UnlockOn;
        public bool ShowButton;
    }

    public static class AttachLessonRegistry
    {
        private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<AttachLessonBinding>> byContract =
            new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<AttachLessonBinding>>();

        public static void Register(string contractTypeName, AttachLessonBinding b)
        {
            if (!byContract.TryGetValue(contractTypeName, out var list))
                byContract[contractTypeName] = list = new System.Collections.Generic.List<AttachLessonBinding>();
            list.Add(b);
        }

        public static System.Collections.Generic.IList<AttachLessonBinding> Get(string contractTypeName) =>
            byContract.TryGetValue(contractTypeName, out var list)
                ? (System.Collections.Generic.IList<AttachLessonBinding>)list
                : Array.Empty<AttachLessonBinding>();

        public static void Clear() => byContract.Clear();
    }
}

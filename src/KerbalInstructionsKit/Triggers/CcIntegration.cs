using System;
using System.Linq;
using System.Reflection;
using Contracts;
using UnityEngine;

namespace KerbalInstructionsKit.Triggers
{
    public static class CcIntegration
    {
        public static bool IsAvailable { get; private set; }
        public static Assembly CcAssembly { get; private set; }

        private static Type configuredContractType;
        private static FieldInfo contractTypeField;
        private static PropertyInfo contractTypeProp;
        private static FieldInfo contractTypeNameField;
        private static PropertyInfo contractTypeNameProp;

        public static void Detect()
        {
            CcAssembly = AssemblyLoader.loadedAssemblies
                .FirstOrDefault(a => a.name == "ContractConfigurator")
                ?.assembly;
            IsAvailable = CcAssembly != null;
            if (IsAvailable)
            {
                CacheReflectionHandles();
                Debug.Log("[KIK] ContractConfigurator detected; KIK_CONTRACT_LESSON scanning enabled");
            }
            else
            {
                Debug.Log("[KIK] ContractConfigurator not present. Use LESSON_TRIGGER for unlock control.");
            }
        }

        private static void CacheReflectionHandles()
        {
            configuredContractType = CcAssembly.GetType("ContractConfigurator.ConfiguredContract");
            if (configuredContractType == null) return;

            contractTypeField = configuredContractType.GetField("contractType",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            contractTypeProp = configuredContractType.GetProperty("contractType",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var memberType = contractTypeField?.FieldType ?? contractTypeProp?.PropertyType;
            if (memberType == null) return;

            contractTypeNameField = memberType.GetField("name",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            contractTypeNameProp = memberType.GetProperty("name",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Log($"[KIK] CC reflection: type={configuredContractType != null} " +
                      $"ctField={contractTypeField != null} ctProp={contractTypeProp != null} " +
                      $"nameField={contractTypeNameField != null} nameProp={contractTypeNameProp != null}");
        }

        public static string GetContractTypeName(Contract c)
        {
            if (configuredContractType != null && configuredContractType.IsInstanceOfType(c))
            {
                try
                {
                    var ct = contractTypeField != null
                        ? contractTypeField.GetValue(c)
                        : contractTypeProp?.GetValue(c);
                    if (ct != null)
                    {
                        var name = (contractTypeNameField != null
                            ? contractTypeNameField.GetValue(ct)
                            : contractTypeNameProp?.GetValue(ct)) as string;
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                }
                catch { }
            }
            return c.GetType().Name;
        }

        public static Type GetCcType(string fullName) =>
            CcAssembly?.GetType(fullName);
    }
}

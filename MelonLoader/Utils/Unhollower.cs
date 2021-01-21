﻿using System;
using System.Reflection;

namespace MelonLoader
{
    public static class UnhollowerSupport
    {
        internal static Type Il2CppObjectBaseType = null;
        internal static Type Il2CppMethodInfoType = null;
        internal static MethodInfo Il2CppObjectBaseToPtrMethod = null;
        internal static MethodInfo Il2CppStringToManagedMethod = null;
        internal static MethodInfo ManagedStringToIl2CppMethod = null;
        internal static MethodInfo GetIl2CppMethodInfoPointerFieldForGeneratedMethod = null;
        private static Type Il2CppCallerCountAttributeType = null;
        private static FieldInfo Il2CppCallerCountField = null;

        static UnhollowerSupport()
        {
            if (!MelonUtils.IsGameIl2Cpp())
                return;
            Assembly UnhollowerBaseLib = Assembly.Load("UnhollowerBaseLib");
            if (UnhollowerBaseLib == null)
            {
                MelonLogger.ThrowInternalFailure("Failed to Load Assembly for UnhollowerBaseLib!");
                return;
            }
            Il2CppObjectBaseType = UnhollowerBaseLib.GetType("UnhollowerBaseLib.Il2CppObjectBase");
            Il2CppMethodInfoType = UnhollowerBaseLib.GetType("UnhollowerBaseLib.Runtime.Il2CppMethodInfo");
            Il2CppObjectBaseToPtrMethod = UnhollowerBaseLib.GetType("UnhollowerBaseLib.IL2CPP").GetMethod("Il2CppObjectBaseToPtr");
            Il2CppStringToManagedMethod = UnhollowerBaseLib.GetType("UnhollowerBaseLib.IL2CPP").GetMethod("Il2CppStringToManaged");
            ManagedStringToIl2CppMethod = UnhollowerBaseLib.GetType("UnhollowerBaseLib.IL2CPP").GetMethod("ManagedStringToIl2Cpp");
            GetIl2CppMethodInfoPointerFieldForGeneratedMethod = UnhollowerBaseLib.GetType("UnhollowerBaseLib.UnhollowerUtils").GetMethod("GetIl2CppMethodInfoPointerFieldForGeneratedMethod");
            Il2CppCallerCountAttributeType = UnhollowerBaseLib.GetType("UnhollowerBaseLib.Attributes.CallerCountAttribute");
            Il2CppCallerCountField = Il2CppCallerCountAttributeType.GetField("Count", BindingFlags.Public | BindingFlags.Instance);
        }

        public static bool IsGeneratedAssemblyType(Type type) => ((Il2CppObjectBaseType != null) && (type != null) && type.IsSubclassOf(Il2CppObjectBaseType));

        public static IntPtr MethodBaseToIl2CppMethodInfoPointer(MethodBase method)
        {
            if (method == null)
                throw new NullReferenceException("The method cannot be null.");
            FieldInfo methodPtr = (FieldInfo)GetIl2CppMethodInfoPointerFieldForGeneratedMethod.Invoke(null, new object[] { method });
            if (methodPtr == null)
                throw new NotSupportedException($"Cannot get IntPtr for {method.Name} as there is no corresponding IL2CPP method");
            return (IntPtr)methodPtr.GetValue(null);
        }

        public static T Il2CppObjectPtrToIl2CppObject<T>(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new NullReferenceException("The ptr cannot be IntPtr.Zero.");
            if (!IsGeneratedAssemblyType(typeof(T)))
                throw new NullReferenceException("The type must be a Generated Assembly Type.");
            return (T)typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(IntPtr) }, new ParameterModifier[0]).Invoke(new object[] { ptr });
        }

        public static int? GetIl2CppMethodCallerCount(MethodBase original) {
            object[] callerCountAttributes = original.GetCustomAttributes(Il2CppCallerCountAttributeType, false);
            if (callerCountAttributes.Length != 1)
                return null;
            return (int) Il2CppCallerCountField.GetValue(callerCountAttributes[0]);
        }
    }
}
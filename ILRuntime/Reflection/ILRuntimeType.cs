﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Reflection
{
    class ILRuntimeType : Type
    {
        ILType type;
        Runtime.Enviorment.AppDomain appdomain;
        object[] customAttributes;
        
        public ILRuntimeType(ILType t)
        {
            type = t;
            appdomain = t.AppDomain;
        }

        void InitializeCustomAttribute()
        {
            customAttributes = new object[type.TypeDefinition.CustomAttributes.Count];
            List<IType> param = null;
            for (int i = 0; i < type.TypeDefinition.CustomAttributes.Count; i++)
            {
                var attribute = type.TypeDefinition.CustomAttributes[i];
                var at = appdomain.GetType(attribute.AttributeType, type);
                object ins = null;

                if (at is ILType)
                {
                    var it = (ILType)at;
                    if (!attribute.HasConstructorArguments)
                        ins = it.Instantiate(true);
                    else
                    {
                        ins = it.Instantiate(false);
                        if (param == null)
                            param = new List<IType>();
                        param.Clear();
                        object[] p = new object[attribute.ConstructorArguments.Count + 1];
                        p[0] = ins;
                        for (int j = 0; j < attribute.ConstructorArguments.Count; j++)
                        {
                            var ca = attribute.ConstructorArguments[j];
                            param.Add(appdomain.GetType(ca.Type, type));
                            p[j + 1] = ca.Value;
                        }
                        var ctor = it.GetConstructor(param);
                        appdomain.Invoke(ctor, p);
                    }

                    if (attribute.HasProperties)
                    {
                        object[] p = new object[2];
                        p[0] = ins;
                        foreach (var j in attribute.Properties)
                        {
                            p[1] = j.Argument.Value;
                            var setter = it.GetMethod("set_" + j.Name, 1);
                            appdomain.Invoke(setter, p);
                        }
                    }
                }
                else
                {
                    param = new List<IType>();
                    object[] p = null;
                    if (attribute.HasConstructorArguments)
                    {
                        p = new object[attribute.ConstructorArguments.Count];
                        for (int j = 0; j < attribute.ConstructorArguments.Count; j++)
                        {
                            var ca = attribute.ConstructorArguments[j];
                            param.Add(appdomain.GetType(ca.Type, type));
                            p[j] = ca.Value;
                        }
                    }
                    ins = ((CLRMethod)at.GetConstructor(param)).ConstructorInfo.Invoke(p);
                    if (attribute.HasProperties)
                    {                        
                        foreach (var j in attribute.Properties)
                        {
                            var prop = at.TypeForCLR.GetProperty(j.Name);
                            prop.SetValue(ins, j.Argument.Value, null);
                        }
                    }
                }

                customAttributes[i] = ins;
            }

        }

        public override Assembly Assembly
        {
            get
            {
                return typeof(ILRuntimeType).Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return type.FullName;
            }
        }

        public override Type BaseType
        {
            get
            {
                return type.BaseType != null ? type.BaseType.ReflectionType : null;
            }
        }

        public override string FullName
        {
            get
            {
                return type.FullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Module Module
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                return type.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return typeof(ILTypeInstance);
            }
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();

            return customAttributes;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            IMethod res;
            if (types == null)
                res = type.GetMethod(name);
            else
            {
                List<IType> param = new List<IType>();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] is ILRuntimeType)
                        param.Add(((ILRuntimeType)types[i]).type);
                    else
                    {
                        var t = appdomain.GetType(types[i]);
                        if (t == null)
                            t = appdomain.GetType(types[i].AssemblyQualifiedName);
                        if (t == null)
                            throw new TypeLoadException();
                        param.Add(t);
                    }
                }

                res = type.GetMethod(name, param, null);
            }
            if (res != null)
                return ((ILMethod)res).ReflectionMethodInfo;
            else
                return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }
    }
}

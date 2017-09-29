using System;
using System.Linq;
using System.Reflection;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Descriptors.Specialized;

namespace Jint.Runtime.Interop
{
	/// <summary>
	/// Wraps a CLR instance
	/// </summary>
	public sealed class ObjectWrapper : ObjectInstance, IObjectWrapper
    {
        public Object Target { get; set; }

        public ObjectWrapper(Engine engine, Object obj)
            : base(engine)
        {
            Target = obj;
            _initProperties();
        }

        public override void Put(string propertyName, JsValue value, bool throwOnError)
        {
            if (!CanPut(propertyName))
            {
                if (throwOnError)
                {
                    throw new JavaScriptException(Engine.TypeError);
                }

                return;
            }

            var ownDesc = GetOwnProperty(propertyName);

            if (ownDesc == null)
            {
                if (throwOnError)
                {
                    throw new JavaScriptException(Engine.TypeError, "Unknown member: " + propertyName);
                }
                else
                {
                    return;
                }
            }

            ownDesc.Value = value;
        }

        private void _initProperties()
        {
            var type = Target.GetType();

            // look for a property
            type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
                .ToList()
                .ForEach(property => {
                    var descriptor = new PropertyInfoDescriptor(Engine, property, Target);
                    Properties.Add(property.Name, descriptor);
                });

            // look for a field
            type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
                .Where(field => !Properties.ContainsKey(field.Name))
                .ToList()
                .ForEach(field => {
                    var descriptor = new FieldInfoDescriptor(Engine, field, Target);
                    Properties.Add(field.Name, descriptor);
                });

            // if no properties were found then look for a method 
            type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
                .ToList()
                .ForEach(method => {
                    if (!Properties.ContainsKey(method.Name))
                    {
                        var descriptor = new PropertyDescriptor(new MethodInfoFunctionInstance(Engine, new[] { method }), false, true, false);
                        Properties.Add(method.Name, descriptor);
                    }
                });
        }

        private bool EqualsIgnoreCasing(string s1, string s2)
        {
            bool equals = false;
            if (s1.Length == s2.Length)
            {
                if (s1.Length > 0 && s2.Length > 0) 
                {
                    equals = (s1.ToLower()[0] == s2.ToLower()[0]);
                }
                if (s1.Length > 1 && s2.Length > 1) 
                {
                    equals = equals && (s1.Substring(1) == s2.Substring(1));
                }
            }
            return equals;
        }
    }
}

using System;
using System.Reflection;

namespace FlexalonCopilot
{
    internal class PropertyOrField
    {
        private PropertyInfo _propertyInfo;
        private FieldInfo _fieldInfo;
        private PropertyOrField _subProperty = null;


        public Type DeclaringType => _subProperty?.DeclaringType ?? _propertyInfo?.DeclaringType ?? _fieldInfo.DeclaringType;
        public Type Type => _subProperty?.Type ?? _propertyInfo?.PropertyType ?? _fieldInfo.FieldType;
        public bool Valid => _propertyInfo != null || _fieldInfo != null;

        public PropertyOrField(Type type, string name)
        {
            var parts = name.Split('.');
            var property = parts[0];

            var prop = type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                _propertyInfo = prop;
            }
            else
            {
                var field = type.GetField(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    _fieldInfo = field;
                }
                else
                {
                    throw new Exception($"Property {property} not found on {type}");
                }
            }

            if (parts.Length > 1)
            {
                _subProperty = new PropertyOrField(Type, string.Join(".", parts, 1, parts.Length - 1));
            }
        }

        public static bool TrySet(object obj, string name, object value)
        {
            var property = new PropertyOrField(obj.GetType(), name);
            if (property.Valid)
            {
                property.SetValue(obj, value);
            }

            return true;
        }

        public static bool TryGet<T>(object obj, string name, out T value)
        {
            var property = new PropertyOrField(obj.GetType(), name);
            if (property.Valid && typeof(T).IsAssignableFrom(property.Type))
            {
                value = (T)property.GetValue(obj);
                return true;
            }

            value = default(T);
            return false;
        }

        public static Type GetType(Type type, string name)
        {
            var property = new PropertyOrField(type, name);
            return property.Type;
        }

        public void SetValue(object obj, object value)
        {
            if (_subProperty != null)
            {
                object subObj = _propertyInfo?.GetValue(obj) ?? _fieldInfo.GetValue(obj);
                if (subObj == null)
                {
                    return;
                }

                _subProperty.SetValue(subObj, value);
                value = subObj;
            }

            _propertyInfo?.SetValue(obj, value);
            _fieldInfo?.SetValue(obj, value);

            // UnityEngine.Debug.Log($"Set {obj} {_propertyInfo?.Name ?? _fieldInfo.Name} = {value}");

            #if UNITY_TMPRO
                if (obj is TMPro.TMP_Text tmp)
                {
                    tmp.SetAllDirty();
                }
            #endif
        }

        public object GetValue(object obj)
        {
            object value = null;
            if (_propertyInfo != null)
            {
                value = _propertyInfo.GetValue(obj);
            }
            else
            {
                value = _fieldInfo.GetValue(obj);
            }

            if (value != null && _subProperty != null)
            {
                value = _subProperty.GetValue(value);
            }

            return value;
        }
    }
}
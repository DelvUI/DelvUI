using Dalamud.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DelvUI.Config
{
    public abstract class PluginConfigObjectConverter : JsonConverter
    {
        protected Dictionary<string, PluginConfigObjectFieldConverter> FieldConvertersMap = new Dictionary<string, PluginConfigObjectFieldConverter>();

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var genericMethod = GetType().GetMethod("ConvertJson");
            var method = genericMethod?.MakeGenericMethod(objectType);
            return method?.Invoke(this, new object[] { reader, serializer });
        }

        public T? ConvertJson<T>(JsonReader reader, JsonSerializer serializer) where T : PluginConfigObject
        {
            Type type = typeof(T);
            T? config = null;

            try
            {
                config = (T?)Activator.CreateInstance(type);
                if (config == null) { return null; }

                JObject? jsonObject = (JObject?)serializer.Deserialize(reader);
                if (jsonObject == null) { return null; }

                Dictionary<string, object> ValuesMap = new Dictionary<string, object>();

                // get values from json
                foreach (JProperty property in jsonObject.Properties())
                {
                    string propertyName = property.Name;
                    object? value = null;

                    // convert values if needed
                    if (FieldConvertersMap.TryGetValue(propertyName, out PluginConfigObjectFieldConverter? fieldConverter) && fieldConverter != null)
                    {
                        (propertyName, value) = fieldConverter.Convert(property.Value);
                    }
                    // read value from json
                    else
                    {
                        FieldInfo? field = type.GetField(propertyName);
                        if (field != null)
                        {
                            value = property.Value.ToObject(field.FieldType);
                        }
                    }

                    if (value != null)
                    {
                        ValuesMap.Add(propertyName, value);
                    }
                }

                // apply values
                foreach (string key in ValuesMap.Keys)
                {
                    string[] fields = key.Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    object? currentObject = config;
                    object value = ValuesMap[key];

                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo? field = currentObject?.GetType().GetField(fields[i]);
                        if (field == null) { break; }

                        if (i == fields.Length - 1 && value.GetType() == field.FieldType)
                        {
                            field.SetValue(currentObject, value);
                        }
                        else
                        {
                            currentObject = field.GetValue(currentObject);
                        }
                    }
                }
            }
            catch
            {
                PluginLog.Error($"Error deserializing {type.Name}!");
            }

            return config;
        }


        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) { return; }

            JObject jsonObject = new JObject();
            Type type = value.GetType();
            jsonObject.Add("$type", type.FullName + ", DelvUI");

            FieldInfo[] fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) { continue; }

                object? fieldValue = field.GetValue(value);
                if (fieldValue != null)
                {
                    jsonObject.Add(field.Name, JToken.FromObject(fieldValue, serializer));
                }
            }

            jsonObject.WriteTo(writer);
        }
    }

    public abstract class PluginConfigObjectFieldConverter
    {
        public readonly string NewFieldPath;
        public PluginConfigObjectFieldConverter(string newFieldPath)
        {
            NewFieldPath = newFieldPath;
        }

        public abstract (string, object) Convert(JToken token);
    }

    public class NewTypeFieldConverter<TOld, TNew> : PluginConfigObjectFieldConverter
        where TOld : struct
        where TNew : struct
    {
        private TNew DefaultValue;
        private Func<TOld, TNew> Func;

        public NewTypeFieldConverter(string newFieldPath, TNew defaultValue, Func<TOld, TNew> func) : base(newFieldPath)
        {
            DefaultValue = defaultValue;
            Func = func;
        }

        public override (string, object) Convert(JToken token)
        {
            TNew result = DefaultValue;

            TOld? oldValue = token.ToObject<TOld>();
            if (oldValue.HasValue)
            {
                result = Func(oldValue.Value);
            }

            return (NewFieldPath, result);
        }
    }

    public class SameTypeFieldConverter<T> : NewTypeFieldConverter<T, T> where T : struct
    {
        public SameTypeFieldConverter(string newFieldPath, T defaultValue)
            : base(newFieldPath, defaultValue, (oldValue) => { return oldValue; })
        {
        }
    }
}

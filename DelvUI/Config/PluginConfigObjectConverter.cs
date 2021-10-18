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
        protected Dictionary<string, Type> FieldConvertersMap = new Dictionary<string, Type>();

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var genericMethod = GetType().GetMethod("ConvertJson");
            var method = genericMethod?.MakeGenericMethod(objectType);
            return method?.Invoke(this, new object[] { reader, serializer });
        }

        public T? ConvertJson<T>(JsonReader reader, JsonSerializer serializer) where T : PluginConfigObject
        {
            Type type = typeof(T);
            T? config = (T?)Activator.CreateInstance(typeof(T));

            try
            {
                JObject? jsonObject = (JObject?)serializer.Deserialize(reader);
                if (jsonObject == null) { return null; }

                Dictionary<string, object> ValuesMap = new Dictionary<string, object>();

                // get values from json
                foreach (JProperty property in jsonObject.Properties())
                {
                    string propertyName = property.Name;
                    object? value = null;

                    // convert values if needed
                    if (FieldConvertersMap.TryGetValue(propertyName, out Type? converterType))
                    {
                        object? converterObj = Activator.CreateInstance(converterType);
                        if (converterObj is PluginConfigObjectFieldConverter converter)
                        {
                            (propertyName, value) = converter.Convert(property.Value);
                        }
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
                FieldInfo[] fields = typeof(T).GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (ValuesMap.TryGetValue(field.Name, out object? value) && value != null && value.GetType() == field.FieldType)
                    {
                        field.SetValue(config, value);
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
        public abstract (string, object) Convert(JToken token);
    }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEditorInternal;
using UnityEngine;

namespace GameTool
{
    public static class JsonHelper
    {
        private static List<Type> CustomJsonTypes = new List<Type>
        {
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
        };

        public static void ApplyCustomSetting()
        {
            var converters = new Dictionary<Type, JsonConverter>();
            foreach (var item in CustomJsonTypes)
            {
                converters.Add(item, new CustomConverter());
            }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CustomResolver(converters),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            JsonConvert.DefaultSettings = () => settings;
        }

        public class CustomResolver : DefaultContractResolver
        {
            private Dictionary<Type, JsonConverter> Converters { get; set; }

            public CustomResolver(Dictionary<Type, JsonConverter> converters)
            {
                Converters = converters;
            }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);
                if (Converters.TryGetValue(objectType, out JsonConverter converter))
                {
                    contract.Converter = converter;
                }
                return contract;
            }
        }

        public class CustomConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return CustomJsonTypes.Contains(objectType);
            }

            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Type objectType = value.GetType();

                if (value == null) {
                    writer.WriteNull();
                    return;
                }

                var typeSwitch = new Dictionary<Type, Action> {

                    { typeof(Vector2), () => {
                        writer.WritePropertyName("x");
                        writer.WriteValue(((Vector2)value).x.ToString());
                        writer.WritePropertyName("y");
                        writer.WriteValue(((Vector2)value).y.ToString());
                    }},

                    { typeof(Vector2Int), () => {
                        writer.WritePropertyName("x");
                        writer.WriteValue(((Vector2Int)value).x.ToString());
                        writer.WritePropertyName("y");
                        writer.WriteValue(((Vector2Int)value).y.ToString());
                    }},

                    { typeof(Vector3), () => {
                        writer.WritePropertyName("x");
                        writer.WriteValue(((Vector3)value).x.ToString());
                        writer.WritePropertyName("y");
                        writer.WriteValue(((Vector3)value).y.ToString());
                        writer.WritePropertyName("z");
                        writer.WriteValue(((Vector3)value).z.ToString());
                    }},

                    { typeof(Vector3Int), () => {
                        writer.WritePropertyName("x");
                        writer.WriteValue(((Vector3Int)value).x.ToString());
                        writer.WritePropertyName("y");
                        writer.WriteValue(((Vector3Int)value).y.ToString());
                        writer.WritePropertyName("z");
                        writer.WriteValue(((Vector3Int)value).z.ToString());
                    }},
                };

                writer.WriteStartObject();
                typeSwitch[objectType]();
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }
        }
    }
}
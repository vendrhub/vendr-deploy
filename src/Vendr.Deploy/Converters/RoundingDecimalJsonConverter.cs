using Newtonsoft.Json;
using System;

namespace Vendr.Deploy.Converters
{
    public class RoundingDecimalJsonConverter : JsonConverter
    {
        private int _precision;
        private MidpointRounding _rounding;

        public RoundingDecimalJsonConverter()
            : this(2)
        { }

        public RoundingDecimalJsonConverter(int precision)
            : this(precision, MidpointRounding.AwayFromZero)
        { }

        public RoundingDecimalJsonConverter(int precision, MidpointRounding rounding)
        {
            _precision = precision;
            _rounding = rounding;
        }

        public override bool CanRead 
            => false;

        public override bool CanConvert(Type objectType) 
            => objectType == typeof(double);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) 
            => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(Math.Round((decimal)value, _precision, _rounding));
    }
}

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.API.JsonConverters
{
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        private static readonly string[] TimeFormats = new[]
        {
            @"hh\:mm\:ss",
            @"h\:mm\:ss",
            @"hh\:mm",
            @"h\:mm",
            @"h\:m"
        };

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return TimeSpan.Zero;

            // Try parsing with each supported format
            foreach (var format in TimeFormats)
            {
                if (TimeSpan.TryParseExact(value, format, null, out TimeSpan result))
                    return result;
            }

            // Try general TimeSpan parsing as fallback
            if (TimeSpan.TryParse(value, out TimeSpan generalResult))
                return generalResult;

            throw new JsonException($"Invalid time format. Please use one of the following formats: HH:mm:ss, H:mm:ss, HH:mm, H:mm. Received: {value}");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
        }
    }
}
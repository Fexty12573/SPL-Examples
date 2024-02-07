using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SharpPluginLoader.Core.MtTypes;

namespace ColEditor;

internal class MtColorJsonConverter : JsonConverter<MtColor>
{
    public override MtColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Convert.ToUInt32(value, 16);
    }

    public override void Write(Utf8JsonWriter writer, MtColor value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"0x{value.Rgba:X}");
    }
}

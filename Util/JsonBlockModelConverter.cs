﻿using Blockgame_OpenTK.BlockUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.Util
{
    internal class JsonBlockModelConverter : JsonConverter<BlockModel>
    {
        public override BlockModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            // Console.Log($"Loading {reader.GetString()}");
            if (reader.GetString() == null) return null;
            BlockModel model = BlockModel.LoadFromJson(reader.GetString());
            return model;

        }

        public override void Write(Utf8JsonWriter writer, BlockModel value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

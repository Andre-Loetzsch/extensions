﻿using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Oleander.Extensions.Configuration.Json
{
    public class WritableJsonStreamConfigurationSource : JsonStreamConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (this.Stream != null) this.Stream.Position = 0;
            var buffer = new byte[this.Stream?.Length ?? 0];
            _ = this.Stream?.Read(buffer, 0, buffer.Length);
            if (this.Stream != null) this.Stream.Position = 0;

            var provider = new WritableJsonStreamConfigurationProvider(this, this.Stream ?? new MemoryStream()) ;
            this.Stream = new MemoryStream(buffer) { Position = 0 };
            return provider;
        }
    }
}
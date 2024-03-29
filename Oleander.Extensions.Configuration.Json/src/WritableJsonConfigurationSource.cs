﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Oleander.Extensions.Configuration.Json
{
    public class WritableJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            this.EnsureDefaults(builder);
            return new WritableJsonConfigurationProvider(this);
        }
    }
}
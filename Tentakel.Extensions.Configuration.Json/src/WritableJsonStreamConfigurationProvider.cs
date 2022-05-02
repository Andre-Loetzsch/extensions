using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration.Json;

namespace Tentakel.Extensions.Configuration.Json
{
    public class WritableJsonStreamConfigurationProvider : JsonStreamConfigurationProvider
    {
        private readonly MemoryStream _stream;
        private readonly Stream _sourceStream;
        private readonly WritableJsonConfigurationProviderHelper _providerHelper = new();

        public WritableJsonStreamConfigurationProvider(JsonStreamConfigurationSource source, Stream stream) : base(source)
        {
            var buffer = new byte[source.Stream.Length];
            source.Stream.Read(buffer, 0, buffer.Length);
            source.Stream.Position = 0;

            this._stream = new MemoryStream();
            this._stream.Write(buffer, 0, buffer.Length);
            this._stream.Position = 0;
            this._sourceStream = stream;
        }

        internal IDictionary<string, string> InnerData => base.Data;

        public override void Set(string key, string value)
        {
            var buffer = new byte[this._stream.Length];
            this._stream.Position = 0;
            this._stream.Read(buffer, 0, buffer.Length);
            
            buffer = Encoding.UTF8.GetBytes(
                this._providerHelper.Set(Encoding.UTF8.GetString(buffer), key, value, (k, v) => {base.Set(k, v);} ));

            this._sourceStream.SetLength(0);
            this._sourceStream.Position = 0;
            this._sourceStream.Write(buffer, 0, buffer.Length);
            this._sourceStream.Position = 0;

            this._stream.SetLength(0);
            this._stream.Position = 0;
            this._stream.Write(buffer, 0, buffer.Length);
            this._stream.Position = 0;
        }
    }
}
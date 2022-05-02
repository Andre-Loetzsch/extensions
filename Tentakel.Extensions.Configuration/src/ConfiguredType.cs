namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredType
    {
        public string? Type { get; set; }
        public object? Instance { get; internal set; }
    }
}
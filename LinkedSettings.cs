using Auxiliary.Configuration;
using System.Text.Json.Serialization;

namespace Linked
{
    public class LinkedSettings : ISettings
    {
        [JsonPropertyName("IsDataCentral")]
        public bool IsDataCentral { get; set; }
    }
}

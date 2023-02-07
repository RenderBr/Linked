using Auxiliary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Linked
{
    public class LinkedSettings : ISettings
    {
        [JsonPropertyName("IsDataCentral")]
        public bool IsDataCentral { get; set; }

        [JsonPropertyName("CentralServer")]
        public string CentralServer { get; set; }
    }
}

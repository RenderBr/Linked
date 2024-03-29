﻿using Auxiliary.Configuration;
using System.Text.Json.Serialization;

namespace Linked
{
    public class LinkedSettings : ISettings
    {
        [JsonPropertyName("IsDataCentral")]
        public bool IsDataCentral { get; set; }

        [JsonPropertyName("DisableRegistrations")]
        public bool DisableRegistrations { get; set; }

        [JsonPropertyName("ForceAccountMadeAlready")]
        public bool ForceAccountMadeAlready { get; set; }

        [JsonPropertyName("AutoLogin")]
        public bool AutoLogin { get; set; } = true;

        [JsonPropertyName("DoGreetPlayer")]
        public bool DoGreetPlayer { get; set; } = false;

        [JsonPropertyName("IgnoredPorts")]
        public List<int> IgnoredPorts { get; set; } = new List<int>();

        [JsonPropertyName("LocalOverridesStorage")] // json / mongo
        public string LocalOverridesStorageType { get; set; } = "json";

    }
}

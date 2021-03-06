﻿using Newtonsoft.Json;

namespace Microsoft.Education.Data
{
    public class Teacher : SectionUser
    {
         [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_TeacherId")]
        public string TeacherId { get; set; }

        public override string UserId => TeacherId;
    }
}
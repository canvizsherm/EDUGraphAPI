﻿using Newtonsoft.Json;

namespace Microsoft.Education.Data
{
    public class Student : SectionUser
    {
        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_StudentId")]
        public string StudentId { get; set; }

        public override string UserId => StudentId;
    }
}
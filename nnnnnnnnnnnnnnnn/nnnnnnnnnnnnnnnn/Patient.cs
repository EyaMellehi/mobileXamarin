using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace nnnnnnnnnnnnnnnn
{
    public class Patient
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("nom")]
        public string Nom { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("pwd")]
        public string pwd { get; set; }
    }
}

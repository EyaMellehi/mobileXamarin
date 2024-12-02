using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace nnnnnnnnnnnnnnnn
{
    class Doctor{
        [JsonProperty("codem")]
        public int Codem { get; set; }

        [JsonProperty("nom")]
        public string Nom { get; set; }
       
        [JsonProperty("specialite")]
        public string Specialite { get; set; }

        [JsonProperty("cin")]
        public string Cin { get; set; }       

        [JsonProperty("telephone")]
        public string Telephone { get; set; } 

        [JsonProperty("email")]
        public string Email { get; set; } 
    
    }
}

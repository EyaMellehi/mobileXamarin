using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Text;

namespace nnnnnnnnnnnnnnnn
{
    class DoctorAvail
    {

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }
        [JsonProperty("date_a")]
        public DateTime Date { get; set; }
        [JsonProperty("hour_a")]
        public TimeSpan Hour { get; set; }
    }
}

using System;
using Newtonsoft.Json;

using System.Collections.Generic;
using System.Text;

namespace nnnnnnnnnnnnnnnn
{
    public class Appointment
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("codem")]
        public int Codem { get; set; }
        [JsonProperty("patient")]
        public int Patient { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }  // Date only (no time, database `DATE` type)

        [JsonProperty("etat")]
        public string Etat { get; set; }  // Appointment status (e.g., "Pending")

        [JsonProperty("hour")]
        public TimeSpan Hour { get; set; }  // Only the time part (stored separately)

        [JsonProperty("appointment_date")]
        public string AppointmentDate { get; set; }

        [JsonProperty("doctor_name")]
        public string DoctorName { get; set; }

        [JsonProperty("doctor_specialty")]
        public string DoctorSpecialty { get; set; }
    }
}

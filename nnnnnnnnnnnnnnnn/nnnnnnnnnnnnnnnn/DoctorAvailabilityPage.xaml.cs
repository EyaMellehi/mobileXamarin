using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace nnnnnnnnnnnnnnnn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoctorAvailabilityPage : ContentPage
    {
        private int doctorId;
        private int appointmentId;
        private const string BaseUrl = "http://192.168.1.6:4003/api";

        public DoctorAvailabilityPage(int doctorId, int appointmentId)
        {
            InitializeComponent();
            this.doctorId = doctorId;
            this.appointmentId = appointmentId;
            Console.WriteLine($"Doctor ID: {doctorId}, Appointment ID: {appointmentId}");

            LoadDoctorAvailability();
        }
        private async void LoadDoctorAvailability()
        {
            try
            {
                string apiUrl = $"{BaseUrl}/doctors/availability/{doctorId}";
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"Doctor ID: {doctorId}");

                    var response = await client.GetStringAsync(apiUrl);
                    Console.WriteLine("API Response: " + response);  // Log the raw response

                    var availabilities = JsonConvert.DeserializeObject<List<DoctorAvail>>(response);
                    // Add one day to each doctor's availability date
                    foreach (var availability in availabilities)
                    {
                        Console.WriteLine($"Updated Date: {availability.Date.ToString("yyyy-MM-dd")}");  // Log the updated date
                    }
                    if (availabilities != null && availabilities.Any())
                    {
                        // Use the main thread for UI updates
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            AvailabilityListView.ItemsSource = availabilities;
                        });
                    }
                    else
                    {
                        Console.WriteLine("No availability data returned.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching availability: {ex.Message}");
            }
        }

        


        private async void OnSelectAvailabilityClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var selectedAvailability = (DoctorAvail)button.BindingContext;

            try
            {
                // Step 1: Delete the selected time slot from the doctor_avail table
                string deleteApiUrl = $"{BaseUrl}/doctor_avail/{selectedAvailability.Id}";

                using (HttpClient client = new HttpClient())
                {
                    var deleteResponse = await client.DeleteAsync(deleteApiUrl);

                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Availability deleted successfully.");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to delete the availability", "OK");
                        return; // Exit if the deletion fails
                    }
                }

                // Step 2: Add the selected time slot to the appointment table
                string updateApiUrl = $"{BaseUrl}/appointments/{appointmentId}";

                var updatePayload = new
                {
                    date = selectedAvailability.Date,
                    hour = selectedAvailability.Hour,
                    etat = "encours", // The updated status of the appointment
                };

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(updatePayload), System.Text.Encoding.UTF8, "application/json");
                    var updateResponse = await client.PutAsync(updateApiUrl, content);

                    if (updateResponse.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Appointment updated successfully", "OK");
                        // Optionally, perform other actions or navigate
                        await Navigation.PopAsync();
                        MessagingCenter.Send(this, "AppointmentUpdated");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to update appointment", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appointment: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while updating the appointment", "OK");
            }
        }


        /*private async Task DeleteTimeSlotAsync(DoctorAvail selectedSlot)
        {
            try
            {
                // Build the API URL to delete the availability slot by ID
                string apiUrl = $"http:// 192.168.124.35:4003/api/doctor_avail/{selectedSlot.Id}";

                using (HttpClient client = new HttpClient())
                {
                    // Make a DELETE request to remove the availability slot
                    var response = await client.DeleteAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Time slot deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to delete the time slot. Status Code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the time slot. Please try again.", "OK");
            }
        }*/


        /* private async void OnSelectAvailabilityClicked(object sender, EventArgs e)
         {
             var button = (Button)sender;
             var selectedAvailability = (DoctorAvail)button.BindingContext;

             try
             {
                 string apiUrl = $"http://192.168.1.6:4003/api/appointments/{appointmentId}";

                 // Payload to send in the request body
                 var updatePayload = new
                 {
                     date = selectedAvailability.Date,
                     hour = selectedAvailability.Hour,
                     etat = "encours" // The updated status of the appointment
                 };

                 using (HttpClient client = new HttpClient())
                 {
                     var content = new StringContent(JsonConvert.SerializeObject(updatePayload), System.Text.Encoding.UTF8, "application/json");
                     var response = await client.PutAsync(apiUrl, content);

                     if (response.IsSuccessStatusCode)
                     {
                         await DisplayAlert("Success", "Appointment updated successfully", "OK");
                         // Optionally, delete the selected timeslot or perform other actions
                         await Navigation.PopAsync();
                         MessagingCenter.Send(this, "AppointmentUpdated");
                     }
                     else
                     {
                         await DisplayAlert("Error", "Failed to update appointment", "OK");
                     }
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Error updating appointment: {ex.Message}");
             }
         }*/

    }
}
    
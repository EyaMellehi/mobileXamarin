using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json.Linq;

namespace nnnnnnnnnnnnnnnn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        private string _email;
        private int _PatientId;
        private const string BaseUrl = "http://192.168.180.35:4003/api";


        public Home(string email,int id)
        {
            InitializeComponent();
            _email= email;
            _PatientId = id;
            _ = LoadDoctorsAsync();


        }
        private async Task LoadDoctorsAsync()
        {
            try
            {
                string baseUrl = $"{BaseUrl}/doctors"; // Your API endpoint

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(baseUrl); // Get the response from the API
                    Console.WriteLine("API Response: " + response); // Log the response

                    // Directly deserialize the JSON array into a list of Doctor objects
                    var doctors = JsonConvert.DeserializeObject<List<Doctor>>(response);

                    foreach (var doctor in doctors)
                    {
                        Console.WriteLine($"Doctor Name: {doctor.Nom}, Specialty: {doctor.Specialite}");
                    }

                    // Bind the doctors to the ListView
                    DoctorsListView.ItemsSource = doctors;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading doctors: {ex.Message}");
                // Optionally display an error to the user
                await DisplayAlert("Error", "There was an issue fetching the doctors.", "OK");
            }
        }




        private async void OnCheckAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                string baseUrl = $"{BaseUrl}/appointments/patient/{_PatientId}";
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(baseUrl);
                    var appointments = JsonConvert.DeserializeObject<List<Appointment>>(response);

                    if (appointments != null && appointments.Count > 0)
                    {
                        await Navigation.PushAsync(new AppointmentDetailsPage(appointments, _PatientId));
                    }
                    else
                    {
                        await DisplayAlert("No Appointment", "You don't have any appointments.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // General error handling
                await DisplayAlert("Error", "There was an issue fetching the appointment.", "OK");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async void OnMakeAppointmentClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var doctor = (Doctor)button.BindingContext;

            if (doctor == null)
            {
                await DisplayAlert("Error", "Doctor data not found.", "OK");
                return;
            }

            await Navigation.PushAsync(new Input(doctor.Specialite, doctor.Nom, doctor.Codem, _PatientId));
        }

    }
}



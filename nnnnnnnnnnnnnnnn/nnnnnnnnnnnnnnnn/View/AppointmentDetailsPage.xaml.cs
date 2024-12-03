using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace nnnnnnnnnnnnnnnn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppointmentDetailsPage : ContentPage
    {
        private int _patientId;
        private const string BaseUrl = "http://192.168.1.6:4003/api";
        public AppointmentDetailsPage(List<Appointment> appointments,int PatientId)
        {
            InitializeComponent();
            _patientId = PatientId;
            Console.WriteLine("Patient ID: " + _patientId);
            AppointmentsListView.ItemsSource = appointments;
            MessagingCenter.Subscribe<DoctorAvailabilityPage>(this, "AppointmentUpdated", (sender) =>
            {
                LoadAppointments();
            });

        }
        private async void OnDeclareAppointmentClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var appointment = (Appointment)button.BindingContext;
            Console.WriteLine("doctorID: " + appointment.Codem + "appoint id: " + appointment.Id);
            await Navigation.PushAsync(new DoctorAvailabilityPage(appointment.Codem, appointment.Id));
            LoadAppointments();
        }
        private async void LoadAppointments()
        {
            try
            {
                string apiUrl = $"{BaseUrl}/appointments/patient/{_patientId}";
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUrl); // Use GetAsync to handle status codes properly

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Check if the response is empty (status 204 No Content)
                        if (string.IsNullOrEmpty(responseBody))
                        {
                            AppointmentsListView.ItemsSource = new List<Appointment>(); // Empty list to avoid UI issues
                            Console.WriteLine("No active appointments found.");
                            await DisplayAlert("No Appointments", "You don’t have any appointments.", "OK");
                        }
                        else
                        {
                            var appointments = JsonConvert.DeserializeObject<List<Appointment>>(responseBody);
                            if (appointments != null && appointments.Count > 0)
                            {
                                AppointmentsListView.ItemsSource = appointments; // Populate the list view with active appointments
                            }
                            else
                            {
                                AppointmentsListView.ItemsSource = new List<Appointment>(); // Empty list if no active appointments
                                Console.WriteLine("No active appointments found.");
                                await DisplayAlert("No Appointments", "You don’t have any appointments.", "OK");
                            }
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        // If response is 204 (No Content), it means no appointments or all are cancelled
                        Console.WriteLine("No appointments found or all cancelled.");
                        await DisplayAlert("No Appointments", "You don’t have any appointments.", "OK");
                    }
                    else
                    {
                        // In case of any other error status code
                        Console.WriteLine($"Error fetching appointments: {response.ReasonPhrase}");
                        await DisplayAlert("Error", "An error occurred while fetching appointments", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // This is a catch block for network errors or unexpected issues
                Console.WriteLine($"Error fetching appointments: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while fetching appointments", "OK");
            }
        }






        private async void OnCancelAppointmentClicked(object sender, EventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var appointment = (Appointment)button.BindingContext;

                Console.WriteLine($"Cancelling appointment with ID: {appointment.Id}");

                string apiUrl = $"{BaseUrl}/appointments/cancel/{appointment.Id}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PutAsync(apiUrl, null); // No body needed for this request
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Appointment canceled successfully");
                        await DisplayAlert("Success", "Appointment canceled successfully", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to cancel appointment: {response.StatusCode}");
                        await DisplayAlert("Error", "Failed to cancel appointment", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling appointment: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while canceling the appointment", "OK");
            }
        }


    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace nnnnnnnnnnnnnnnn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Input : ContentPage
    {
        private string _doctorName;
        private int _doctorId;
        private const string BaseUrl = "http://192.168.180.35:4003/api";

        private int _patientId;
        private List<DoctorAvail> _availableSlots;

        public Input(string specialty, string doctorName, int doctorId, int patientId)
        {
            InitializeComponent();
            _doctorName = doctorName;
            _doctorId = doctorId;  // Store the doctor’s ID (CodeM)
            _patientId = patientId;  // Store the patient's ID

            InputLabel.Text = $"You are viewing available times with Dr. {doctorName} for {specialty}.";
            _availableSlots = new List<DoctorAvail>();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAvailableSlots(); // Charge les créneaux lors de l'affichage
        }
        private void OnTimeSlotSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var selectedSlot = e.SelectedItem as DoctorAvail;
                // Handle the selected slot logic here, for example:
                // You can update other UI elements or perform further actions
            }
        }
        // Method to handle the submission of the form
        private async void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            if (AvailableTimesListView.SelectedItem == null)
            {
                Console.WriteLine("No time slot selected.");
                await DisplayAlert("Error", "Please select an available time slot.", "OK");
                return;
            }

            var selectedSlot = (DoctorAvail)AvailableTimesListView.SelectedItem;
            Console.WriteLine($"Selected Slot: ID = {selectedSlot.Id}, Date = {selectedSlot.Date}, Hour = {selectedSlot.Hour}");


            Appointment appointment = new Appointment
            {
                Codem = _doctorId,
                Patient = _patientId,
                Date = selectedSlot.Date,
                Etat = "encours",
                Hour = selectedSlot.Hour
            };
            Console.WriteLine(selectedSlot.Id);
            await SaveAppointmentAsync(appointment);
            await DeleteTimeSlotAsync(selectedSlot);
            await DisplayAlert("Success", "Your appointment has been submitted. Wait for confirmation.", "OK");
            await Navigation.PopAsync();
        }

        private async Task DeleteTimeSlotAsync(DoctorAvail selectedSlot)
        {
            try
            {
                if (selectedSlot == null || selectedSlot.Id == 0)
                {
                    Console.WriteLine("Invalid selected slot: null or ID is 0.");
                    await DisplayAlert("Error", "Invalid time slot selected.", "OK");
                    return;
                }

                string apiUrl = $"{BaseUrl}/doctor_avail/{selectedSlot.Id}";
                Console.WriteLine($"Deleting time slot: ID = {selectedSlot.Id}, URL = {apiUrl}");

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.DeleteAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Time slot deleted successfully.");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to delete time slot. Status Code: {response.StatusCode}, Content: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the time slot. Please try again.", "OK");
            }
        }


        private async Task LoadAvailableSlots()
        {
            try
            {
                string apiUrl = $"{BaseUrl}/doctors/availability/{_doctorId}";
                Console.WriteLine($"Doctor ID: {_doctorId}");


                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl);
                    Console.WriteLine("API Response: " + response);

                    _availableSlots = JsonConvert.DeserializeObject<List<DoctorAvail>>(response);

                    Console.WriteLine("Raw API Response: " + response);

                    foreach (var slot in _availableSlots)
                    {
                        Console.WriteLine($"Slot: ID = {slot.Id}, Date = {slot.Date}, Hour = {slot.Hour}");
                    }


                    AvailableTimesListView.ItemsSource = _availableSlots;


                    if (_availableSlots.Count == 0)
                    {
                        await DisplayAlert("No Slots", "No available slots for this doctor.", "OK");
                    }
                    else

                    {
                        AvailableTimesListView.ItemsSource = _availableSlots;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while fetching available slots. Please try again.", "OK");
            }
        }


        // Method to save the appointment to the backend
        private async Task SaveAppointmentAsync(Appointment appointment)
        {
            try
            {
                string apiUrl = $"{BaseUrl}/appointments";

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(appointment);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Appointment saved successfully: " + responseContent);
                        Console.WriteLine($"Codem: {appointment.Codem}, Patient: {appointment.Patient}, Date: {appointment.Date}, Hour: {appointment.Hour}, Etat: {appointment.Etat}");

                    }
                    else
                    {
                        Console.WriteLine($"Failed to save appointment. Status Code: {response.StatusCode}, Response: {responseContent}");
                        //await DisplayAlert("Error", "Failed to save the appointment. Please check the server logs.", "OK");
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving your appointment. Please try again.", "OK");
            }
        }

    }
  
}
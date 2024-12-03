using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace nnnnnnnnnnnnnnnn
{
    public partial class MainPage : ContentPage
    {
        private Service _service;
        public MainPage()
        {
            InitializeComponent();
            _service = new Service();
            TestDatabaseConnection();

        }
        private async void TestDatabaseConnection()
        {
            try
            {
                bool isConnected = await _service.TestConnectionAsync();
                if (isConnected)
                {
                    await DisplayAlert("Connection Status", "Connection Successful", "OK");
                }
                else
                {
                    await DisplayAlert("Connection Status", "Connection Failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Status", $"Connection Failed: {ex.Message}", "OK");
            }
        }
      
       
        private async void Button_Clicked(Object sender, EventArgs e)
        {
            
            try
            {
                var patient = await _service.LoginAsync(UsernameEntry.Text, PasswordEntry.Text);
                await DisplayAlert("Login", "Login Successful", "OK");

                await Navigation.PushAsync(new Home(patient.Email,patient.Id)); // Assuming Home takes patient details
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUp());
        }
    }
}

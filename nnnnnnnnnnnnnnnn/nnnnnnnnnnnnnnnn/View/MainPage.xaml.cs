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
        // Déclaration d'un objet Service,pour gérer la communication avec une base de données
        private Service _service;
        public MainPage()
        {
            InitializeComponent();
            _service = new Service();// Création d'une nouvelle instance de la classe Service
            TestDatabaseConnection();// Teste la connexion à la base de données au démarrage de la page

        }
        private void EyeIcon_Clicked(object sender, EventArgs e)
        {
            // Check the current visibility and toggle
            if (PasswordEntry.IsPassword)
            {
                PasswordEntry.IsPassword = false;  // Show password
                EyeIcon.Source = "eya.png";  // Change icon to open eye
            }
            else
            {
                PasswordEntry.IsPassword = true;  // Hide password
                EyeIcon.Source = "ceye.png";  // Change icon to closed eye
            }
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

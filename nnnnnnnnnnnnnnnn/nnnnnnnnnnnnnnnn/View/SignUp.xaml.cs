using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace nnnnnnnnnnnnnnnn
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUp : ContentPage
    {
        private Service _service;

        public SignUp()
        {
            InitializeComponent();
            _service = new Service();
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            String email= EmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(NomEntry.Text))
            {
                await DisplayAlert("Erreur", "Le champ 'Nom' est obligatoire.", "OK");
                return;
            }  

            if (string.IsNullOrWhiteSpace(TelephoneEntry.Text) || !Regex.IsMatch(TelephoneEntry.Text, @"^\d{8}$"))
            {
                await DisplayAlert("Erreur", "Veuillez entrer un numéro de téléphone valide (8 chiffres).", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                await DisplayAlert("Erreur", "Veuillez entrer une adresse email valide.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 6)
            {
                await DisplayAlert("Erreur", "Le mot de passe doit contenir au moins 6 caractères.", "OK");
                return;
            }

            var patient = new Patient
            {
                Nom = NomEntry.Text,
                Telephone = TelephoneEntry.Text,
                Email = email,
                pwd = PasswordEntry.Text
            };
            Console.WriteLine($"Nom: {patient.Nom}, Telephone: {patient.Telephone}, Email: {patient.Email}, pwd: {patient.pwd}");

           // await _service.AddPatientAsync(patient);
             try
             {
                 await _service.AddPatientAsync(patient);
                //await DisplayAlert("Success", "Registration successful!", "OK");
                 await Navigation.PushAsync(new MainPage());
             }
             catch (Exception ex)
             {
                 await DisplayAlert("Error", ex.Message, "OK");
             }
        }
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}
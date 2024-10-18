using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using MySql.Data.MySqlClient;

namespace nnnnnnnnnnnnnnnn
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            /* string conn = "Server=192.168.56.1;  Database=centremedical; Uid=root;  Pwd=";
             MySqlConnection connection = new MySqlConnection(conn);
             try
             {
                 connection.Open();
                 DisplayAlert("conn","success","ok");
             }
             catch (Exception ex)
             {
                 DisplayAlert("Alert",ex.Message,"ok");
             }
             finally
             {
                 connection.Close();
             }*/
        }
        private void Button_Clicked(Object sender, EventArgs e)
        {
            if (UsernameEntry.Text == "eya" && PasswordEntry.Text == "123")
            {
                DisplayAlert("Login", "Login Successful", "OK");
                Navigation.PushAsync(new Home(UsernameEntry.Text));
            }
            else
            {
                DisplayAlert("Ops...", "Username or password incorrect", "OK");
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
        }
    }
}

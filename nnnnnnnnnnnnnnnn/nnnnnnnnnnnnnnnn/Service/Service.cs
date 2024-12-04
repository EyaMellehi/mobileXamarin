using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

using System.Text;

namespace nnnnnnnnnnnnnnnn
{
    // Classe qui gère la communication avec l'API et la gestion des patients.
    class Service
    {
        private readonly HttpClient _httpClient; // Déclaration de l'instance HttpClient pour effectuer des requêtes HTTP
        public Service()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://192.168.180.35:4003/api/");

        }
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("status"); 
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task AddPatientAsync(Patient patient)
        {
            // Sérialisation de l'objet patient en JSON
            var json = JsonConvert.SerializeObject(patient);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // Envoi d'une requête POST à l'endpoint "patient"
            var response = await _httpClient.PostAsync("patient", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Server Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to add patient: {responseContent}");
            }
        }
        public async Task<Patient> LoginAsync(string email, string password)
        {
            Console.WriteLine($"Attempting login with Email: {email}");
            email = email?.Trim();
            password = password?.Trim();

            var json = JsonConvert.SerializeObject(new { email, pwd = password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Envoi de la requête POST à l'endpoint "login"
                var response = await _httpClient.PostAsync("login", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server Response: {jsonResponse}");

                if (response.IsSuccessStatusCode)
                {
                    // Si la connexion est réussie, on essaie de désérialiser la réponse en un objet Patient

                    var patient = JsonConvert.DeserializeObject<Patient>(jsonResponse);
                    if (patient != null)
                    {
                        return patient;// Retourne l'objet patient
                    }
                    else
                    {
                        throw new Exception("Failed to deserialize patient data");
                    }
                }
                else
                {
                    
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                        if (errorResponse.ContainsKey("message"))
                        {
                            throw new Exception($"Login failed: {errorResponse["message"]}");
                        }
                    }
                    catch
                    {
                        throw new Exception($"Login failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new Exception($"Network error occurred: {e.Message}");
            }
            catch (JsonException e)
            {
                throw new Exception($"Error parsing server response: {e.Message}");
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred: {e.Message}");
            }

            // Ajoutez une valeur de retour par défaut dans le cas où tout échoue
            throw new Exception("Unexpected error: LoginAsync did not return a value.");
        }


    }
}

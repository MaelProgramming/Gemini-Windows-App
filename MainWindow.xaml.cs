using System.Collections.Generic;
using System;
using System.IO;

using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Controls;

using Microsoft.UI.Xaml.Controls.Primitives;

using Microsoft.UI.Xaml.Data;

using Microsoft.UI.Xaml.Input;

using Microsoft.UI.Xaml.Media;

using Microsoft.UI.Xaml.Navigation;

using Google.GenAI;

using Windows.Foundation;

using Windows.Foundation.Collections;

using Microsoft.Extensions.AI;

using DotNetEnv;


namespace Gemini_Windows_App
{
    public sealed partial class MainWindow : Window
    {
        private string _apiKey; // On ne l'initialise pas ici !

        public MainWindow()
        {
            // 1. Charger le .env AVANT toute chose
            Env.Load();
            _apiKey = Env.GetString("GEMINI_API_KEY");

            this.InitializeComponent();
            
            UserInput.KeyDown += UserInput_KeyDown;
        }

        private void UserInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Vérification simple du Shift dans WinUI 3
                var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                if ((shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) != Windows.UI.Core.CoreVirtualKeyStates.Down)
                {
                    e.Handled = true;
                    myButton_Click(this, new RoutedEventArgs());
                }
            }
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInput.Text)) return;
            if (string.IsNullOrEmpty(_apiKey)) {
                ResponseText.Text = "Erreur : Clé API manquante dans le .env";
                return;
            }

            myButton.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Visible;
            ResponseText.Text = "Gemini réfléchit...";

            try
            {
                // Utilise le bon modèle (2.0-flash est stable)
                var client = new Client(apiKey: _apiKey);
                var model = "gemini-2.0-flash"; 

                var response = await client.Models.GenerateContentAsync(model, UserInput.Text);

                // Sécurité : Vérifier si on a une réponse
                if (response?.Candidates != null && response.Candidates.Count > 0)
                {
                    ResponseText.Text = response.Candidates[0].Content.Parts[0].Text;
                    UserInput.Text = string.Empty;
                }
                else
                {
                    ResponseText.Text = "Gemini n'a pas pu générer de réponse.";
                }
            }
            catch (Exception ex)
            {
                ResponseText.Text = $"Erreur : {ex.Message}";
            }
            finally
            {
                myButton.IsEnabled = true;
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ResponseText.Text)) return;

            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(ResponseText.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            // On évite d'écraser le texte par "Copié !", on pourrait utiliser un Flyout ou changer la couleur
        }
    }
}
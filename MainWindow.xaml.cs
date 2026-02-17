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



namespace Gemini_Windows_App

{

    /// <summary>

    /// An empty window that can be used on its own or navigated to within a Frame.

    /// </summary>

    public sealed partial class MainWindow : Window

    {

        private readonly string _apiKey = "AIzaSyDooQg_rx0ju9O6RqvGCPZdxZa-FfZgUmc";

        public MainWindow()

        {

            InitializeComponent();
            // Lecture du Keydown Enter
            UserInput.KeyDown += UserInput_KeyDown;

        }
        private void UserInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Empêche le retour à la ligne si on n'appuie pas sur Shift
                var shiftLabel = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                if (shiftLabel == Windows.UI.Core.CoreVirtualKeyStates.None)
                {
                    e.Handled = true;
                    myButton_Click(this, new RoutedEventArgs());
                }
            }
        }
        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInput.Text)) return;

            // 1. Préparation de l'UI
            myButton.IsEnabled = false;
            LoadingBar.Visibility = Visibility.Visible;
            ResponseText.Text = "Gemini réfléchit..."; // On laisse ce texte !

            try
            {
                var client = new Client(apiKey: _apiKey);
                var model = "gemini-2.5-flash";

                var response = await client.Models.GenerateContentAsync(model, UserInput.Text);

                // 2. On remplace le message de chargement par la vraie réponse
                ResponseText.Text = response.Candidates[0].Content.Parts[0].Text;

                // On vide l'INPUT, pas la réponse !
                UserInput.Text = string.Empty;
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
            ResponseText.Text = "Texte copié dans le presse-papier !";
        }


    }

}
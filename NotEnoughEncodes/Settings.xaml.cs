using System.Windows;

namespace NotEnoughEncodes
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            //Loads the previously saved values
            if (MainWindow.logging == true)
            {
                CheckBoxLogging.IsChecked = true;
            }
            if (MainWindow.shutdownafterencode == true)
            {
                CheckBoxShutdown.IsChecked = true;
            }
            if (MainWindow.enableCustomSettings == true)
            {
                CheckBoxCustomSettings.IsChecked = true;
            }
            if (MainWindow.batchEncoding == true)
            {
                CheckBoxBatchMode.IsChecked = true;
            }
        }

        private bool checkboxlogging;
        private bool shutDownAfterEncode;
        private bool batchEncode;
        private bool customSettings;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxLogging.IsChecked == true)
            {
                checkboxlogging = true;
            }
            if (CheckBoxShutdown.IsChecked == true)
            {
                shutDownAfterEncode = true;
            }
            if (CheckBoxCustomSettings.IsChecked == true)
            {
                customSettings = true;
            }
            if (CheckBoxBatchMode.IsChecked == true)
            {
                batchEncode = true;
            }
            SendSettingsToSave(checkboxlogging, shutDownAfterEncode, batchEncode, customSettings);
        }

        private void SendSettingsToSave(bool enableLog, bool shutDown, bool batch, bool settings)
        {
            MainWindow.SaveSettings(enableLog, shutDown, batch, settings);
            //Closes the Window after Settings have been send to main window
            this.Close();
        }

    }
}
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
        }

        private bool checkboxlogging;
        private bool shutDownAfterEncode;
        private bool batchEncode;

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
            SendSettingsToSave(checkboxlogging, shutDownAfterEncode, batchEncode);
        }

        private void SendSettingsToSave(bool enableLog, bool shutDown, bool batch)
        {
            MainWindow.SaveSettings(enableLog, shutDown, batch);
            //Closes the Window after Settings have been send to main window
            this.Close();
        }
    }
}
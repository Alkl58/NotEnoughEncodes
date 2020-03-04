using System.Windows;
using System.Windows.Controls;

namespace NotEnoughEncodes
{
    /// <summary>
    /// Interaktionslogik für AudioSettings.xaml
    /// </summary>
    public partial class AudioSettings : Window
    {
        public AudioSettings()
        {
            InitializeComponent();
            if (MainWindow.audioCodec != null)
            {
                ComboBoxAudioCodec.Text = MainWindow.audioCodec;
            }
            if (MainWindow.audioBitrate != null)
            {
                TextBoxAudioBitrate.Text = MainWindow.audioBitrate;
            }
        }
        string text;
        private void ComboBoxAudioCodec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
 
            text = (e.AddedItems[0] as ComboBoxItem).Content as string;

            if (text == "MP3 VBR")
            {
                LabelBitrate.Content = "Quality (0-9)";
            }
            else if (text == "Copy Audio")
            {
                TextBoxAudioBitrate.IsEnabled = false;
            }
            else if (TextBoxAudioBitrate != null)
            {
                if (TextBoxAudioBitrate.IsEnabled == false || text != "MP3 VBR")
                {
                    TextBoxAudioBitrate.IsEnabled = true;
                    LabelBitrate.Content = "Bitrate (kbps)";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            MainWindow.SaveAudioSettings(ComboBoxAudioCodec.Text, TextBoxAudioBitrate.Text);
            this.Close();
        }
    }
}
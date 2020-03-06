using System;
using System.Windows;
using System.Windows.Controls;

namespace NotEnoughEncodes
{
    /// <summary>
    /// Interaktionslogik für AudioSettings.xaml
    /// </summary>
    public partial class AudioSettings : Window
    {
        string text;
        bool trackone;
        bool tracktwo;
        bool trackthree;
        bool trackfour;
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
            if (MainWindow.trackoneactive == false)
            {
                CheckBoxTrackOne.IsChecked = false;

            }
            else if (MainWindow.trackoneactive == true)
            {
                CheckBoxTrackOne.IsChecked = true;
            }
            if (MainWindow.tracktwoactive == false)
            {
                CheckBoxTrackTwo.IsChecked = false;
            }
            else if (MainWindow.tracktwoactive == true)
            {
                CheckBoxTrackTwo.IsChecked = true;
            }
            if (MainWindow.trackthreeactive == false)
            {
                CheckBoxTrackThree.IsChecked = false;
            }
            else if (MainWindow.trackthreeactive == true)
            {
                CheckBoxTrackThree.IsChecked = true;
            }
            if (MainWindow.trackfouractive == false)
            {
                CheckBoxTrackFour.IsChecked = false;
            }
            else if (MainWindow.trackfouractive == true)
            {
                CheckBoxTrackFour.IsChecked = true;
            }

        }

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
            trackone = CheckBoxTrackOne.IsChecked == true;
            tracktwo = CheckBoxTrackTwo.IsChecked == true;
            trackthree = CheckBoxTrackThree.IsChecked == true;
            trackfour = CheckBoxTrackFour.IsChecked == true;
            //Console.WriteLine(trackone + " " + tracktwo + " " + trackthree + " " + trackfour);
            SaveSettings();
        }

        private void SaveSettings()
        {
            MainWindow.SaveAudioSettings(ComboBoxAudioCodec.Text, TextBoxAudioBitrate.Text, trackone, tracktwo, trackthree, trackfour);
            this.Close();
        }
    }
}
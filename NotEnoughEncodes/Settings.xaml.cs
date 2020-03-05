using System.IO;
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
            if (MainWindow.deleteTempFiles == true)
            {
                CheckBoxDeleteTempFiles.IsChecked = true;
            }
            if (MainWindow.tempFolderActive == true)
            {
                CheckBoxTempFolder.IsChecked = true;
                TextBoxTempFolder.Text = MainWindow.tempFolder;
            }
        }

        private bool checkboxlogging;
        private bool shutDownAfterEncode;
        private bool batchEncode;
        private bool customSettings;
        private bool deletefiles;
        private bool tempFolder = false;
        private bool aomencFolder = false;
        private bool ffmpegFolder = false;
        private bool ffprobeFolder = false;
        private string tempFolderDir;
        private string aomencFolderDir;
        private string ffmpegFolderDir;
        private string ffprobeFolderDir;
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
            if (CheckBoxDeleteTempFiles.IsChecked == true)
            {
                deletefiles = true;
            }
            if (CheckBoxTempFolder.IsChecked == true)
            {
                tempFolder = true;
                tempFolderDir = TextBoxTempFolder.Text;
            }
            if (CheckBoxAomencFolder.IsChecked == true)
            {
                aomencFolder = true;
                aomencFolderDir = TextBoxAomenc.Text;
            }
            if (CheckBoxFfmpegFolder.IsChecked == true)
            {
                ffmpegFolder = true;
                ffmpegFolderDir = TextBoxFfmpeg.Text;
            }
            if (CheckBoxFfprobeFolder.IsChecked == true)
            {
                ffprobeFolder = true;
                ffprobeFolderDir = TextBoxFfprobe.Text;
            }
            SendSettingsToSave(checkboxlogging, shutDownAfterEncode, batchEncode, customSettings, deletefiles, tempFolder, aomencFolder, ffmpegFolder, ffprobeFolder, tempFolderDir, aomencFolderDir, ffmpegFolderDir, ffprobeFolderDir);
        }

        private void SendSettingsToSave(bool enableLog, bool shutDown, bool batch, bool settings, bool delete, bool tempFolderChunks, bool aomencDir, bool ffmpegDir, bool ffprobeDir, string temp, string aomenc, string ffmpeg, string ffprobe)
        {
            MainWindow.SaveSettings(enableLog, shutDown, batch, settings, delete, tempFolderChunks, aomencDir, ffmpegDir, ffprobeDir, temp, aomenc, ffmpeg, ffprobe);
            //Closes the Window after Settings have been send to main window
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CheckBoxTempFolder.IsChecked == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxTempFolder.Text = browse.SelectedPath;
                }
            }

        }

        private void Button_aomenc_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxAomencFolder.IsChecked == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxAomenc.Text = browse.SelectedPath;
                    bool fileExist = File.Exists(TextBoxAomenc.Text + "\\aomenc.exe");

                    if (fileExist != true)
                    {
                        MessageBox.Show("Couldn't find aomenc in the directory!");
                    }

                }
            }
        }

        private void ButtonFfmpeg_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxFfmpegFolder.IsChecked == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxFfmpeg.Text = browse.SelectedPath;
                    bool fileExist = File.Exists(TextBoxFfmpeg.Text + "\\ffmpeg.exe");

                    if (fileExist != true)
                    {
                        MessageBox.Show("Couldn't find ffmpeg in the directory!");
                    }

                }
            }

        }

        private void ButtonFfprobe_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxFfprobeFolder.IsChecked == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxFfprobe.Text = browse.SelectedPath;
                    bool fileExist = File.Exists(TextBoxFfprobe.Text + "\\ffprobe.exe");

                    if (fileExist != true)
                    {
                        MessageBox.Show("Couldn't find ffprobe in the directory!");
                    }

                }
            }

        }

        private void CheckBoxTempFolder_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxTempFolder.IsEnabled = true;
        }
        private void CheckBoxTempFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxTempFolder.IsEnabled = false;
        }

        private void CheckBoxAomencFolder_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxAomenc.IsEnabled = true;
        }
        private void CheckBoxAomencFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxAomenc.IsEnabled = false;
        }

        private void CheckBoxFfmpegFolder_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxFfmpeg.IsEnabled = true;
        }
        private void CheckBoxFfmpegFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxFfmpeg.IsEnabled = false;
        }

        private void CheckBoxFfprobeFolder_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxFfprobe.IsEnabled = true;
        }
        private void CheckBoxFfprobeFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxFfprobe.IsEnabled = false;
        }
    }
}
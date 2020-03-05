using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NotEnoughEncodes
{
    public partial class MainWindow : Window
    {
        public static bool logging;
        public static bool shutdownafterencode;
        public static bool batchEncoding;
        public static bool enableCustomSettings = false;
        public static string audioCodec;
        public static string audioBitrate;
        public static bool deleteTempFiles = false;
        public static string tempFolder;
        public static bool tempFolderActive;

        public MainWindow()
        {
            InitializeComponent();

            //Get Number of Cores
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            //Sets the Number of Workers = Phsyical Core Count
            int tempCorecount = coreCount * 1 / 2;
            TextBoxNumberWorkers.Text = tempCorecount.ToString();
            //Checks for aomenc, ffmpeg and ffprobe
            SmallScripts.CheckDependencies();
            //Load Settings
            ReadSettings();
            LoadUnfinishedJob();
        }

        public void ReadSettings()
        {
            try
            {
                //If settings.ini exist -> Set all Values
                bool fileExist = File.Exists("settings.ini");

                if (fileExist)
                {
                    string[] lines = File.ReadAllLines("settings.ini");

                    TextBoxNumberWorkers.Text = lines[0];
                    ComboBoxCpuUsed.Text = lines[1];
                    ComboBoxBitdepth.Text = lines[2];
                    TextBoxEncThreads.Text = lines[3];
                    TextBoxcqLevel.Text = lines[4];
                    TextBoxKeyframeInterval.Text = lines[5];
                    TextBoxTileCols.Text = lines[6];
                    TextBoxTileRows.Text = lines[7];
                    ComboBoxPasses.Text = lines[8];
                    TextBoxFramerate.Text = lines[9];
                    ComboBoxEncMode.Text = lines[10];
                    TextBoxChunkLength.Text = lines[11];
                    audioCodec = lines[12];
                    audioBitrate = lines[13];
                    if (lines[14] == "True")
                    {
                        enableCustomSettings = true;
                        CheckBoxCustomSettings.IsChecked = true;
                    }
                    if (lines[15] == "True")
                    {
                        CheckBoxEnableAudio.IsChecked = true;
                    }
                    tempFolder = lines[16];
                    if (lines[17] == "True")
                    {
                        tempFolderActive = true;
                    }
                }
                //Reads custom settings to settings_custom.ini
                bool customFileExist = File.Exists("settings_custom.ini");
                if (customFileExist)
                {
                    string[] linesa = File.ReadAllLines("settings_custom.ini");
                    TextBoxCustomSettings.Text = linesa[0];
                }
            }
            catch { }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Saves all Current Settings to a file
            string audioCheckBox = CheckBoxEnableAudio.IsChecked.ToString();
            string customSettingsBool = enableCustomSettings.ToString();
            string tempFolderSaveActive = tempFolderActive.ToString();
            string customSettings = TextBoxCustomSettings.Text;
            string maxConcurrency = TextBoxNumberWorkers.Text;
            string kfmaxdist = TextBoxKeyframeInterval.Text;
            string chunkLength = TextBoxChunkLength.Text;
            string audioSettingsBitrate = audioBitrate;
            string encThreads = TextBoxEncThreads.Text;
            string bitDepth = ComboBoxBitdepth.Text;
            string tilecols = TextBoxTileCols.Text;
            string tilerows = TextBoxTileRows.Text;
            string audioSettingsCodec = audioCodec;
            string encMode = ComboBoxEncMode.Text;
            string cpuUsed = ComboBoxCpuUsed.Text;
            string nrPasses = ComboBoxPasses.Text;
            string cqLevel = TextBoxcqLevel.Text;
            string fps = TextBoxFramerate.Text;
            string tempFolderSave = tempFolder;

            if (audioSettingsCodec == "" || audioSettingsCodec == null)
            {
                audioSettingsCodec = "Opus";
            }
            if (audioSettingsBitrate == "" || audioSettingsBitrate == null)
            {
                audioSettingsBitrate = "128";
            }

            //Saves custom settings in settings_custom.ini
            if (CheckBoxCustomSettings.IsChecked == true)
            {
                string[] linescustom = { customSettings };
                File.WriteAllLines("settings_custom.ini", linescustom);
            }

            string[] lines = { maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, fps, encMode, chunkLength, audioSettingsCodec, audioSettingsBitrate, customSettingsBool, audioCheckBox, tempFolderSave, tempFolderSaveActive };
            File.WriteAllLines("settings.ini", lines);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (batchEncoding == false || batchEncoding == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    TextBoxInputVideo.Text = openFileDialog.FileName;
                if (TextBoxInputVideo.Text != "" && TextBoxInputVideo.Text != null && TextBoxInputVideo.Text != " Input Video")
                {
                    GetStreamFps(TextBoxInputVideo.Text);
                    SmallScripts.GetStreamLength(TextBoxInputVideo.Text);
                }
            }
            else if (batchEncoding == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxInputVideo.Text = browse.SelectedPath;
                }
            }
            //Open File Dialog

            //Set the Stream Framerate from the video
        }

        public static string streamLength;
        public string streamFps;

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            if (batchEncoding == false || batchEncoding == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Matroska|*.mkv";
                if (saveFileDialog.ShowDialog() == true)
                    TextBoxOutputVideo.Text = saveFileDialog.FileName;
            }
            else if (batchEncoding == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxOutputVideo.Text = browse.SelectedPath;
                }
            }
        }

        bool folderExist;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tempFolderActive == false)
            {
                folderExist = Directory.Exists("Chunks");
            }
            else
            {
                folderExist = Directory.Exists(tempFolderActive + "\\Chunks");
            }

            if (prgbar.Value != 0)
            {
                prgbar.Value = 0;
                prgbar.Maximum = 100;
            }


            if (folderExist && Cancel.CancelAll == false && CheckBoxResume.IsChecked == false)
            {
                if (MessageBox.Show("It appears that you finished a previous encode but forgot to delete the temp files. To let the program delete the files, press Yes. Press No if pressing on Encode was a mistake.",
                        "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SmallScripts.DeleteTempFiles();
                    SmallScripts.DeleteTempFilesDir(tempFolder);
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "Restarting...", DispatcherPriority.Background);
                    //This will be set if you Press Encode after a already finished encode
                    prgbar.Maximum = 100;
                    prgbar.Value = 0;
                }
                else
                {
                    Cancel.CancelAll = true;
                }
            }

            if (Cancel.CancelAll == true && CheckBoxResume.IsChecked == false)
            {
                //Asks the user if he wants to resume the process.
                if (MessageBox.Show("It appears that you canceled a previous encode. If you want to resume an cancelled encode, press Yes.", "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    CheckBoxResume.IsChecked = true;
                    Cancel.CancelAll = false;
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "Resuming...", DispatcherPriority.Background);
                    prgbar.Maximum = 100;
                    prgbar.Value = 0;
                }
                else
                {
                    Cancel.CancelAll = false;
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "Starting...", DispatcherPriority.Background);
                    prgbar.Maximum = 100;
                    prgbar.Value = 0;
                }
            }
            else if (Cancel.CancelAll == true && CheckBoxResume.IsChecked == true)
            {
                Cancel.CancelAll = false;
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Resuming...", DispatcherPriority.Background);
                prgbar.Maximum = 100;
                prgbar.Value = 0;
            }

            if (logging == true)
            {
                SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Clicked on Start Encode", "log.log");
            }

            if (TextBoxInputVideo.Text == " Input Video")
            {
                MessageBox.Show("No Input File selected!");

                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " No Input File selected!", "log.log");
                }
            }
            else if (TextBoxOutputVideo.Text == " Output Video")
            {
                MessageBox.Show("No Output Path specified!");

                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " No Output Path specified!", "log.log");
                }
            }
            else if (TextBoxInputVideo.Text != " Input Video")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Started MainClass()", "log.log");
                }
                if (Cancel.CancelAll == false)
                {
                    //Start MainClass
                    if (batchEncoding == false)
                    {
                        MainClass();
                    }
                    else if (batchEncoding == true)
                    {
                        BatchEncode();
                    }
                }
                else
                {
                    pLabel.Dispatcher.Invoke(() => pLabel.Content = "Process has been canceled!", DispatcherPriority.Background);
                }
            }
        }

        public string batchencodefile;
        public string batchencodeoutputfile;
        public bool batchFilefinished;
        public DateTime starttimea;

        public async void BatchEncode()
        {
            batchFilefinished = true;
            DirectoryInfo batchfiles = new DirectoryInfo(TextBoxInputVideo.Text);
            foreach (var file in batchfiles.GetFiles())
            {
                batchFilefinished = false;
                batchencodefile = TextBoxInputVideo.Text + "\\" + file;
                batchencodeoutputfile = TextBoxOutputVideo.Text + "\\" + file + "_av1.mkv";

                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Starting...", DispatcherPriority.Background);
                //This will be set if you Press Encode after a already finished encode
                prgbar.Maximum = 100;
                prgbar.Value = 0;

                //Sets the working directory
                //Sets the working directory
                if (tempFolderActive == false)
                {
                    currentPath = Directory.GetCurrentDirectory();
                }
                else if (tempFolderActive == true)
                {
                    currentPath = tempFolder;
                }
                //Checks if Chunks folder exist, if no it creates Chunks folder
                if (!Directory.Exists(Path.Combine(currentPath, "Chunks")))
                    Directory.CreateDirectory(Path.Combine(currentPath, "Chunks"));
                GetStreamFps(batchencodefile);
                SmallScripts.GetStreamLength(batchencodefile);

                string customSettings = TextBoxCustomSettings.Text;
                string chunkLength = TextBoxChunkLength.Text;

                string streamLenghtVideo = streamLength;
                string encMode = ComboBoxEncMode.Text;
                string streamFrameRate = streamFps;
                string fps = TextBoxFramerate.Text;

                int maxConcurrency = Int16.Parse(TextBoxNumberWorkers.Text);
                int kfmaxdist = Int16.Parse(TextBoxKeyframeInterval.Text);
                int encThreads = Int16.Parse(TextBoxEncThreads.Text);
                int bitDepth = Int16.Parse(ComboBoxBitdepth.Text);
                int cpuUsed = Int16.Parse(ComboBoxCpuUsed.Text);
                int cqLevel = Int16.Parse(TextBoxcqLevel.Text);

                int tilecols = Int16.Parse(TextBoxTileCols.Text);
                int tilerows = Int16.Parse(TextBoxTileRows.Text);
                int nrPasses = Int16.Parse(ComboBoxPasses.Text);

                bool customSettingsbool = false;
                bool audioOutput = false;
                bool reencode = false;
                bool resume = false;

                if (CheckBoxReencode.IsChecked == true)
                {
                    reencode = true;
                }
                if (CheckBoxEnableAudio.IsChecked == true)
                {
                    audioOutput = true;
                }
                if (CheckBoxCustomSettings.IsChecked == true)
                {
                    customSettingsbool = true;
                }

                Console.WriteLine(batchencodefile);

                await Task.Run(() => Splitting(batchencodefile, resume, logging, reencode, chunkLength, currentPath));
                //Audio Encoding
                if (CheckBoxEnableAudio.IsChecked == true && CheckBoxResume.IsChecked == false)
                {
                    await Task.Run(() => AudioEncode.EncodeAudio(batchencodefile, logging, audioBitrate, audioCodec, currentPath));
                }
                if (CheckBoxResume.IsChecked == false)
                {
                    await Task.Run(() => Rename.RenameChunks(currentPath));
                }

                //Create Array List with all Chunks
                string[] chunks;
                //Sets the Chunks directory
                string sdira = currentPath + "\\Chunks";
                //Add all Files in Chunks Folder to array
                chunks = Directory.GetFiles(sdira, "*mkv", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();
                //Parse Textbox Text to String for loop threading

                string finalEncodeMode = "";

                //Sets the Encoding Mode
                if (encMode == "q")
                {
                    if (logging == true)
                    {
                        SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to q", "log.log");
                    }
                    finalEncodeMode = " --end-usage=q --cq-level=" + cqLevel;
                }
                else if (encMode == "vbr")
                {
                    //If vbr set finalEncodeMode
                    if (logging == true)
                    {
                        SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to vbr", "log.log");
                    }
                    finalEncodeMode = " --end-usage=vbr --target-bitrate=" + cqLevel;
                }
                else if (encMode == "cbr")
                {
                    //If cbr set finalEncodeMode
                    if (logging == true)
                    {
                        SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to cbr", "log.log");
                    }
                    finalEncodeMode = " --end-usage=cbr --target-bitrate=" + cqLevel;
                }

                string allSettingsAom = "";
                //Sets aom settings to custom or preset
                if (customSettingsbool == true)
                {
                    allSettingsAom = " " + customSettings;
                    //Console.WriteLine(allSettingsAom);
                }
                else if (customSettingsbool == false)
                {
                    allSettingsAom = " --cpu-used=" + cpuUsed + " --threads=" + encThreads + finalEncodeMode + " --bit-depth=" + bitDepth + " --tile-columns=" + tilecols + " --fps=" + fps + " --tile-rows=" + tilerows + " --kf-max-dist=" + kfmaxdist;
                    //Console.WriteLine(allSettingsAom);
                }

                prgbar.Dispatcher.Invoke(() => prgbar.Maximum = chunks.Count(), DispatcherPriority.Background);
                //prgbar.Maximum = chunks.Count();
                //Set the Progresslabel to 0 out of Number of chunks, because people would think that it doesnt to anything
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "0 / " + prgbar.Maximum, DispatcherPriority.Background);

                //Starts the async task
                await Task.Run(() => Encode(maxConcurrency, nrPasses, allSettingsAom, resume, batchencodeoutputfile, audioOutput, streamLenghtVideo, fps, streamFrameRate, currentPath));
                //Set Maximum of Progressbar
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing files", DispatcherPriority.Background);
                await Task.Run(() => Concatenate.Concat(batchencodeoutputfile, audioOutput, currentPath));
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing completed! Elapsed Time: " + (DateTime.Now - starttimea).ToString(), DispatcherPriority.Background);
                if (batchEncoding == true)
                {
                    SmallScripts.DeleteTempFiles();
                    SmallScripts.DeleteTempFilesDir(tempFolder);
                }
                if (Cancel.CancelAll == false)
                {
                    File.Delete("unfinished_job.ini");
                }
            }
            if (shutdownafterencode == true)
            {
                if (Cancel.CancelAll == false)
                {
                    Process.Start("shutdown.exe", "/s /t 0");
                }
            }
        }

        public static class Cancel
        {
            //Public Cancel boolean
            public static bool CancelAll = false;
        }
        string currentPath;
        public void MainClass()
        {
            string videoInput = "";
            string videoOutput = "";
            if (logging == true)
            {
                SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " MainClass started", "log.log");
            }
            //Sets Label
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Starting...", DispatcherPriority.Background);

            //Sets the working directory
            if (tempFolderActive == false)
            {
                currentPath = Directory.GetCurrentDirectory();
            }
            else if (tempFolderActive == true)
            {
                currentPath = tempFolder;
            }

            //Checks if Chunks folder exist, if no it creates Chunks folder
            if (!Directory.Exists(Path.Combine(currentPath, "Chunks")))
                Directory.CreateDirectory(Path.Combine(currentPath, "Chunks"));

            //Sets the variable for input / output of video
            string customSettings = TextBoxCustomSettings.Text;
            string chunkLength = TextBoxChunkLength.Text;
            if (batchEncoding == false)
            {
                videoInput = TextBoxInputVideo.Text;
                videoOutput = TextBoxOutputVideo.Text;
            }
            else if (batchEncoding == true)
            {
                videoInput = batchencodefile;
                videoOutput = batchencodeoutputfile;
            }

            string streamLenghtVideo = streamLength;
            string encMode = ComboBoxEncMode.Text;
            string streamFrameRate = streamFps;
            string fps = TextBoxFramerate.Text;

            int maxConcurrency = Int16.Parse(TextBoxNumberWorkers.Text);
            int kfmaxdist = Int16.Parse(TextBoxKeyframeInterval.Text);
            int encThreads = Int16.Parse(TextBoxEncThreads.Text);
            int bitDepth = Int16.Parse(ComboBoxBitdepth.Text);
            int cpuUsed = Int16.Parse(ComboBoxCpuUsed.Text);
            int cqLevel = Int16.Parse(TextBoxcqLevel.Text);

            int tilecols = Int16.Parse(TextBoxTileCols.Text);
            int tilerows = Int16.Parse(TextBoxTileRows.Text);
            int nrPasses = Int16.Parse(ComboBoxPasses.Text);

            bool customSettingsbool = false;
            bool audioOutput = false;
            bool reencode = false;
            bool resume = false;

            if (logging == true)
            {
                SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Checked or Created Chunks folder", "log.log");
            }

            //Sets the boolean if audio should be included -> Concat() needs this value
            if (CheckBoxEnableAudio.IsChecked == true)
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Audio Boolean to true", "log.log");
                }
                audioOutput = true;
            }

            if (CheckBoxResume.IsChecked == true)
            {
                resume = true;
            }

            if (CheckBoxReencode.IsChecked == true)
            {
                reencode = true;
            }
            if (CheckBoxCustomSettings.IsChecked == true)
            {
                customSettingsbool = true;
            }
            SaveUnfinishedJob();
            Async(videoInput, currentPath, videoOutput, resume, logging, reencode, chunkLength, audioBitrate, audioCodec, maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, fps, encMode, customSettingsbool, customSettings, audioOutput, streamLenghtVideo, streamFrameRate);
        }

        private async void Async(string videoInput, string currentPath, string videoOutput, bool resume, bool logging, bool reencode, string chunkLength, string audioBitrate, string audioCodec, int maxConcurrency, int cpuUsed, int bitDepth, int encThreads, int cqLevel, int kfmaxdist, int tilecols, int tilerows, int nrPasses, string fps, string encMode, bool customSettingsbool, string customSettings, bool audioOutput, string streamLenghtVideo, string streamFrameRate)
        {
            await Task.Run(() => Splitting(videoInput, resume, logging, reencode, chunkLength, currentPath));
            //Audio Encoding
            if (CheckBoxEnableAudio.IsChecked == true && CheckBoxResume.IsChecked == false)
            {
                await Task.Run(() => AudioEncode.EncodeAudio(videoInput, logging, audioBitrate, audioCodec, currentPath));
            }
            if (CheckBoxResume.IsChecked == false)
            {
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Renaming Chunks...", DispatcherPriority.Background);
                await Task.Run(() => Rename.RenameChunks(currentPath));
            }
            await Task.Run(() => Encoding(currentPath, videoOutput, maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, resume, logging, fps, encMode, customSettingsbool, customSettings, audioOutput, streamLenghtVideo, streamFrameRate));
        }

        private void Splitting(string videoInput, bool resume, bool logging, bool reencode, string chunkLength, string outputPath)
        {
            //Start Splitting
            if (resume == false)
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Start Splitting", "log.log");
                }
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Started Splitting...", DispatcherPriority.Background);
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                //FFmpeg Arguments

                //Checks if Source needs to be reencoded
                if (reencode == false)
                {
                    if (logging == true)
                    {
                        SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Splitting without reencoding", "log.log");
                    }
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vcodec copy -f segment -segment_time " + chunkLength + " -an " + '\u0022' + outputPath + "\\Chunks\\out%0d.mkv" + '\u0022';
                }
                else if (reencode == true)
                {
                    if (logging == true)
                    {
                        SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Splitting with reencoding", "log.log");
                    }
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -c:v utvideo -f segment -segment_time " + chunkLength + " -an " + '\u0022' + outputPath + "\\Chunks\\out%0d.mkv" + '\u0022';
                }
                //Console.WriteLine(startInfo.Arguments);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                if (Cancel.CancelAll == false)
                {
                    SmallScripts.WriteToFileThreadSafe("True", outputPath + "\\splitted.log");
                }
            }
        }

        private void Encoding(string currentPath, string videoOutput, int maxConcurrency, int cpuUsed, int bitDepth, int encThreads, int cqLevel, int kfmaxdist, int tilecols, int tilerows, int nrPasses, bool resume, bool logging, string fps, string encMode, bool customSettingsbool, string customSettings, bool audioOutput, string streamLenghtVideo, string streamFrameRate)
        {
            //Create Array List with all Chunks
            string[] chunks;
            //Sets the Chunks directory
            string sdira = currentPath + "\\Chunks";
            //Add all Files in Chunks Folder to array
            chunks = Directory.GetFiles(sdira, "*mkv", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();
            //Parse Textbox Text to String for loop threading

            //Sets Resume Mode
            if (resume == true)
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Resume Mode Started", "log.log");
                }

                bool FileExist = File.Exists("encoded.txt");
                if (FileExist)
                {
                    foreach (string line in File.ReadLines("encoded.txt"))
                    {
                        //Removes all Items from Arraylist which are in encoded.txt
                        chunks = chunks.Where(s => s != line).ToArray();
                        if (logging == true)
                        {
                            SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Resume Mode - Deleting " + line + " from Array", "log.log");
                        }
                    }
                }
                else
                {

                    bool fileExist = File.Exists(currentPath + "\\splitted.log");

                    if (fileExist)
                    {
                        string[] lines = File.ReadAllLines(currentPath + "\\splitted.log");

                        if (lines[0] == "True")
                        {
                            if (MessageBox.Show("It appears that you toggled the resume mode, but there are no encoded chunks. Press Yes, to start Encoding of all Chunks. Press No, if you want to cancel!",
                                    "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Restarting...", DispatcherPriority.Background);
                                //This will be set if you Press Encode after a already finished encode
                            }
                            else
                            {
                                Cancel.CancelAll = true;
                            }

                        }
                        else
                        {
                            if (MessageBox.Show("It appears that you toggled the resume mode, but there are no encoded chunks and no information about a successfull splitting process. Press Yes, to start Encoding of all Chunks. Press No, if you want to cancel!",
                                    "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Restarting...", DispatcherPriority.Background);
                                //This will be set if you Press Encode after a already finished encode
                            }
                            else
                            {
                                Cancel.CancelAll = true;
                            }
                        }

                    }

                }

                //Set the Maximum Value of Progressbar
                prgbar.Dispatcher.Invoke(() => prgbar.Maximum = chunks.Count(), DispatcherPriority.Background);
            }

            string finalEncodeMode = "";

            //Sets the Encoding Mode
            if (encMode == "q")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to q", "log.log");
                }
                finalEncodeMode = " --end-usage=q --cq-level=" + cqLevel;
            }
            else if (encMode == "vbr")
            {
                //If vbr set finalEncodeMode
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to vbr", "log.log");
                }
                finalEncodeMode = " --end-usage=vbr --target-bitrate=" + cqLevel;
            }
            else if (encMode == "cbr")
            {
                //If cbr set finalEncodeMode
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to cbr", "log.log");
                }
                finalEncodeMode = " --end-usage=cbr --target-bitrate=" + cqLevel;
            }

            string allSettingsAom = "";
            //Sets aom settings to custom or preset
            if (customSettingsbool == true)
            {
                allSettingsAom = " " + customSettings;
                //Console.WriteLine(allSettingsAom);
            }
            else if (customSettingsbool == false)
            {
                allSettingsAom = " --cpu-used=" + cpuUsed + " --threads=" + encThreads + finalEncodeMode + " --bit-depth=" + bitDepth + " --tile-columns=" + tilecols + " --fps=" + fps + " --tile-rows=" + tilerows + " --kf-max-dist=" + kfmaxdist;
                //Console.WriteLine(allSettingsAom);
            }

            //Starts the async task
            if (Cancel.CancelAll != true)
            {
                StartTask(maxConcurrency, nrPasses, allSettingsAom, resume, videoOutput, audioOutput, logging, streamLenghtVideo, fps, streamFrameRate, currentPath);
            }
            //Set Maximum of Progressbar
            prgbar.Dispatcher.Invoke(() => prgbar.Maximum = chunks.Count(), DispatcherPriority.Background);
            //prgbar.Maximum = chunks.Count();
            //Set the Progresslabel to 0 out of Number of chunks, because people would think that it doesnt to anything
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "0 / " + prgbar.Maximum, DispatcherPriority.Background);
        }

        //Async Class -> UI doesnt freeze
        private async void StartTask(int maxConcurrency, int passes, string allSettingsAom, bool resume, string videoOutput, bool audioOutput, bool logging, string streamLenghtVideo, string fps, string streamFrameRate, string currentPath)
        {
            if (logging == true)
            {
                SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Async Task started", "log.log");
            }
            //Run encode class async
            await Task.Run(() => Encode(maxConcurrency, passes, allSettingsAom, resume, videoOutput, audioOutput, streamLenghtVideo, fps, streamFrameRate, currentPath));
        }

        //Main Encoding Class
        public void Encode(int maxConcurrency, int passes, string allSettingsAom, bool resume, string videoOutput, bool audioOutput, string streamLenghtVideo, string fps, string streamFrameRate, string currentPath)
        {
            //Set Working directory
            //string currentPath = Directory.GetCurrentDirectory();

            //Create Array List with all Chunks
            string[] chunks;
            //Sets the Chunks directory
            string sdira = currentPath + "\\Chunks";

            //Add all Files in Chunks Folder to array
            chunks = Directory.GetFiles(sdira, "*mkv", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();

            bool FileExist = File.Exists("encoded.txt");

            if (FileExist)
            {
                if (resume == true)
                {
                    //Removes all Items from Arraylist which are in encoded.txt
                    foreach (string line in File.ReadLines("encoded.txt"))
                    {
                        chunks = chunks.Where(s => s != line).ToArray();
                    }
                }
            }


            //Get Number of chunks for label of progressbar
            string labelstring = chunks.Count().ToString();

            //Starts the Timer for eta calculation
            DateTime starttime = DateTime.Now;
            starttimea = starttime;
            //Parallel Encoding - aka some blackmagic
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
            {
                List<Task> tasks = new List<Task>();
                foreach (var items in chunks)
                {
                    concurrencySemaphore.Wait();

                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (Cancel.CancelAll == false)
                            {
                                if (passes == 1)
                                {
                                    Process process = new Process();
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=1" + allSettingsAom + " --output=" + '\u0022' + currentPath + "\\Chunks\\" + items + "-av1.ivf" + '\u0022';
                                    process.StartInfo = startInfo;
                                    //Console.WriteLine(startInfo.Arguments);
                                    process.Start();
                                    process.WaitForExit();

                                    //Progressbar +1
                                    prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                    //Label of Progressbar = Progressbar
                                    TimeSpan timespent = DateTime.Now - starttime;
                                    //pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - eta: " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + " min left", DispatcherPriority.Background);
                                    pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - " + Math.Round(Convert.ToDecimal(((((Int16.Parse(streamLenghtVideo) * Int16.Parse(streamFrameRate)) / Int16.Parse(labelstring)) * prgbar.Value) / timespent.TotalSeconds)), 2).ToString() + "fps" + " - " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + "min left", DispatcherPriority.Background);

                                    if (Cancel.CancelAll == false)
                                    {
                                        //Write Item to file for later resume if something bad happens
                                        SmallScripts.WriteToFileThreadSafe(items, "encoded.txt");
                                    }
                                    else
                                    {
                                        SmallScripts.KillInstances();
                                    }
                                }
                                else if (passes == 2)
                                {
                                    Process process = new Process();
                                    ProcessStartInfo startInfo = new ProcessStartInfo();

                                    bool FileExistFirstPass = File.Exists(currentPath + "\\Chunks\\" + items + "_1pass_successfull.log");
                                    if (FileExistFirstPass != true)
                                    {

                                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=1 --fpf=" + '\u0022' + currentPath + "\\Chunks\\" + items + "_stats.log" + '\u0022' + allSettingsAom + " --output=NUL";
                                        process.StartInfo = startInfo;
                                        //Console.WriteLine(startInfo.Arguments);
                                        process.Start();
                                        process.WaitForExit();

                                        if (Cancel.CancelAll == false)
                                        {
                                            //Write Item to file for later resume if something bad happens
                                            SmallScripts.WriteToFileThreadSafe("", currentPath + "\\Chunks\\" + items + "_1pass_successfull.log");
                                        }
                                        else
                                        {
                                            SmallScripts.KillInstances();
                                        }
                                    }

                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=2 --fpf=" + '\u0022' + currentPath + "\\Chunks\\" + items + "_stats.log" + '\u0022' + allSettingsAom + " --output=" + '\u0022' + currentPath + "\\Chunks\\" + items + "-av1.ivf" + '\u0022';
                                    process.StartInfo = startInfo;
                                    //Console.WriteLine(startInfo.Arguments);
                                    process.Start();
                                    process.WaitForExit();

                                    prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                    TimeSpan timespent = DateTime.Now - starttime;
                                    pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - " + Math.Round(Convert.ToDecimal(((((Int16.Parse(streamLenghtVideo) * Int16.Parse(streamFrameRate)) / Int16.Parse(labelstring)) * prgbar.Value) / timespent.TotalSeconds)), 2).ToString() + "fps" + " - " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + "min left", DispatcherPriority.Background);
                                    //pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - eta: " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + " min left", DispatcherPriority.Background);
                                    //pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring, DispatcherPriority.Background);
                                    if (Cancel.CancelAll == false)
                                    {
                                        //Write Item to file for later resume if something bad happens
                                        SmallScripts.WriteToFileThreadSafe(items, "encoded.txt");
                                    }
                                    else
                                    {
                                        SmallScripts.KillInstances();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                        }
                    });

                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }

            //Mux all Encoded chunks back together
            if (batchEncoding == false && Cancel.CancelAll == false)
            {
                LocalConcat(videoOutput, audioOutput, currentPath);
            }
        }

        private async void LocalConcat(string videoOutput, bool audioOutput, string currentPath)
        {
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing files", DispatcherPriority.Background);
            await Task.Run(() => Concatenate.Concat(videoOutput, audioOutput, currentPath));
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing completed! Elapsed Time: " + (DateTime.Now - starttimea).ToString(), DispatcherPriority.Background);
            if (Cancel.CancelAll == false)
            {
                File.Delete("unfinished_job.ini");
            }
            if (deleteTempFiles == true)
            {
                SmallScripts.DeleteTempFiles();
                SmallScripts.DeleteTempFilesDir(tempFolder);
            }
        }

        //Kill all aomenc instances
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //We don't want accidental shutdowns when clickling on cancel ¯\_(ツ)_/¯
            if (shutdownafterencode == true)
            {
                shutdownafterencode = false;
            }
            Cancel.CancelAll = true;
            SmallScripts.KillInstances();
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Cancled!", DispatcherPriority.Background);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SmallScripts.DeleteTempFiles();
            SmallScripts.DeleteTempFilesDir(tempFolder);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        public static int numberOfAudioTracks;

        public void GetStreamFps(string fileinput)
        {
            string input = "";

            input = '\u0022' + fileinput + '\u0022';

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C ffprobe.exe -i " + input + " -v 0 -of csv=p=0 -select_streams v:0 -show_entries stream=r_frame_rate",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.Start();
            string fpsOutput = process.StandardOutput.ReadLine();
            TextBoxFramerate.Text = fpsOutput;
            string value = new DataTable().Compute(TextBoxFramerate.Text, null).ToString();
            streamFps = Convert.ToInt64(Math.Round(Convert.ToDouble(value))).ToString();
            process.WaitForExit();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //Shows the Settings Window
            Settings settings = new Settings();
            settings.Show();
        }

        public static void SaveSettings(bool Settingslogging, bool shutdown, bool batch, bool loadsettings, bool delete, bool tempFolderChunks, bool aomencDir, bool ffmpegDir, bool ffprobeDir, string temp, string aomenc, string ffmpeg, string ffprobe)
        {
            //Gets the Settings from the Settings Window and sets them in MainWindow
            logging = Settingslogging;
            shutdownafterencode = shutdown;
            batchEncoding = batch;
            enableCustomSettings = loadsettings;
            deleteTempFiles = delete;
            tempFolder = temp;
            tempFolderActive = tempFolderChunks;

        }

        public void SetBatch()
        {
            Input.Content = "Open Folder";
        }

        private void ComboBoxEncMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Sets the Quality label accordingly the selected encode mode
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "vbr" || text == "cbr")
            {
                LabelQ.Visibility = Visibility.Collapsed;
                LabelVbr.Visibility = Visibility.Visible;
            }
            if (text == "q")
            {
                if (LabelVbr == null)
                {
                    //Null point exception if not "(LabelVbr == null)"
                }
                else
                {
                    LabelVbr.Visibility = Visibility.Collapsed;
                    LabelQ.Visibility = Visibility.Visible;
                }
            }
        }

        private void ButtonAudioSettings_Click(object sender, RoutedEventArgs e)
        {
            AudioSettings audiosettings = new AudioSettings();
            audiosettings.Show();
        }

        public static void SaveAudioSettings(string AudioCodec, string AudioBitrate)
        {
            audioCodec = AudioCodec;
            audioBitrate = AudioBitrate;
        }

        public void SaveUnfinishedJob()
        {
            //Saves all Current Settings to a file
            string audioCheckBox = CheckBoxEnableAudio.IsChecked.ToString();
            string customSettingsBool = enableCustomSettings.ToString();
            string customSettings = TextBoxCustomSettings.Text;
            string maxConcurrency = TextBoxNumberWorkers.Text;
            string kfmaxdist = TextBoxKeyframeInterval.Text;
            string chunkLength = TextBoxChunkLength.Text;
            string audioSettingsBitrate = audioBitrate;
            string encThreads = TextBoxEncThreads.Text;
            string bitDepth = ComboBoxBitdepth.Text;
            string tilecols = TextBoxTileCols.Text;
            string tilerows = TextBoxTileRows.Text;
            string audioSettingsCodec = audioCodec;
            string encMode = ComboBoxEncMode.Text;
            string cpuUsed = ComboBoxCpuUsed.Text;
            string nrPasses = ComboBoxPasses.Text;
            string cqLevel = TextBoxcqLevel.Text;
            string fps = TextBoxFramerate.Text;
            string videoInput = TextBoxInputVideo.Text;
            string videoOutput = TextBoxOutputVideo.Text;

            //Saves custom settings in settings_custom.ini
            if (CheckBoxCustomSettings.IsChecked == true)
            {
                string[] linescustom = { customSettings };
                File.WriteAllLines("unfinished_job_settings_custom.ini", linescustom);
            }

            string[] lines = { maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, fps, encMode, chunkLength, audioSettingsCodec, audioSettingsBitrate, customSettingsBool, audioCheckBox, videoInput, videoOutput };
            File.WriteAllLines("unfinished_job.ini", lines);
        }

        public void LoadUnfinishedJob()
        {
            try
            {
                //If unifnished_job.ini exist -> Set all Values
                bool fileExist = File.Exists("unfinished_job.ini");

                if (fileExist)
                {
                    if (MessageBox.Show("Unfinished Job detected. Load unfinished job? Press No to delete the unfinished Job files.",
                        "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string[] lines = File.ReadAllLines("unfinished_job.ini");

                        TextBoxNumberWorkers.Text = lines[0];
                        ComboBoxCpuUsed.Text = lines[1];
                        ComboBoxBitdepth.Text = lines[2];
                        TextBoxEncThreads.Text = lines[3];
                        TextBoxcqLevel.Text = lines[4];
                        TextBoxKeyframeInterval.Text = lines[5];
                        TextBoxTileCols.Text = lines[6];
                        TextBoxTileRows.Text = lines[7];
                        ComboBoxPasses.Text = lines[8];
                        TextBoxFramerate.Text = lines[9];
                        ComboBoxEncMode.Text = lines[10];
                        TextBoxChunkLength.Text = lines[11];
                        audioCodec = lines[12];
                        audioBitrate = lines[13];
                        if (lines[14] == "True")
                        {
                            enableCustomSettings = true;
                            CheckBoxCustomSettings.IsChecked = true;
                        }
                        if (lines[15] == "True")
                        {
                            CheckBoxEnableAudio.IsChecked = true;
                        }
                        TextBoxInputVideo.Text = lines[16];
                        TextBoxOutputVideo.Text = lines[17];
                        CheckBoxResume.IsChecked = true;
                        SmallScripts.GetStreamLength(TextBoxInputVideo.Text);
                        GetStreamFps(TextBoxInputVideo.Text);
                        //Reads custom settings to settings_custom.ini
                        bool customFileExist = File.Exists("unfinished_job_settings_custom.ini");
                        if (customFileExist)
                        {
                            string[] linesa = File.ReadAllLines("unfinished_job_settings_custom.ini");
                            TextBoxCustomSettings.Text = linesa[0];
                        }
                    }
                    else
                    {
                        File.Delete("unfinished_job.ini");
                        SmallScripts.DeleteTempFiles();
                        SmallScripts.DeleteTempFilesDir(tempFolder);
                    }
                }
            }
            catch { }
        }

        public static void SetNumberOfAudioTracks(int count)
        {
            numberOfAudioTracks = count;
        }

        public static void SetStreamLength(string length)
        {
            streamLength = length;
        }
    }
}
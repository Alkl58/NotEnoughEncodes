using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Data;

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
            CheckDependencies();
            //Load Settings
            ReadSettings();
            LoadUnfinishedJob();
        }

        private void CheckDependencies()
        {
            bool aomencExist = File.Exists("aomenc.exe");
            bool ffmpegExist = File.Exists("ffmpeg.exe");
            bool ffprobeExist = File.Exists("ffprobe.exe");
            if (aomencExist == false || ffmpegExist == false || ffprobeExist == false)
            {
                MessageBox.Show("Couldn't find all depedencies: \n aomenc found: " + aomencExist + "\n ffmpeg found: " + ffmpegExist + " \n ffprobe found: " + ffprobeExist);
            }
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

            //Saves custom settings in settings_custom.ini
            if (CheckBoxCustomSettings.IsChecked == true)
            {
                string[] linescustom = { customSettings };
                File.WriteAllLines("settings_custom.ini", linescustom);
            }

            string[] lines = { maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, fps, encMode, chunkLength, audioSettingsCodec, audioSettingsBitrate, customSettingsBool, audioCheckBox };
            File.WriteAllLines("settings.ini", lines);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (batchEncoding == false || batchEncoding == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                    TextBoxInputVideo.Text = openFileDialog.FileName;
                GetStreamFps(TextBoxInputVideo.Text);
                GetStreamLength(TextBoxInputVideo.Text);
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
        public string streamLength;
        public string streamFps;
        private void GetStreamLength(string fileinput)
        {
            string input;

            input = '\u0022' + fileinput + '\u0022';

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C ffprobe.exe -i " + input + " -show_entries format=duration -v quiet -of csv=" + '\u0022'+ "p=0" + '\u0022',
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.Start();
            string streamlength = process.StandardOutput.ReadLine();
            //TextBoxFramerate.Text = fpsOutput;
            //Console.WriteLine(streamlength);
            string value = new DataTable().Compute(streamlength, null).ToString();
            streamLength = Convert.ToInt64(Math.Round(Convert.ToDouble(value))).ToString();
            //streamLength = streamlength;
            Console.WriteLine(streamLength);
            process.WaitForExit();


        }

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            if (batchEncoding == false || batchEncoding == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Matroska|*.mkv";
                if (saveFileDialog.ShowDialog() == true)
                    TextBoxOutputVideo.Text = saveFileDialog.FileName;
            }else if (batchEncoding == true)
            {
                System.Windows.Forms.FolderBrowserDialog browse = new System.Windows.Forms.FolderBrowserDialog();
                if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxOutputVideo.Text = browse.SelectedPath;
                }
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            bool folderExist = Directory.Exists("Chunks");

            if (folderExist && Cancel.CancelAll == false && CheckBoxResume.IsChecked == false)
            {
                if (MessageBox.Show("It appears that you finished a previous encode but forgot to delete the temp files. To let the program delete the files, press Yes. Press No if pressing on Encode was a mistake.",
                        "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DeleteTempFiles();
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
                WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Clicked on Start Encode", "log.log");
            }

            if (TextBoxInputVideo.Text == " Input Video")
            {
                MessageBox.Show("No Input File selected!");

                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " No Input File selected!", "log.log");
                }
            }
            else if (TextBoxOutputVideo.Text == " Output Video")
            {
                MessageBox.Show("No Output Path specified!");

                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " No Output Path specified!", "log.log");
                }
            }
            else if (TextBoxInputVideo.Text != " Input Video")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Started MainClass()", "log.log");
                }
                if (Cancel.CancelAll == false)
                {
                    //Start MainClass
                    if (batchEncoding == false)
                    {
                        MainClass();
                    }else if (batchEncoding == true)
                    {
                        batchEncode();
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
        public async void batchEncode()
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
                string currentPath = Directory.GetCurrentDirectory();
                //Checks if Chunks folder exist, if no it creates Chunks folder
                if (!Directory.Exists(Path.Combine(currentPath, "Chunks")))
                    Directory.CreateDirectory(Path.Combine(currentPath, "Chunks"));
                GetStreamFps(batchencodefile);
                GetStreamLength(batchencodefile);

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

                await Task.Run(() => Splitting(batchencodefile, resume, logging, reencode, chunkLength));
                //Audio Encoding
                if (CheckBoxEnableAudio.IsChecked == true && CheckBoxResume.IsChecked == false)
                {
                    await Task.Run(() => EncodeAudio(batchencodefile, logging, audioBitrate, audioCodec, currentPath));
                }
                if (CheckBoxResume.IsChecked == false)
                {
                    await Task.Run(() => Rename(currentPath));
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
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to q", "log.log");
                    }
                    finalEncodeMode = " --end-usage=q --cq-level=" + cqLevel;
                }
                else if (encMode == "vbr")
                {
                    //If vbr set finalEncodeMode
                    if (logging == true)
                    {
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to vbr", "log.log");
                    }
                    finalEncodeMode = " --end-usage=vbr --target-bitrate=" + cqLevel;
                }
                else if (encMode == "cbr")
                {
                    //If cbr set finalEncodeMode
                    if (logging == true)
                    {
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to cbr", "log.log");
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
                await Task.Run(() => Encode(maxConcurrency, nrPasses, allSettingsAom, resume, batchencodeoutputfile, audioOutput, streamLenghtVideo, fps, streamFrameRate));
                //Set Maximum of Progressbar

                await Task.Run(() => Concat(batchencodeoutputfile, audioOutput, starttimea));

            }
        }

        public static class Cancel
        {
            //Public Cancel boolean
            public static bool CancelAll = false;
        }

        public void MainClass()
        {
            string videoInput ="";
            string videoOutput = "";
            if (logging == true)
            {
                WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " MainClass started", "log.log");
            }
            //Sets Label
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Starting...", DispatcherPriority.Background);

            //Sets the working directory
            string currentPath = Directory.GetCurrentDirectory();
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
                WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Checked or Created Chunks folder", "log.log");
            }

            //Sets the boolean if audio should be included -> Concat() needs this value
            if (CheckBoxEnableAudio.IsChecked == true)
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Audio Boolean to true", "log.log");
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
            await Task.Run(() => Splitting(videoInput, resume, logging, reencode, chunkLength));
            //Audio Encoding
            if (CheckBoxEnableAudio.IsChecked == true && CheckBoxResume.IsChecked == false)
            {
                await Task.Run(() => EncodeAudio(videoInput, logging, audioBitrate, audioCodec, currentPath));
            }
            if (CheckBoxResume.IsChecked == false)
            {
                await Task.Run(() => Rename(currentPath));
            }
            await Task.Run(() => Encoding(currentPath, videoOutput, maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, resume, logging, fps, encMode, customSettingsbool, customSettings, audioOutput, streamLenghtVideo, streamFrameRate));
        }

        private void Splitting(string videoInput, bool resume, bool logging, bool reencode, string chunkLength)
        {
            //Start Splitting
            if (resume == false)
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Start Splitting", "log.log");
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
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Splitting without reencoding", "log.log");
                    }
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vcodec copy -f segment -segment_time " + chunkLength + " -an " + '\u0022' + "Chunks\\out%0d.mkv" + '\u0022';
                }
                else if (reencode == true)
                {
                    if (logging == true)
                    {
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Splitting with reencoding", "log.log");
                    }
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -c:v utvideo -f segment -segment_time " + chunkLength + " -an " + '\u0022' + "Chunks\\out%0d.mkv" + '\u0022';
                }
                //Console.WriteLine(startInfo.Arguments);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        private void Rename(string currentPath)
        {
            //Create Array List with all Chunks
            string[] chunks;
            //Sets the Chunks directory
            string sdira = currentPath + "\\Chunks";
            //Add all Files in Chunks Folder to array
            chunks = Directory.GetFiles(sdira, "*mkv", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();
            DirectoryInfo d = new DirectoryInfo(currentPath + "\\Chunks");
            FileInfo[] infos = d.GetFiles();

            int numberOfChunks = chunks.Count();

            //outx.mkv = 8 | outxx.mkv = 9 (99) | outxxx.mkv = 10 (999) | outxxxx.mkv = 11 (9999) | outxxxxx.mkv = 12 (99999)

            //int numberOfChunks = 20000;

            if (numberOfChunks >= 10 && numberOfChunks <= 99)
            {
                foreach (FileInfo f in infos)
                {
                    int count = f.ToString().Count();

                    if (count == 8)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0"));
                    }
                }
            }
            else if (numberOfChunks >= 100 && numberOfChunks <= 999) //If you have more than 100 Chunks and less than 999
            {
                foreach (FileInfo f in infos)
                {
                    int count = f.ToString().Count();

                    if (count == 8)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out00"));
                    }

                    if (count == 9)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0"));
                    }
                }
            }
            else if (numberOfChunks >= 1000 && numberOfChunks <= 9999) //If you have more than 1.000 Chunks and less than 9.999
            {
                foreach (FileInfo f in infos)
                {
                    int count = f.ToString().Count();

                    if (count == 8)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out000"));
                    }

                    if (count == 9)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out00"));
                    }

                    if (count == 10)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0"));
                    }
                }
            }
            else if (numberOfChunks >= 10000 && numberOfChunks <= 99999) //If you have more than 10.000 Chunks and less than 99.999
            {
                foreach (FileInfo f in infos)
                {
                    int count = f.ToString().Count();

                    if (count == 8)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0000"));
                    }

                    if (count == 9)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out000"));
                    }

                    if (count == 10)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out00"));
                    }

                    if (count == 11)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0"));
                    }
                }
            }
            else if (numberOfChunks >= 100000 && numberOfChunks <= 999999)
            {
                foreach (FileInfo f in infos)
                {
                    int count = f.ToString().Count();
                    //If you have more than 100.000 Chunks and less than 999.999
                    //BTW are fu*** insane?
                    if (count == 8)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out00000"));
                    }

                    if (count == 9)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0000"));
                    }

                    if (count == 10)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out000"));
                    }

                    if (count == 11)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out00"));
                    }
                    if (count == 12)
                    {
                        File.Move(d + "\\" + f, d + "\\" + f.Name.Replace("out", "out0"));
                    }
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
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Resume Mode Started", "log.log");
                }

                foreach (string line in File.ReadLines("encoded.txt"))
                {
                    //Removes all Items from Arraylist which are in encoded.txt
                    chunks = chunks.Where(s => s != line).ToArray();
                    if (logging == true)
                    {
                        WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Resume Mode - Deleting " + line + " from Array", "log.log");
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
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to q", "log.log");
                }
                finalEncodeMode = " --end-usage=q --cq-level=" + cqLevel;
            }
            else if (encMode == "vbr")
            {
                //If vbr set finalEncodeMode
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to vbr", "log.log");
                }
                finalEncodeMode = " --end-usage=vbr --target-bitrate=" + cqLevel;
            }
            else if (encMode == "cbr")
            {
                //If cbr set finalEncodeMode
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Set Encode Mode to cbr", "log.log");
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
            StartTask(maxConcurrency, nrPasses, allSettingsAom, resume, videoOutput, audioOutput, logging, streamLenghtVideo, fps, streamFrameRate);
            //Set Maximum of Progressbar
            prgbar.Dispatcher.Invoke(() => prgbar.Maximum = chunks.Count(), DispatcherPriority.Background);
            //prgbar.Maximum = chunks.Count();
            //Set the Progresslabel to 0 out of Number of chunks, because people would think that it doesnt to anything
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "0 / " + prgbar.Maximum, DispatcherPriority.Background);
        }

        //Async Class -> UI doesnt freeze
        private async void StartTask(int maxConcurrency, int passes, string allSettingsAom, bool resume, string videoOutput, bool audioOutput, bool logging, string streamLenghtVideo, string fps, string streamFrameRate)
        {
            if (logging == true)
            {
                WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Async Task started", "log.log");
            }
            //Run encode class async
            await Task.Run(() => Encode(maxConcurrency, passes, allSettingsAom, resume, videoOutput, audioOutput, streamLenghtVideo, fps, streamFrameRate));
        }

        //Main Encoding Class
        public void Encode(int maxConcurrency, int passes, string allSettingsAom, bool resume, string videoOutput, bool audioOutput, string streamLenghtVideo, string fps, string streamFrameRate)
        {
            //Set Working directory
            string currentPath = Directory.GetCurrentDirectory();

            //Create Array List with all Chunks
            string[] chunks;
            //Sets the Chunks directory
            string sdira = currentPath + "\\Chunks";

            //Add all Files in Chunks Folder to array
            chunks = Directory.GetFiles(sdira, "*mkv", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();

            if (resume == true)
            {
                //Removes all Items from Arraylist which are in encoded.txt
                foreach (string line in File.ReadLines("encoded.txt"))
                {
                    chunks = chunks.Where(s => s != line).ToArray();
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
                                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=1" + allSettingsAom + " --output=Chunks\\" + items + "-av1.ivf";
                                    process.StartInfo = startInfo;
                                    //Console.WriteLine(startInfo.Arguments);
                                    process.Start();
                                    process.WaitForExit();

                                    //Progressbar +1
                                    prgbar.Dispatcher.Invoke(() => prgbar.Value += 1, DispatcherPriority.Background);
                                    //Label of Progressbar = Progressbar
                                    TimeSpan timespent = DateTime.Now - starttime;
                                    //pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - eta: " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + " min left", DispatcherPriority.Background);
                                    pLabel.Dispatcher.Invoke(() => pLabel.Content = prgbar.Value + " / " + labelstring + " - " + Math.Round(Convert.ToDecimal(((((Int16.Parse(streamLenghtVideo) * Int16.Parse(streamFrameRate)) / Int16.Parse(labelstring)) * prgbar.Value) / timespent.TotalSeconds)), 2).ToString() + "fps"+ " - " + Math.Round((((timespent.TotalSeconds / prgbar.Value) * (Int16.Parse(labelstring) - prgbar.Value)) / 60), MidpointRounding.ToEven) + "min left", DispatcherPriority.Background);

                                    if (Cancel.CancelAll == false)
                                    {
                                        //Write Item to file for later resume if something bad happens
                                        WriteToFileThreadSafe(items, "encoded.txt");
                                    }
                                    else
                                    {
                                        KillInstances();
                                    }
                                }
                                else if (passes == 2)
                                {
                                    Process process = new Process();
                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=1 --fpf=Chunks\\" + items + "_stats.log" + allSettingsAom + " --output=NUL";
                                    process.StartInfo = startInfo;
                                    //Console.WriteLine(startInfo.Arguments);
                                    process.Start();
                                    process.WaitForExit();

                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + items + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=2 --fpf=Chunks\\" + items + "_stats.log" + allSettingsAom + " --output=Chunks\\" + items + "-av1.ivf";
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
                                        WriteToFileThreadSafe(items, "encoded.txt");
                                    }
                                    else
                                    {
                                        KillInstances();
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
            if (batchEncoding == false)
            {
                Concat(videoOutput, audioOutput, starttime);
            }
            
        }

        //Mux ivf Files back together
        private void Concat(string videoOutput, bool audioOutput, DateTime starttime)
        {
            if (Cancel.CancelAll == false)
            {
                string currentPath = Directory.GetCurrentDirectory();

                string outputfilename = videoOutput;

                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing files", DispatcherPriority.Background);

                //Lists all ivf files in mylist.txt
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                //FFmpeg Arguments
                startInfo.Arguments = "/C (for %i in (Chunks\\*.ivf) do @echo file '%i') > Chunks\\mylist.txt";
                //Console.WriteLine(startInfo.Arguments);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                if (audioOutput == false)
                {
                    //Concat the Videos
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    //FFmpeg Arguments
                    startInfo.Arguments = "/C ffmpeg.exe -f concat -safe 0 -i Chunks\\mylist.txt -c copy " + '\u0022' + outputfilename + '\u0022';
                    //Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                else if (audioOutput == true)
                {
                    //Concat the Videos
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    //FFmpeg Arguments
                    startInfo.Arguments = "/C ffmpeg.exe -f concat -safe 0 -i Chunks\\mylist.txt -c copy no_audio.mkv";
                    //Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();

                    //Concat the Videos
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    //FFmpeg Arguments
                    //Sets the mapping of the video and audiostreams and muxes them
                    if (numberOfAudioTracks == 1)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv  -map 0:v -map 1:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (numberOfAudioTracks == 2)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -map 0:v -map 1:a -map 2:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (numberOfAudioTracks == 3)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -i AudioEncoded\\audio2.mkv -map 0:v -map 1:a -map 2:a -map 3:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (numberOfAudioTracks == 4)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -i AudioEncoded\\audio2.mkv -i AudioEncoded\\audio3.mkv -map 0:v -map 1:a -map 2:a -map 3:a -map 4:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    //startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audioOutput.mkv -map 0:0 -map 0:1 -map 0:2 -c copy " + '\u0022' + outputfilename + '\u0022';
                    //Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                pLabel.Dispatcher.Invoke(() => pLabel.Content = "Muxing completed! Elapsed Time: " + (DateTime.Now - starttime).ToString(), DispatcherPriority.Background);
                if (Cancel.CancelAll == false)
                {
                    File.Delete("unifnished_job.ini");
                }
                if (batchEncoding == true)
                {
                    DeleteTempFiles();
                    batchFilefinished = true;
                }
                if (shutdownafterencode == true)
                {
                    if (Cancel.CancelAll == false)
                    {
                        //Shutdowns the PC if specified in settings
                        Process.Start("shutdown.exe", "/s /t 0");
                    }
                }
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
            KillInstances();
            pLabel.Dispatcher.Invoke(() => pLabel.Content = "Cancled!", DispatcherPriority.Background);
        }

        public void KillInstances()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("aomenc"))
                {
                    process.Kill();
                }
                foreach (var process in Process.GetProcessesByName("ffmpeg"))
                {
                    process.Kill();
                }
            }
            catch { }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DeleteTempFiles();
        }

        private void DeleteTempFiles()
        {
            try
            {
                //Delete Files, because of lazy dump****
                File.Delete("encoded.txt");
                Directory.Delete("Chunks", true);
                Directory.Delete("AudioExtracted", true);
                Directory.Delete("AudioEncoded", true);
                File.Delete("no_audio.mkv");
            }
            catch { }
        }

        //Some smaller Blackmagic, so parallel Workers won't lockdown the encoded.txt file
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void WriteToFileThreadSafe(string text, string path)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Disables editing of preset settings if user wants to write custom settings
            TextBoxCustomSettings.IsEnabled = true;
            ComboBoxEncMode.IsEnabled = false;
            TextBoxcqLevel.IsEnabled = false;
            TextBoxEncThreads.IsEnabled = false;
            ComboBoxBitdepth.IsEnabled = false;
            ComboBoxCpuUsed.IsEnabled = false;
            TextBoxTileCols.IsEnabled = false;
            TextBoxTileRows.IsEnabled = false;
            TextBoxKeyframeInterval.IsEnabled = false;
            TextBoxFramerate.IsEnabled = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Re-enables editing of preset settings
            TextBoxCustomSettings.IsEnabled = false;
            ComboBoxEncMode.IsEnabled = true;
            TextBoxcqLevel.IsEnabled = true;
            TextBoxEncThreads.IsEnabled = true;
            ComboBoxBitdepth.IsEnabled = true;
            ComboBoxCpuUsed.IsEnabled = true;
            TextBoxTileCols.IsEnabled = true;
            TextBoxTileRows.IsEnabled = true;
            TextBoxKeyframeInterval.IsEnabled = true;
            TextBoxFramerate.IsEnabled = true;
        }

        public int numberOfAudioTracks;

        public void EncodeAudio(string videoInput, bool logging, string audioBitrate, string audioCodec, string currentPath)
        {
            if (logging == true)
            {
                WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding started", "log.log");
            }

            string allAudioSettings = "";
            //Sets Settings for Audio Encoding
            if (audioCodec == "Copy Audio")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to Audio Copy", "log.log");
                }
                allAudioSettings = " -c:a copy";
            }
            else if (audioCodec == "Opus")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to libopus", "log.log");
                }
                allAudioSettings = " -c:a libopus -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "Opus 5.1")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to libopus 5.1", "log.log");
                }
                allAudioSettings = " -c:a libopus -b:a " + audioBitrate + "k -af channelmap=channel_layout=5.1";
            }
            else if (audioCodec == "AAC CBR")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to AAC CBR", "log.log");
                }
                allAudioSettings = " -c:a aac -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "AC3")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to AC3 CBR", "log.log");
                }
                allAudioSettings = " -c:a ac3 -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "FLAC")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to FLAC", "log.log");
                }
                allAudioSettings = " -c:a flac ";
            }
            else if (audioCodec == "MP3 CBR")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to MP3 CBR", "log.log");
                }
                allAudioSettings = " -c:a libmp3lame -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "MP3 VBR")
            {
                if (logging == true)
                {
                    WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to MP3 CBR", "log.log");
                }
                if (Int16.Parse(audioBitrate) >= 10)
                {
                    MessageBox.Show("Audio VBR Range is from 0-9");
                }
                else if (Int16.Parse(audioBitrate) <= 10)
                {
                    allAudioSettings = " -c:a libmp3lame -q:a " + audioBitrate + " ";
                }
            }

            //Creates Audio Folder
            if (!Directory.Exists(Path.Combine(currentPath, "AudioExtracted")))
                Directory.CreateDirectory(Path.Combine(currentPath, "AudioExtracted"));
            if (!Directory.Exists(Path.Combine(currentPath, "AudioEncoded")))
                Directory.CreateDirectory(Path.Combine(currentPath, "AudioEncoded"));

            Process process = new Process();
            //Starts extracting maximal 4 Audio Streams
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 AudioExtracted\\audio0.mkv & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 AudioExtracted\\audio1.mkv & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 AudioExtracted\\audio2.mkv & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 AudioExtracted\\audio3.mkv";
            Console.WriteLine(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            DirectoryInfo AudioExtracted = new DirectoryInfo("AudioExtracted");
            //Loops through all mkv files in AudioExtracted
            foreach (var file in AudioExtracted.GetFiles("*.mkv"))
            {
                //Directory.Move(file.FullName, filepath + "\\TextFiles\\" + file.Name);
                //Get the Filesize, because the command above also creates mkv files even if there is not audiostream (filesize = 0)
                long length = new FileInfo("AudioExtracted\\" + file).Length;
                //Console.WriteLine(length);
                //If Filesize = 0 -> delete file
                if (length == 0)
                {
                    File.Delete("AudioExtracted\\" + file);
                }
                else if (length > 1)
                {
                    //Encodes the Audio to the given format
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + "AudioExtracted\\" + file + '\u0022' + " " + allAudioSettings + "-vn " + '\u0022' + "AudioEncoded\\" + file + '\u0022';
                    Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            //Counts the number of AudioFiles
            int audioCount = Directory.GetFiles("AudioEncoded", "*mkv", SearchOption.TopDirectoryOnly).Length;
            //Sets the number of AudioTracks of the concat process
            numberOfAudioTracks = audioCount;
        }

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

        public static void SaveSettings(bool Settingslogging, bool shutdown, bool batch, bool loadsettings)
        {
            //Gets the Settings from the Settings Window and sets them in MainWindow
            logging = Settingslogging;
            shutdownafterencode = shutdown;
            batchEncoding = batch;
            enableCustomSettings = loadsettings;
            

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
                File.WriteAllLines("unifnished_job_settings_custom.ini", linescustom);
            }

            string[] lines = { maxConcurrency, cpuUsed, bitDepth, encThreads, cqLevel, kfmaxdist, tilecols, tilerows, nrPasses, fps, encMode, chunkLength, audioSettingsCodec, audioSettingsBitrate, customSettingsBool, audioCheckBox, videoInput, videoOutput };
            File.WriteAllLines("unifnished_job.ini", lines);
        }

        public void LoadUnfinishedJob()
        {
            try
            {
                //If unifnished_job.ini exist -> Set all Values
                bool fileExist = File.Exists("unifnished_job.ini");

                if (fileExist)
                {

                    if (MessageBox.Show("Unfinished Job detected. Load unfinished job? Press No to delete the unfinished Job files.",
                        "Resume", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string[] lines = File.ReadAllLines("unifnished_job.ini");

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
                        GetStreamLength(TextBoxInputVideo.Text);
                        GetStreamFps(TextBoxInputVideo.Text);
                        //Reads custom settings to settings_custom.ini
                        bool customFileExist = File.Exists("unifnished_job_settings_custom.ini");
                        if (customFileExist)
                        {
                            string[] linesa = File.ReadAllLines("unifnished_job_settings_custom.ini");
                            TextBoxCustomSettings.Text = linesa[0];
                        }
                    }
                    else
                    {
                        File.Delete("unifnished_job.ini");
                        DeleteTempFiles();
                    }

                }
                    
            }
            catch { }

        }
    }
}
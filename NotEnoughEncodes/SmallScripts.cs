using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace NotEnoughEncodes
{
    class SmallScripts
    {
        //Kills all aomenc and ffmpeg instances
        public static void KillInstances()
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

        //Deletes all Temp Files
        public static void DeleteTempFiles()
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
        

        public static void WriteToFileThreadSafe(string text, string path)
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
        //Checks the dependencies (ffmpeg, aomenc, ffprobe)
        public static void CheckDependencies()
        {
            bool aomencExist = File.Exists("aomenc.exe");
            bool ffmpegExist = File.Exists("ffmpeg.exe");
            bool ffprobeExist = File.Exists("ffprobe.exe");
            if (aomencExist == false || ffmpegExist == false || ffprobeExist == false)
            {
                MessageBox.Show("Couldn't find all depedencies: \n aomenc found: " + aomencExist + "\n ffmpeg found: " + ffmpegExist + " \n ffprobe found: " + ffprobeExist);
            }
        }
        private static string streamLength;
        public static void GetStreamLength(string fileinput)
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
                Arguments = "/C ffprobe.exe -i " + input + " -show_entries format=duration -v quiet -of csv=" + '\u0022' + "p=0" + '\u0022',
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
            //Console.WriteLine(streamLength);
            MainWindow.SetStreamLength(streamLength);
            process.WaitForExit();
        }


    }
}

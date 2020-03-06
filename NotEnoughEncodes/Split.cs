using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NotEnoughEncodes
{
    class Split
    {
        public static void Splitting(string videoInput, bool resume, bool logging, bool reencode, string chunkLength, string outputPath, string ffmpegPath)
        {
            //Start Splitting
            if (resume == false)
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Start Splitting", "log.log");
                }
                //pLabel.Dispatcher.Invoke(() => pLabel.Content = "Started Splitting...", DispatcherPriority.Background);
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.WorkingDirectory = ffmpegPath;
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
                    Console.WriteLine(startInfo.Arguments);
                }
                //Console.WriteLine(startInfo.Arguments);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                if (MainWindow.Cancel.CancelAll == false)
                {
                    SmallScripts.WriteToFileThreadSafe("True", outputPath + "\\splitted.log");
                }
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;

namespace NotEnoughEncodes
{
    //Muxing all Files back together
    internal class Concatenate
    {
        public static void Concat(string videoOutput, bool audioOutput, DateTime starttime)
        {
            if (MainWindow.Cancel.CancelAll == false)
            {
                string currentPath = Directory.GetCurrentDirectory();

                string outputfilename = videoOutput;

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
                    if (MainWindow.numberOfAudioTracks == 1)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv  -map 0:v -map 1:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (MainWindow.numberOfAudioTracks == 2)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -map 0:v -map 1:a -map 2:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (MainWindow.numberOfAudioTracks == 3)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -i AudioEncoded\\audio2.mkv -map 0:v -map 1:a -map 2:a -map 3:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    else if (MainWindow.numberOfAudioTracks == 4)
                    {
                        startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audio0.mkv -i AudioEncoded\\audio1.mkv -i AudioEncoded\\audio2.mkv -i AudioEncoded\\audio3.mkv -map 0:v -map 1:a -map 2:a -map 3:a -map 4:a -c copy " + '\u0022' + outputfilename + '\u0022';
                    }
                    //startInfo.Arguments = "/C ffmpeg.exe -i no_audio.mkv -i AudioEncoded\\audioOutput.mkv -map 0:0 -map 0:1 -map 0:2 -c copy " + '\u0022' + outputfilename + '\u0022';
                    //Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }

                if (MainWindow.shutdownafterencode == true && MainWindow.batchEncoding == false)
                {
                    if (MainWindow.Cancel.CancelAll == false)
                    {
                        //Shutdowns the PC if specified in settings
                        Process.Start("shutdown.exe", "/s /t 0");
                    }
                }
            }
        }
    }
}
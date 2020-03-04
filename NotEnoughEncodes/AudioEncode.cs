using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace NotEnoughEncodes
{
    class AudioEncode
    {
        public static void EncodeAudio(string videoInput, bool logging, string audioBitrate, string audioCodec, string currentPath)
        {
            if (logging == true)
            {
                SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding started", "log.log");
            }

            string allAudioSettings = "";
            //Sets Settings for Audio Encoding
            if (audioCodec == "Copy Audio")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to Audio Copy", "log.log");
                }
                allAudioSettings = " -c:a copy";
            }
            else if (audioCodec == "Opus")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to libopus", "log.log");
                }
                allAudioSettings = " -c:a libopus -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "Opus 5.1")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to libopus 5.1", "log.log");
                }
                allAudioSettings = " -c:a libopus -b:a " + audioBitrate + "k -af channelmap=channel_layout=5.1";
            }
            else if (audioCodec == "AAC CBR")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to AAC CBR", "log.log");
                }
                allAudioSettings = " -c:a aac -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "AC3")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to AC3 CBR", "log.log");
                }
                allAudioSettings = " -c:a ac3 -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "FLAC")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to FLAC", "log.log");
                }
                allAudioSettings = " -c:a flac ";
            }
            else if (audioCodec == "MP3 CBR")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to MP3 CBR", "log.log");
                }
                allAudioSettings = " -c:a libmp3lame -b:a " + audioBitrate + "k ";
            }
            else if (audioCodec == "MP3 VBR")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to MP3 CBR", "log.log");
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
            MainWindow.SetNumberOfAudioTracks(audioCount);
            
        }
    }
}

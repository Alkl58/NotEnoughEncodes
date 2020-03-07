using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace NotEnoughEncodes
{
    internal class AudioEncode
    {
        public static void EncodeAudio(string videoInput, bool logging, string audioBitrate, string audioCodec, string currentPath, string ffmpegPath, bool trackone, bool tracktwo, bool trackthree, bool trackfour)
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
            else if (audioCodec == "Opus Downmix")
            {
                if (logging == true)
                {
                    SmallScripts.WriteToFileThreadSafe(DateTime.Now.ToString("h:mm:ss tt") + " Audio Encoding Setting Encode Mode to Opus Downmix", "log.log");
                }
                allAudioSettings = " -c:a libopus -b:a " + audioBitrate + "k -ac 2";
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
            startInfo.WorkingDirectory = ffmpegPath;
            startInfo.FileName = "cmd.exe";

            if (trackone == true && tracktwo == true && trackthree == true && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio2.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio3.mkv" + '\u0022';
            }
            //Only One out of Four Tracks
            //1st Track
            if (trackone == true && tracktwo == false && trackthree == false && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022';
            }
            //2nd Track
            if (trackone == false && tracktwo == true && trackthree == false && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022';
            }
            //3rd Track
            if (trackone == false && tracktwo == false && trackthree == true && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022';
            }
            //4th Track
            if (trackone == false && tracktwo == false && trackthree == false && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022';
            }

            //Two out of Four Tracks
            //1st & 2nd     //                   //
            if (trackone == true && tracktwo == true && trackthree == false && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }
            //1st & 3rd     //                                          //
            if (trackone == true && tracktwo == false && trackthree == true && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }
            //1st & 4th     //                                                               //
            if (trackone == true && tracktwo == false && trackthree == false && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }
            //2nd & 3rd                          //                     //
            if (trackone == false && tracktwo == true && trackthree == true && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }
            //2nd & 4th                          //                                          //
            if (trackone == false && tracktwo == true && trackthree == false && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }
            //3rd & 4th                                                 //                   //
            if (trackone == false && tracktwo == false && trackthree == true && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022';
            }

            //Three out of Four Tracks
            //1st & 2nd & 3rd //                   //                    //
            if (trackone == true && tracktwo == true && trackthree == true && trackfour == false)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio2.mkv" + '\u0022';
            }
            //1st & 2nd & 4th //                   //                                       //
            if (trackone == true && tracktwo == true && trackthree == false && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio2.mkv" + '\u0022';
            }
            //1st & 3rd & 4th //                                       //                   //
            if (trackone == true && tracktwo == false && trackthree == true && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:0 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio2.mkv" + '\u0022';
            }
            //2nd & 3rd & 4th                      //                  //                   //
            if (trackone == false && tracktwo == true && trackthree == true && trackfour == true)
            {
                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:1 " + '\u0022' + currentPath + "\\AudioExtracted\\audio0.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:2 " + '\u0022' + currentPath + "\\AudioExtracted\\audio1.mkv" + '\u0022' + " & ffmpeg.exe -i " + '\u0022' + videoInput + '\u0022' + " -vn -map_metadata -1 -c copy -map 0:a:3 " + '\u0022' + currentPath + "\\AudioExtracted\\audio2.mkv" + '\u0022';
            }

            Console.WriteLine(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            Console.WriteLine(currentPath + "\\AudioExtracted");
            DirectoryInfo AudioExtracted = new DirectoryInfo(currentPath + "\\AudioExtracted");
            //Loops through all mkv files in AudioExtracted
            foreach (var file in AudioExtracted.GetFiles("*.mkv"))
            {
                //Directory.Move(file.FullName, filepath + "\\TextFiles\\" + file.Name);
                //Get the Filesize, because the command above also creates mkv files even if there is not audiostream (filesize = 0)
                long length = new FileInfo(currentPath +"\\AudioExtracted\\" + file).Length;
                //Console.WriteLine(length);
                //If Filesize = 0 -> delete file
                if (length == 0)
                {
                    File.Delete(currentPath + "\\AudioExtracted\\" + file);
                }
                else if (length > 1)
                {
                    //Encodes the Audio to the given format
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.WorkingDirectory = ffmpegPath;
                    startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + currentPath + "\\AudioExtracted\\" + file + '\u0022' + " " + allAudioSettings + "-vn " + '\u0022' + currentPath + "\\AudioEncoded\\" + file + '\u0022';
                    Console.WriteLine(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            //Counts the number of AudioFiles
            int audioCount = Directory.GetFiles(currentPath + "\\AudioEncoded", "*mkv", SearchOption.TopDirectoryOnly).Length;
            //Sets the number of AudioTracks of the concat process
            MainWindow.SetNumberOfAudioTracks(audioCount);
        }
    }
}
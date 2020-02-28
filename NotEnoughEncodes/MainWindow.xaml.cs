using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Threading;

namespace NotEnoughEncodes
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Open File Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                textBlockPath.Text = openFileDialog.FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Sets the working directory
            string currentPath = Directory.GetCurrentDirectory();
            //Checks if Chunks folder exist, if no it creates Chunks folder
            if (!Directory.Exists(Path.Combine(currentPath, "Chunks")))
                Directory.CreateDirectory(Path.Combine(currentPath, "Chunks"));


            //Start Splitting
            String videoInput = textBlockPath.Text;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            //FFmpeg Arguments
            startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022'+videoInput+'\u0022' + " -vcodec copy -f segment -segment_time "+ TextBoxChunkLength.Text + " -an " + '\u0022' +"Chunks\\out%0d.mp4"+ '\u0022';
            Console.WriteLine(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            //Create Array List with all Chunks
            string[] chunks;
            //Sets the Chunks directory
            String sdira = currentPath+"\\Chunks";
            //Add all Files in Chunks Folder to array
            chunks = Directory.GetFiles(sdira, "*", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();

            //Checks if there is more than 9 Chunks, inorder to rename them, because later in order to concat, it has to be sortet (Values from xx-xx instead of x-xx)
            if (chunks.Count() >= 10)
            {
                System.IO.File.Move(sdira + "\\out0.mp4", sdira + "\\out00.mp4");
                System.IO.File.Move(sdira + "\\out1.mp4", sdira + "\\out01.mp4");
                System.IO.File.Move(sdira + "\\out2.mp4", sdira + "\\out02.mp4");
                System.IO.File.Move(sdira + "\\out3.mp4", sdira + "\\out03.mp4");
                System.IO.File.Move(sdira + "\\out4.mp4", sdira + "\\out04.mp4");
                System.IO.File.Move(sdira + "\\out5.mp4", sdira + "\\out05.mp4");
                System.IO.File.Move(sdira + "\\out6.mp4", sdira + "\\out06.mp4");
                System.IO.File.Move(sdira + "\\out7.mp4", sdira + "\\out07.mp4");
                System.IO.File.Move(sdira + "\\out8.mp4", sdira + "\\out08.mp4");
                System.IO.File.Move(sdira + "\\out9.mp4", sdira + "\\out09.mp4");
                //Clears the Array
                Array.Clear(chunks, 0, chunks.Length);
                //Readds all Files to array
                chunks = Directory.GetFiles(sdira, "*", SearchOption.AllDirectories).Select(x => Path.GetFileName(x)).ToArray();
            }
 
            //Parse Textbox Text to Strin for loop threading
            int maxConcurrency = Int16.Parse(TextBoxNumberWorkers.Text);
            int cpuUsed = Int16.Parse(TextBoxCpuUsed.Text);
            int bitDepth = Int16.Parse(TextBoxBitDepth.Text);
            int encThreads = Int16.Parse(TextBoxEncThreads.Text);
            int cqLevel = Int16.Parse(TextBoxcqLevel.Text);
            int kfmaxdist = Int16.Parse(TextBoxKeyframeInterval.Text);
            int tilecols = Int16.Parse(TextBoxTileCols.Text);
            int tilerows = Int16.Parse(TextBoxTileRows.Text);
            int nrPasses = Int16.Parse(TextBoxPasses.Text);

            //Encode concurrent
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
            {
                List<Task> tasks = new List<Task>();
                foreach (var item in chunks)
                {
                    concurrencySemaphore.Wait();

                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            
                            if (nrPasses == 1)
                            {
                                //1-Pass Encode
                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.FileName = "cmd.exe";
                                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + item + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=1 --cpu-used="+cpuUsed+" --threads="+encThreads+ " --end-usage=q --cq-level="+cqLevel+ " --bit-depth="+bitDepth+" --tile-columns="+tilecols+ " --tile-rows="+tilerows+" --kf-max-dist="+kfmaxdist+" --output=Chunks\\"+item+"_av1.ivf";
                                process.StartInfo = startInfo;
                                Console.WriteLine(startInfo.Arguments);
                                process.Start();
                                process.WaitForExit();
                                
                            }
                            else if (nrPasses == 2)
                            {
                                //2-Pass Encode

                                //Pass 1
                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.FileName = "cmd.exe";
                                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + item + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=1 --fpf="+ item +"_stats.log --cpu-used=" + cpuUsed + " --threads=" + encThreads + " --end-usage=q --cq-level=" + cqLevel + " --bit-depth=" + bitDepth + " --tile-columns=" + tilecols + " --tile-rows=" + tilerows + " --kf-max-dist=" + kfmaxdist + " --output=" + item + "_av1.ivf";
                                process.StartInfo = startInfo;
                                Console.WriteLine(startInfo.Arguments);
                                process.Start();
                                process.WaitForExit();

                                //Pass 2
                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.FileName = "cmd.exe";
                                startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + item + '\u0022' + " -pix_fmt yuv420p -vsync 0 -f yuv4mpegpipe - | aomenc.exe - --passes=2 --pass=2 --fpf=" + item + "_stats.log --cpu-used=" + cpuUsed + " --threads=" + encThreads + " --end-usage=q --cq-level=" + cqLevel + " --bit-depth=" + bitDepth + " --tile-columns=" + tilecols + " --tile-rows=" + tilerows + " --kf-max-dist=" + kfmaxdist + " --output=" + item + "_av1.ivf";
                                process.StartInfo = startInfo;
                                Console.WriteLine(startInfo.Arguments);
                                process.Start();
                                process.WaitForExit();

                            }

                            //startInfo.Arguments = "/C ffmpeg.exe -i " + '\u0022' + sdira + "\\" + item + '\u0022' + " -c:v utvideo -an " + '\u0022' + "Chunks\\" + item +"_av1.ivf" + '\u0022';
                            
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

            //Creates Output Folder
            if (!Directory.Exists(Path.Combine(currentPath, "Output")))
                Directory.CreateDirectory(Path.Combine(currentPath, "Output"));

            //Lists all ivf files in mylist.txt
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            //FFmpeg Arguments
            startInfo.Arguments = "/C (for %i in (Chunks\\*.ivf) do @echo file '%i') > Chunks\\mylist.txt";
            Console.WriteLine(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            //Concat the Videos
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            //FFmpeg Arguments
            startInfo.Arguments = "/C ffmpeg.exe -f concat -safe 0 -i Chunks\\mylist.txt -c copy Output\\output.mkv";
            Console.WriteLine(startInfo.Arguments);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
         }

    }
}

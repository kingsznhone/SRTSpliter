using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SRTSpliter
{
    public struct TimeStamp
    {
        public string start;
        public string end;

        public override string ToString()
        {
            return $"-ss {start} -to {end} ";
        }
    }

    internal class Program
    {
        public static object NEOElapsed { get; private set; }

        static void Main(string[] args)
        {
            List<FileInfo> srtFiles = args.Select(x => new FileInfo(x)).ToList();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (FileInfo file in srtFiles)
            {
                string dirName = Path.GetFileNameWithoutExtension(file.FullName);
                string resultDirPath=  Path.Combine(file.Directory.FullName, dirName);
                if(!Directory.Exists(resultDirPath) ) 
                {
                    Directory.CreateDirectory(resultDirPath);
                }
                DirectoryInfo resultDir = new DirectoryInfo(resultDirPath);
                FileInfo wavfile = new FileInfo(resultDirPath + ".wav");

                var stampCollection = ParseSRT(file);

                
                SplitWav(stampCollection, wavfile, resultDir);
                
            }
            sw.Stop();
            double Elapsed = sw.Elapsed.TotalSeconds;

            Console.WriteLine($"Elapsed: {Elapsed}");
            Console.WriteLine("Split Finish...");
            Console.ReadLine();
        }
        private static List<TimeStamp> ParseSRT(FileInfo file)
        {
            Regex re = new Regex(@"^(\d{2}:\d{2}:\d{2},\d{3}) --> (\d{2}:\d{2}:\d{2},\d{3})");
            List < TimeStamp > result = new List<TimeStamp>();
            using (StreamReader sr = new StreamReader(file.FullName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Match match = re.Match(line);

                    // 判断是否有匹配
                    if (match.Success)
                    {
                        TimeStamp stamp = new TimeStamp
                        {
                            start = match.Groups[1].Value.Replace(',', '.'),
                            end = match.Groups[2].Value.Replace(',', '.')
                        };
                        result.Add(stamp);

                        // 打印匹配到的内容
                        Console.WriteLine("Match found: " + stamp.ToString());
                    }
                }
            }

            return result;
        }

        private static void SplitWav(List<TimeStamp> stamps, FileInfo wavfile, DirectoryInfo ResultDir)
        {
            string ffmpegPath =  "ffmpeg";
            string segmentPrefix = Path.GetFileNameWithoutExtension(wavfile.FullName);

            List<string> commandList = new List<string>();
            var CommandBuilder = new StringBuilder($" -hide_banner -y -i \"{wavfile}\" -c:a pcm_s16le -ac 1 -ar 44100 ");

            for (int index =0; index<stamps.Count; index++)
            {
                string buffer = $" {stamps[index]} \"{ResultDir.FullName}/{segmentPrefix}_{index + 1}.wav\"";
                if (CommandBuilder.Length+ buffer.Length > 1000)
                {
                    commandList.Add(CommandBuilder.ToString());
                    CommandBuilder = new StringBuilder($" -hide_banner -y -i \"{wavfile}\" -c:a pcm_s16le -ac 1 -ar 44100 ");
                }
                CommandBuilder.Append(buffer);
            }
            commandList.Add(CommandBuilder.ToString());

            Parallel.For(0, commandList.Count, i =>
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = commandList[i],
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardErrorEncoding = Encoding.UTF8
                };
                using (Process process = new Process { StartInfo = psi })
                {
                    try
                    {
                        process.Start();
                        string output = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        Console.WriteLine(output);
                        Console.WriteLine();
                    }
                    catch { }
                }
            });
        }
    }



}

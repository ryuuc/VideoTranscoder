using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace VideoTranscoder
{
    public class VideoConverter
    {
        private string _sourceVideoFile;
        public string SourceVideoFile
        {
            get { return _sourceVideoFile; }
            set { _sourceVideoFile = value; }
        }
        public VideoConverter(string sourceVideoFile)
        {
            _sourceVideoFile = sourceVideoFile;
        }

        private bool isStopFlg = false;
        private Process process = null;
        private string newFile = string.Empty;

        string _executeMsg = string.Empty;
        public string ExecuteAsync()
        {

            if (string.IsNullOrEmpty(_sourceVideoFile)) return "路径不可为空";
            if (!File.Exists(_sourceVideoFile)) return "文件不存在";

            for (int i = 0; i < 30; i++)
            {
                if (Process.GetProcessesByName("ffmpeg").Length == 0) break;
                Process.GetProcessesByName("ffmpeg")[0].Kill();
                System.Threading.Thread.Sleep(1000);
            }

            FileInfo fi = new FileInfo(_sourceVideoFile);
            newFile = fi.FullName.Replace(fi.Extension, ".mp4");
            if (File.Exists(newFile)) File.Delete(newFile);

            ProcessStartInfo info = new ProcessStartInfo("ffmpeg.exe", string.Format("-i {0} -c:v libx264 -crf 23 -preset medium -vsync 1 -r 25 -c:a aac -strict -2 -b:a 64k -ar 44100 -ac 1 {1}", _sourceVideoFile, newFile));//-vb 100k -ab 48k -r 20 
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            process = new Process();
            process.StartInfo = info;

            process.EnableRaisingEvents = true;

            process.ErrorDataReceived += new DataReceivedEventHandler(
                (sender, e) =>
                {
                    if (!isStopFlg)
                    {
                        System.Diagnostics.Process.Start(newFile);
                        _executeMsg = "处理完毕。";
                    }
                });

            process.OutputDataReceived += new DataReceivedEventHandler(
                (sender, e) =>
                {
                    if (!isStopFlg)
                    {
                        System.Diagnostics.Process.Start(newFile);
                        _executeMsg = DateTime.Now.ToString("hh: mm: ss: fff") + ": " + (isStopFlg ? "中途退出。" : "转码完毕。");
                    }
                });
            process.Exited += new EventHandler(process_Exited);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return "";
        }

        private void process_Exited(object sender, EventArgs e)
        {
            process.Close();
            process.Dispose();
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            process.Close();
            process.Dispose();
        }
    }
}

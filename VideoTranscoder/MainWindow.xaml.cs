using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace VideoTranscoder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.btnOpen.Click += BtnOpen_Click;
            this.btnStart.Click += BtnStart_Click;
            this.btnStop.Click += BtnStop_Click;
        }

        private bool isStopFlg = false;
        private Process process = null;
        private string outputFile = string.Empty;

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (process != null && !this.btnStart.IsEnabled)
            {
                if (MessageBox.Show("处理中，确定终止？", "视频转码", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    isStopFlg = true;
                    Process[] p = Process.GetProcessesByName("HandBrakeCLI");
                    if (p.Length > 0)
                    {
                        p[0].Kill();
                        System.Threading.Thread.Sleep(100);
                    }
                    process.Close();
                    this.btnStart.IsEnabled = true;
                    this.txtMsg.Text = "转码终止";
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.tbxFilePath.Text))
            {
                ExecuteAsync(this.tbxFilePath.Text.Trim());
            }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".wmv";
            ofd.Filter = "wmv file|*.wmv";
            if (ofd.ShowDialog() == true) this.tbxFilePath.Text = ofd.FileName;
        }

        private void ExecuteAsync(string inputFile)
        {
            isStopFlg = false;
            this.btnStart.IsEnabled = false;
            if (!File.Exists(inputFile)) return;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    if (Process.GetProcessesByName("HandBrakeCLI").Length == 0) break;
                    Process.GetProcessesByName("HandBrakeCLI")[0].Kill();
                    System.Threading.Thread.Sleep(1000);
                }

                FileInfo fi = new FileInfo(inputFile);
                outputFile = fi.FullName.Replace(fi.Extension, ".mp4");
                if (File.Exists(outputFile)) File.Delete(outputFile);

                this.txtMsg.Text = "正在转码，请稍后...";
                ProcessStartInfo info = new ProcessStartInfo("HandBrakeCLI.exe", string.Format("-i {0} -o {1}", inputFile, outputFile));
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                process = new Process();
                process.StartInfo = info;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(
                    (sender, e) =>
                    {
                        if (!isStopFlg)
                        {
                            this.txtMsg.Dispatcher.Invoke(new Action(() => { this.txtMsg.Text = "转码完成。"; }));
                            this.btnStart.Dispatcher.Invoke(new Action(() => { this.btnStart.IsEnabled = true; }));
                            process.Close();
                            process.Dispose();
                            System.Diagnostics.Process.Start(outputFile);
                        }
                    });
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

            }
            catch (Exception e)
            {
                this.txtMsg.Dispatcher.Invoke(new Action(() => { this.txtMsg.Text = e.Message; }));
                this.btnStart.Dispatcher.Invoke(new Action(() => { this.btnStart.IsEnabled = true; }));
                if (process != null) process.Dispose();
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace BKLTaskService
{
    public partial class Service1 : ServiceBase
    {
        private Process process;
        private Thread thread;
        private StreamWriter SW;
        private string main_path;
        private string python_path;
        private string logs_path;

        public Service1()
        {
            InitializeComponent();
        }

        private string get_python_path()
        {
            // 指定要遍历的目录  
            string directory = @"C:\Users\";

            // 使用 EnumerateDirectories 方法遍历目录树  
            foreach (string directoryName in Directory.EnumerateDirectories(directory))
            {
                string pythonPath = directoryName + @"\AppData\Local\Programs\Python\Python38\python.exe";

                if (File.Exists(pythonPath))
                {
                    return pythonPath;
                }
                else { continue; }
            }

            return null;
        }

        private string get_main_path()
        {
            string programDataPath = Environment.GetEnvironmentVariable("ProgramData");
            string main_path = programDataPath + @"\BKLTaskController";

            if (Directory.Exists(main_path))
            {
                return main_path;
            } else
            {
                return null;
            }
        }

        private string get_mainpyc_path()
        {
            string mainpyc_path = main_path + @"\Bin\main.txt";

            if (File.Exists(mainpyc_path))
            {
                return mainpyc_path;
            } else {
                return null;
            }
        }

        private void note(string msg)
        {
            try
            {
                SW.WriteLine(DateTime.Now.ToString("[ yyyy-MM-dd HH:mm:ss ]") + $"[ INFO ] {msg}");
            } catch
            {
                SW = new StreamWriter(logs_path + @"\last.log", true);
                SW.AutoFlush = true;
                SW.WriteLine(DateTime.Now.ToString("[ yyyy-MM-dd HH:mm:ss ]") + $"[ INFO ] {msg}");
            }

        }

        private void start_process()
        {

            process = new Process();

            process.StartInfo.FileName = python_path;
            process.StartInfo.Arguments = get_mainpyc_path() + @" --auto-start";

            process.Start();

        }

        private string randomDisplayName()
        {
            // 获取所有服务
            ServiceController[] services = ServiceController.GetServices();

            // 生成随机数
            Random rnd = new Random();
            int randomIndex = rnd.Next(0, services.Length);

            // 获取随机选择的服务的显示名称
            string randomDisplayName = services[randomIndex].DisplayName;

            // 更改服务的显示名称
            Process.Start("sc.exe", $"config BKLTaskService DisplayName=\"{randomDisplayName}\"");

            return randomDisplayName;
        }

        protected override void OnStart(string[] args)
        {
            logs_path = @"C:\Windows\Logs\BKLTaskService";

            if (!Directory.Exists(logs_path))
            {
                Directory.CreateDirectory(logs_path);
            }

            SW = new StreamWriter(logs_path + @"\last.log", true);
            SW.AutoFlush = true;

            SW.WriteLine();

            note("BKLTaskService Now Started.");

            note($"New Serverice Name --> {randomDisplayName()}");


            main_path = get_main_path();

            if (main_path == null)
            {
                note("This device is not installed BKLTaskController. Stop the Service.");
                Stop();
                return;
            }

            note("BKLTaskController detected.");

            python_path = get_python_path();

            if (python_path == null)
            {
                note("This device is not installed Runtime. Stop the Service.");
                Stop();
                return;
            }

            note("The Runtime detected.");

            try
            {
                start_process();
            } catch (Exception)
            {
                note("Process startup failed.");
                Stop();
            }

            note("Process started successfully.");

            thread = new Thread(new ThreadStart(this.wait_process));
            thread.Start();

            note("Started done.");
        }

        protected override void OnStop()
        {
            note("BKLTaskService Is Stoping.");

            SW.Close();

            ExitCode = 0;
        }

        protected override void OnShutdown()
        {
            Stop();
        }

        private void wait_process()
        {
            while (true)
            {
                process.WaitForExit();
                process.Close();
                note("BKLTaskController Has Been Ended. Restart it.");

                try
                {
                    start_process();
                }
                catch (Exception)
                {
                    note("Process restart failed.");
                    Stop();
                    break;
                }
            }
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}

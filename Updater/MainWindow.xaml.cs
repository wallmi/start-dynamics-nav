using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Updater
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string temp = System.IO.Path.GetTempPath() + @"StartNAV\Updater\";
        readonly Log log = new Log();
        string updateuri;

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 1)
                return;          

            Update(args[1]);
        }

        private void Update (string path)
        {
            try 
            {
                //Download
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebClient wc = new WebClient();
                Directory.CreateDirectory(temp);
                File.Delete(temp + "package.7z");
                wc.DownloadFile(path, temp + "package.7z");
                log.Add("Downloaded from: " + path);

                //entpacken 7-zip
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = @"7z.exe";
                processStartInfo.Arguments = @"e ";
                processStartInfo.Arguments += temp + "package.7z ";
                processStartInfo.Arguments += @"-o" + temp + @"files";
                Process pr = Process.Start(processStartInfo);
                pr.WaitForExit();
                log.Add("Entpackt nach: " + temp + @"files");

                //Kopiere die Dateien
                foreach(string file in Directory.GetFiles(temp+@"files\"))
                {
                    if (System.IO.Path.GetFileName(file) == "Updater.exe")
                        FileCopy(file, "Updater_new.exe");
                    else
                        FileCopy(file, System.IO.Path.GetFileName(file));
                }

                //FileCopy(@temp+ @"files\StartNAV.exe", "StartNAV.exe");
                //FileCopy(@temp + @"files\IniFileParser.dll", "IniFileParser.dll");

                
                //Temp Verzeichnis löschen
                Directory.Delete(temp,true);
                log.Add("Temporäres Verzeichnis gelöscht");
                updateuri = path;
                Close();
            }
            catch (Exception e)
            {
                log.Add("Ausnahme Fehler: " + e.Message);
                tb_errormsg.Text = e.Message;
            }

            
        }

        private void FileCopy(string from, string to)
        {
            File.Copy(from, to, true);
            log.Add("Datei kopiert: " + from + " >>> " + to);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            log.SaveToFile();

            ProcessStartInfo start = new ProcessStartInfo("StartNAV.exe");
            start.Arguments = updateuri;

            
            if (Environment.GetCommandLineArgs().Length >= 3)           //For Debugging
                if (Environment.GetCommandLineArgs()[2] == "debug")     //For Debugging
                    return;

            Process.Start(start);
        }
    }
}

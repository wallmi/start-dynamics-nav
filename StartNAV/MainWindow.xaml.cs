using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using Octokit;

using StartNAV.Model;
using StartNAV.Dialog;
using System.Diagnostics;
using Microsoft.Win32;

namespace StartNAV
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string INIFILE = "Settings.ini";
        const string OBJECTFILE = "NAV_Objects.csv";
        readonly IniHandler ini;
        readonly ObjectHandler handler;
        readonly List<object> toSave = new List<object>();
        readonly Log Loghandler;
        enum StartTyp {Nav,Session }
      
        public MainWindow()
        {
            InitializeComponent();
            Loghandler = new Log(tb_status);
            handler = new ObjectHandler(OBJECTFILE);
            ini = new IniHandler(INIFILE, OBJECTFILE,Loghandler);
            
            ListView lv = lv_fav.lv_items;
            lv.MouseDoubleClick += new MouseButtonEventHandler(Lv_fav_MouseDoubleClick);
            
            foreach (String temp in NavObjects.GetObjectNames())
                cb_objektTyp.Items.Add(temp);

            //TODO: Geht sicher auch schöner :)
            toSave.Add(cb_server);              toSave.Add(cb_mandant);
            toSave.Add(cb_objektTyp);           toSave.Add(tx_objId);
            toSave.Add(cbo_config);             toSave.Add(cbo_debug);
            toSave.Add(cbo_schow_startstring);  toSave.Add(cb_profil);

            Load();

            if (!ini.CheckExe())
            {
                string tooltip = "Für diese Option muss zuerst die Anwendung ausgewählt werden";
                cbo_config.IsEnabled = false;
                cbo_debug.IsEnabled = false;
                b_start_session_list.IsEnabled = false;
                cb_profil.IsEnabled = false;
                b_start_nav.IsEnabled = false;

                cb_profil.SelectedItem = "<kein Profil>";
                cbo_config.IsChecked = false;
                cbo_debug.IsChecked = false;

                cbo_config.ToolTip = tooltip;
                cbo_debug.ToolTip = tooltip;
                cb_profil.ToolTip = tooltip;
                b_start_session_list.ToolTip = tooltip;
                b_start_nav.ToolTip = tooltip;
            }

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 2) { 
                MessageBox.Show("Argumente: " + args[1], "Update");
                ini.SetSettings("updateuri", args[1]);
            }


            if (ini.GetSetting("upd") == "true")
                UpdateApplicationAsync();
        
        }

        void Load()
        {
            Load_Server();
            Load_Profil();
            LoadFav();
            LoadSettings();
        }

        void LoadFav()
        {
            List<NavObject> favs = ini.GetFav();
            lv_fav.SetItems(favs);

            Loghandler.Add(Resource.Load_Fav);
            if (!handler.withversion)
                lv_fav.setShowVersion(false);
        }

        #region Actions
        private void B_delfile_Click(object sender, RoutedEventArgs e)
        {
            ini.MakeBackup();
            ini.DelFile();
            Load();
        }

        private void Tx_objId_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetObjectName();
        }


        private void Cb_server_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Load_mandanten();
            if (cb_server.SelectedItem is null)
                return;                         
            tbl_serveradresse.Text = ini.GetServerAdress(cb_server.SelectedItem.ToString());
        }

        private void Cb_objektTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_objektTyp.SelectedItem is null)  
                return;                             

            if (cb_objektTyp.SelectedItem.ToString() == "None") { 
                tx_objId.IsEnabled = false;
                b_add_fav.IsEnabled = false;
                b_getId.IsEnabled = false;
            } else
            {
                tx_objId.IsEnabled = true;
                b_add_fav.IsEnabled = true;
                b_getId.IsEnabled = true;
            }
            GetObjectName();
        }
        private void B_StartNav_Click(object sender, RoutedEventArgs e)
        {
            string exe = "";
            string param;
            
            if (((Button)sender).Name == "b_start_session_list")
                param = GetStartParameter(StartTyp.Session);
            else
                param = GetStartParameter(StartTyp.Nav);

            if (ini.CheckExe())
                exe = ini.Data["Settings"]["ExePath"] + " ";
            
            
            if (cbo_schow_startstring.IsChecked == true)
            {
                MessageBox.Show(exe + " " +param,"Start String",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            Loghandler.Add("Nav gestartet mit Parameter:" + param);
            if (String.IsNullOrEmpty(exe))
                return;

            Process.Start(exe,param);
        }

        string GetStartParameter(StartTyp st)
        {
            string Navbase = "\"dynamicsnav://";
            string ServerAdress = ini.GetServerAdress(cb_server.Text);
            string Mandant = "/" + cb_mandant.Text;
            ObjectType ob = NavObjects.GetObj(cb_objektTyp.Text);
            string Config = " -configure";
            string Debug = " -debug";
            string Profile = " -profile:";
            string sessionlist = " -protocolhandler";
            string checkedServer = handler.CheckServerString(ServerAdress);
             string startstring = "";

            //Serveradresse aktualisieren
            if (checkedServer != ServerAdress)
            {
                ini.AddServer(cb_server.Text, checkedServer);
                ServerAdress = ini.GetServerAdress(cb_server.Text);
            }

            string ObjectStart = "/";
            startstring += Navbase + ServerAdress;

            if (st == StartTyp.Nav)
            {
                switch (ob)
                {
                    case ObjectType.Page: ObjectStart += "runpage?page=" + tx_objId.Text; break;
                    case ObjectType.Table: ObjectStart += "runtable?table=" + tx_objId.Text; break;
                    case ObjectType.Report: ObjectStart += "runreport?report=" + tx_objId.Text; break;
                }

               startstring += Mandant + ObjectStart + "\"";
                if (cb_profil.Text != "<kein Profil>")
                    startstring += Profile + cb_profil.Text;
                if (cbo_config.IsChecked.Value)
                    startstring += Config;
                if (cbo_debug.IsChecked.Value)
                    startstring += Debug;
            }
            else if (st == StartTyp.Session)
            {
                startstring += "//debug\"" + sessionlist;
            }
            return startstring;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddServer w = new AddServer(ini);
            w.ShowDialog();
            Load_Server();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ini.DelServer(cb_server.Text);
            Load_Server();

        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ini.DelMandant(cb_server.Text, cb_mandant.Text);
            Load_mandanten();
        }
        #endregion

        void Load_Server()
        {
            cb_server.ItemsSource = ini.GetServer();
        }

        void Load_Profil()
        {
            cb_profil.ItemsSource = ini.GetProfile();
        }

        void Load_mandanten()
        {
            if (!(cb_server.SelectedItem is null))
                cb_mandant.ItemsSource = ini.GetMandanten(cb_server.SelectedItem.ToString());
            else
                cb_mandant.ItemsSource = null;
        }

        void GetObjectName()
        {
            //Auslesen des Object Namens
            //TODO: Eingabe Text verhindern  
            if (!handler.IsLoaded())
            {
                tb_ObjektName.Text = "Objekt kann nicht angezeigt werden";
                return;
            }

            try
            {
                int id = Int32.Parse(tx_objId.Text);
                string text = handler.GetObjName(id, cb_objektTyp.Text); 
                if (cb_objektTyp.SelectedItem.ToString() == ObjectType.None.ToString())
                {
                    tb_ObjektName.Text = "";
                    return;
                }

                if (text == "")
                    tb_ObjektName.Text = "ID nicht gefunden";
                else
                    tb_ObjektName.Text = text;
            }
            catch
            {
                tb_ObjektName.Text = "Fehlerhafte Eingabe";
            }
        }

        private void B_add_fav_Click(object sender, RoutedEventArgs e)
        {
            int id = Int32.Parse(tx_objId.Text);
            //lv_fav.Add(new NavObject(cb_objektTyp.Text, id, tb_ObjektName.Text));
            ini.AddFav(cb_objektTyp.Text,id);
            LoadFav();
        }


        private void B_del_fav_Click(object sender, RoutedEventArgs e)
        {
            //foreach (NavObject temp in lv_fav.GetSelectItems())
            //    ini.DeleteFav(temp.Typ, temp.ID);

            ini.DeleteFavs(lv_fav.GetSelectItems());
            LoadFav();
        }

        private void Lv_fav_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lv_fav.GetSelectItems().Count == 1)
            {
                NavObject temp = (NavObject)lv_fav.GetSelectItems()[0];
                tx_objId.Text = temp.ID.ToString();
                cb_objektTyp.Text = NavObjects.GetName(temp.Typ);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        void SaveSettings()
        {
            foreach (object temp in toSave)
            {
                if (temp.GetType() == typeof(ComboBox))
                {
                    ComboBox cb = (ComboBox)temp;
                    ini.Data["Settings"][cb.Name] = cb.Text;
                }
                else if (temp.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)temp;
                    ini.Data["Settings"][tb.Name] = tb.Text;
                }
                else if (temp.GetType() == typeof(CheckBox))
                {
                    CheckBox cb = (CheckBox)temp;
                    ini.Data["Settings"][cb.Name] = cb.IsChecked.ToString();
                }
            }
            ini.WriteFile();
        }
        void LoadSettings()
        {
            foreach (object temp in toSave)
            {
                if (temp.GetType() == typeof(ComboBox))
                {
                    ComboBox cb = (ComboBox)temp;
                    cb.Text = ini.Data["Settings"][cb.Name];
                }
                else if (temp.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)temp;
                    tb.Text = ini.Data["Settings"][tb.Name];
                }
                else if (temp.GetType() == typeof(CheckBox))
                {
                    CheckBox cb = (CheckBox)temp;
                    if(ini.Data["Settings"].ContainsKey(cb.Name))
                        cb.IsChecked = Boolean.Parse(ini.Data["Settings"][cb.Name]);
                }
            }
            Loghandler.Add("Einstellungen geladen:" + ini.GetFilename());
        }
        
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            AddMandant w = new AddMandant(ini);
            w.SetServer(ini.GetServer(), cb_server.Text);
            w.ShowDialog();
            Load_mandanten();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Mi_save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void B_getId_Click(object sender, RoutedEventArgs e)
        {
            GetObject w = new GetObject(handler);

            if (((Button)sender).Name == b_get_favs.Name)
            {
                w.ShowDialog3();
                //foreach (NavObject temp in w.retList)
                //{
                //    ini.AddFav(temp.Typ, temp.ID);
                //}
                ini.AddFavs(w.getSelectionList());
                LoadFav();
            }
            else
            {
                w.ShowDialog2();
                NavObject ret = w.getSelectedItem();
                tx_objId.Text = ret.ID.ToString();
                cb_objektTyp.Text = ret.Typ.ToString();
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            AddProfile w = new AddProfile(ini);
            w.ShowDialog();
            Load_Profil();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            ini.DelProfile(cb_profil.Text);
            Load_Profil();
        }

        private void Mi_set_exe_path_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Client (Microsoft.Dynamics.Nav.Client.exe)|Microsoft.Dynamics.Nav.Client.exe",
                  InitialDirectory = @"C:\Program Files (x86)\Microsoft Dynamics NAV\100\RoleTailored Client\"
            };

            if (dialog.ShowDialog() == true)
            {
                ini.SetExePath(dialog.FileName);
                MessageBox.Show("Pfad zur exe wurde geändert. Bitte Anwendung neu starten!",
                    "Neustart erforderlich",MessageBoxButton.OK,MessageBoxImage.Information);
                Loghandler.Add("Pfad zur exe gesetzt: " +dialog.FileName);
            }
        }

        private void Link2Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/wallmi/start-dynamics-nav");
        }

        private void CreateIssue_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/wallmi/start-dynamics-nav/issues/new");
        }

        private void Reload_objlist_Click(object sender, RoutedEventArgs e)
        {
            handler.LoadFromFile(OBJECTFILE);
            Loghandler.Add("Object File neu eingelesen");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Loghandler.Add("Programm Ordnungsmäßig beendet");
            Loghandler.SaveToFile();
            ini.WriteFile();
        }

        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            LogWindow dw = new LogWindow(Loghandler.GetEntries());
            dw.Show();
        }

        private void UpdateApplication(object sender, RoutedEventArgs e)
        {
            UpdateApplicationAsync();
        }

        private async Task UpdateApplicationAsync()
        {
            //MessageBoxResult res;
            ProcessStartInfo upd = new ProcessStartInfo("Updater.exe");

            //res = MessageBox.Show("Wollen sie die letzte stabile Version installieren? \n (Nein = Beta Version installieren)", "Update", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            var client = new GitHubClient(new ProductHeaderValue(ini.GetSetting("upd_user")));
            //var repo = client.Repository.Get(ini.Data["Settings"]["upd_user"],
            //    ini.Data["Settings"]["upd_repository"]);

            var releases = await client.Repository.Release.GetAll(
                ini.GetSetting("upd_user"), 
                ini.GetSetting("upd_repository"));

            string updateuri = null;

            foreach (var temp in releases) {
                if (temp.Prerelease & ini.GetSetting("upd_beta") == "false")
                    continue;   //Pre Release überstringen

                updateuri = temp.Assets[0].BrowserDownloadUrl;

                if (updateuri == ini.GetSetting("updateuri"))
                    continue;   //Wenn Updateversion gleich ist

                if (String.IsNullOrEmpty(updateuri))
                    continue;   //Wenn kein Release vorhanden ist

                break;
            }

            if (updateuri == null)
                return;

            MessageBoxResult res = MessageBox.Show("Neue Version vorhanden, soll von " + updateuri + " heruntergeladen und installiert werden?",
                "Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.No) {
                Loghandler.Add("Update wurde übersprungen");
                return;
            }

            Loghandler.Add("Update von: " + updateuri);
            
            upd.Arguments = updateuri;
            Process.Start(upd);
            Close();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options opt = new Options(ini);
            opt.ShowDialog();
        }
    }
}

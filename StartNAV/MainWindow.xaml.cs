﻿using System;
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
using System.Threading;

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
        enum StartTyp { Nav, Session }

        public MainWindow()
        {
            InitializeComponent();
            Loghandler = new Log(tb_status);
            handler = new ObjectHandler(OBJECTFILE);
            ini = new IniHandler(INIFILE, OBJECTFILE, Loghandler);

            ListView lv = lv_fav.lv_items;
            lv.MouseDoubleClick += new MouseButtonEventHandler(Lv_fav_MouseDoubleClick);

            foreach (String temp in NavObjects.GetObjectNames())
                cb_objektTyp.Items.Add(temp);

            //Pbjekte die gespeichert werden sollen
            toSave.Add(cb_server);              toSave.Add(cb_mandant);
            toSave.Add(cb_objektTyp);           toSave.Add(tx_objId);
            toSave.Add(cbo_config);             toSave.Add(cbo_debug);
            toSave.Add(cbo_schow_startstring);  toSave.Add(cb_profil);
            toSave.Add(cbo_disable_pers);
            toSave.Add(cb_favGroup);

            //Keine Favgruppeneinstellung gefunden
            if (ini.GetSetting("favgroup") == null) 
                if (MessageBox.Show(Resource.Fav_Group_Ques,
                    "Favoriten Gruppen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                { 
                    ini.SetSettings("favgroup", "true");
                    ini.AddFavGroup("(DEFAULT)");
                    ini.SetSettings("cb_favGroup", "(DEFAULT)");
                }
                else
                    ini.SetSettings("favgroup", "false");

            Load();

            //Kein Pfad zur exe eingerichtet
            if (!ini.CheckExe())
                if (MessageBox.Show("Pfad zur Exe wurde nicht eingerichtet. Möchten sie das jetzt durchführen?",
                    "Pfad zur EXE", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Mi_set_exe_path_Click(null, null);
                else
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


            //Updater aktualisieren
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2) {
                Thread.Sleep(2000);
                try { 
                    File.Copy("Updater_new.exe","Updater.exe",true);
                    File.Delete("Updater_new.exe");
                }
                catch (Exception e)
                {
                    Loghandler.Add("Fehler Updater aktualisieren: " + e.Message);
                }
                Loghandler.Add("Der Updater wurde aktualisiert");
                MessageBox.Show("Update erfolgreich durchgeführt: " + args[1], "Update erfolgreich");
                ini.SetSettings("updateuri", args[1]);
            }

            //Update
            if (ini.GetSetting("upd") == "true")
                UpdateApplicationAsync();
        }

        void Load()
        {
            Load_Server();
            Load_Profil();
            LoadFavGroup();
            LoadFav();
            //-------------
            LoadSettings();
        }

        void LoadFav()
        {
            lv_fav.SetItems(ini.GetFav(GetFavgroup()));

            Loghandler.Add(Resource.Load_Fav+GetFavgroup());
            if (!handler.withversion)
                lv_fav.setShowVersion(false);
        }
        void LoadFavGroup()
        {
            if (ini.GetSetting("favgroup") == "true")
                cb_favGroup.ItemsSource = ini.GetFavGroups();
            else
            { 
                sp_FavGroup.Visibility = Visibility.Hidden;
                sp_FavGroup.Height = 0;
                cb_favGroup.Items.Clear();
                cb_favGroup.Items.Add("NOFAVGROUP");
                cb_favGroup.SelectedItem = "NOFAVGROUP";
            }
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
                MessageBox.Show(exe + " " + param, "Start String", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Loghandler.Add("Nav gestartet mit Parameter:" + param);
            if (String.IsNullOrEmpty(exe))
                return;

            Process.Start(exe, param);
        }

        string GetStartParameter(StartTyp st)
        {
            string Navbase = "\"dynamicsnav://";
            string ServerAdress = ini.GetServerAdress(cb_server.Text);
            string Mandant = "/" + cb_mandant.Text;
            ObjectType ob = NavObjects.GetObj(cb_objektTyp.Text);
            string Config = " -configure";
            string Debug = " -debug";
            string DisablePer = " -disablepersonalization";
            string Profile = " -profile:";
            string sessionlist = " -protocolhandler";
            string checkedServer = ObjectHandler.CheckServerString(ServerAdress);
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
                    case ObjectType.Codeunit: ObjectStart += "runcodeunit?codeunit=" + tx_objId.Text; break;
                }

                startstring += Mandant + ObjectStart + "\"";
                if (cb_profil.Text != "<kein Profil>")
                    startstring += Profile + "\"" + cb_profil.Text + "\"";
                if (cbo_config.IsChecked.Value)
                    startstring += Config;
                if (cbo_debug.IsChecked.Value)
                    startstring += Debug;
                if (cbo_disable_pers.IsChecked.Value)
                    startstring += DisablePer;
            }
            else if (st == StartTyp.Session)
            {
                startstring += "//debug\"" + sessionlist;
            }
            return startstring;
        }

        private void AddServer(object sender, RoutedEventArgs e)
        {
            AddServer w = new AddServer(ini);
            w.ShowDialog();
            Load_Server();
        }

        private void DelServer(object sender, RoutedEventArgs e)
        {
            ini.DelServer(cb_server.Text);
            Load_Server();

        }
        private void DelMandant(object sender, RoutedEventArgs e)
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

                if (String.IsNullOrEmpty(text))
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
            ini.AddFav(cb_objektTyp.Text,id, GetFavgroup());
            LoadFav();
        }


        private void B_del_fav_Click(object sender, RoutedEventArgs e)
        {
            //foreach (NavObject temp in lv_fav.GetSelectItems())
            //    ini.DeleteFav(temp.Typ, temp.ID);

            ini.DeleteFavs(lv_fav.GetSelectItems(),GetFavgroup());
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
                    if (ini.Data["Settings"].ContainsKey(cb.Name))
                        cb.IsChecked = Boolean.Parse(ini.Data["Settings"][cb.Name]);
                }
            }
            Loghandler.Add("Einstellungen geladen:" + ini.GetFilename());
        }

        private void AddMandant(object sender, RoutedEventArgs e)
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
                ini.AddFavs(w.getSelectionList(), GetFavgroup());
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

        private void AddProfil(object sender, RoutedEventArgs e)
        {
            AddDialog w = new AddDialog("Profil");

            w.ShowDialog();
            ini.AddProfile(w.INPUT);
            Load_Profil();
        }

        private void DelProfil(object sender, RoutedEventArgs e)
        {
            ini.DelProfile(cb_profil.Text);
            Load_Profil();
        }

        private void Mi_set_exe_path_Click(object sender, RoutedEventArgs e)
        {
            string inidir = "";
            if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Dynamics NAV\110\RoleTailored Client\"))
            {
                inidir = @"C:\Program Files (x86)\Microsoft Dynamics NAV\110\RoleTailored Client\";
                ini.SetSettings("DefaultService", "DynamicsNAV110");
            }
            else if (Directory.Exists(@"C:\Program Files (x86)\Microsoft Dynamics NAV\100\RoleTailored Client\"))
            {
                inidir = @"C:\Program Files (x86)\Microsoft Dynamics NAV\100\RoleTailored Client\";
                ini.SetSettings("DefaultService", "DynamicsNAV100");
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Client (Microsoft.Dynamics.Nav.Client.exe)|Microsoft.Dynamics.Nav.Client.exe",
                InitialDirectory = inidir
            };

            if (dialog.ShowDialog() == true)
            {
                ini.SetExePath(dialog.FileName);
                MessageBox.Show("Pfad zur exe wurde geändert. Bitte Anwendung neu starten!",
                    "Neustart erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);
                Loghandler.Add("Pfad zur exe gesetzt: " + dialog.FileName);
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
            SetDefaultOptions();
            //MessageBoxResult res;
            ProcessStartInfo upd = new ProcessStartInfo("Updater.exe");

            //res = MessageBox.Show("Wollen sie die letzte stabile Version installieren? \n (Nein = Beta Version installieren)", "Update", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            var client = new GitHubClient(new ProductHeaderValue(ini.GetSetting("upd_user")));

            var releases = await client.Repository.Release.GetAll(
                ini.GetSetting("upd_user"),
                ini.GetSetting("upd_repository"));

            string updateuri = null;
            string version = "";
            string whatsnew = "";

            foreach (var temp in releases) {
                if (temp.Prerelease & ini.GetSetting("upd_beta") == "false")
                    continue;   //Pre Release überstringen

                updateuri = temp.Assets[0].BrowserDownloadUrl;

                if (updateuri == ini.GetSetting("updateuri")) {
                     //Wenn die installierte Version mit der gefunden zusammen passt Ende
                    updateuri = null;
                    if (ini.GetSetting("upd_beta") == "true")
                        MessageBox.Show("Du besitzt bereits die neuerste Beta Version: " + temp.TagName, "Kein neues Update vorhanden", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show("Du besitzt bereits die neuerste Version: " + temp.TagName, "Kein neues Update vorhanden", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;   //Wenn Updateversion gleich ist   
                }

                if (String.IsNullOrEmpty(updateuri))
                    continue;   //Wenn kein Release vorhanden ist

                version = temp.TagName;
                whatsnew = temp.Body;
                break;          //
            }

            if (updateuri == null)
                return;

            MessageBoxResult res = MessageBox.Show("Neue Version " + version + " vorhanden, soll von " + updateuri + " heruntergeladen und installiert werden?\n\r"
                + "\n\r Das ist neu in der Version: \n\r"
                + whatsnew,
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
            SetDefaultOptions();
            Options opt = new Options(ini);
            opt.ShowDialog();
        }


        private void SetDefaultOptions()
        {
            if (ini.GetSetting("upd_user") == null)
                ini.SetSettings("upd_user", "wallmi");

            if (ini.GetSetting("upd_repository") == null)
                ini.SetSettings("upd_repository", "start-dynamics-nav");

            if (ini.GetSetting("upd_beta") == null)
                ini.SetSettings("upd_beta", "false");
        }

        private void StartNAVFilter(object sender, RoutedEventArgs e)
        {
            Process.Start("NAVFilterConvert.exe");

        }

        private void B_AddFavGroup_Click(object sender, RoutedEventArgs e)
        {
            AddDialog w = new AddDialog("Favoriten Gruppe");
            w.ShowDialog();
            ini.AddFavGroup(w.INPUT);
            LoadFavGroup();
        }

        private void B_DelFavGroup_Click(object sender, RoutedEventArgs e)
        {
            ini.DelFavGroup(cb_favGroup.SelectedValue.ToString());
            LoadFavGroup();
        }

        private bool IsFavGroup()
        {
            if (ini.GetSetting("favgroup") == "true")
                return true;

            return false;
        }

        private string GetFavgroup()
        {
            if (cb_favGroup.SelectedValue == null)
                return null;

            if (cb_favGroup.SelectedValue.ToString() == "NOFAVGROUP")
                return null;

            return cb_favGroup.SelectedValue.ToString();
        }


        private void Cb_favGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFav();
        }

        private void Cb_objektTyp_DropDownClosed(object sender, EventArgs e)
        {
            GetObjectName();
        }
    }
}

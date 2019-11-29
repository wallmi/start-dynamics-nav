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
        enum StartTyp {Nav,Session }
      
        public MainWindow()
        {
            InitializeComponent();
            handler = new ObjectHandler(OBJECTFILE);
            ini = new IniHandler(INIFILE, OBJECTFILE);

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

                cb_profil.SelectedItem = "<kein Profil>";
                cbo_config.IsChecked = false;
                cbo_debug.IsChecked = false;

                cbo_config.ToolTip = tooltip;
                cbo_debug.ToolTip = tooltip;
                cb_profil.ToolTip = tooltip;
                b_start_session_list.ToolTip = tooltip;
            }
        }

        void Load()
        {
            ini.Reload();
            Load_Server();
            Load_Profil();
            LoadFav();
            LoadSettings();
        }

        void LoadFav()
        {
            List<NavObject> favs = ini.GetFav();
            lv_fav.SetItems(favs);
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
            tbl_serveradresse.Text = ini.GetServerAdress(cb_server.SelectedItem.ToString());
        }

        private void Cb_objektTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetObjectName();
            if(cb_objektTyp.SelectedItem.ToString() == "None") { 
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
                exe = ini.data["Settings"]["ExePath"] + " ";
            
            
            if (cbo_schow_startstring.IsChecked == true)
            {
                MessageBox.Show(exe + " " +param,"Start String",MessageBoxButton.OK,MessageBoxImage.Information);
            }

            Process.Start(exe,param);
        }

        string GetStartParameter(StartTyp st)
        {
            string Navbase = "\"dynamicsnav://";
            string ServerAdress = ini.GetServerAdress(cb_server.Text);
            string Mandant = "/" + cb_mandant.Text;
            ObjectTypes ob = NavObjects.GetObj(cb_objektTyp.Text);
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
                    case ObjectTypes.Page: ObjectStart += "runpage?page=" + tx_objId.Text; break;
                    case ObjectTypes.Table: ObjectStart += "runtable?table=" + tx_objId.Text; break;
                    case ObjectTypes.Report: ObjectStart += "runreport?report=" + tx_objId.Text; break;
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
            AddServer w = new AddServer(INIFILE, OBJECTFILE);
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
            foreach (NavObject temp in lv_fav.GetSelectItems())
                ini.DeleteFav(temp.Typ, temp.ID);

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
                    ini.data["Settings"][cb.Name] = cb.Text;
                }
                else if (temp.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)temp;
                    ini.data["Settings"][tb.Name] = tb.Text;
                }
                else if (temp.GetType() == typeof(CheckBox))
                {
                    CheckBox cb = (CheckBox)temp;
                    ini.data["Settings"][cb.Name] = cb.IsChecked.ToString();
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
                    cb.Text = ini.data["Settings"][cb.Name];
                }
                else if (temp.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)temp;
                    tb.Text = ini.data["Settings"][tb.Name];
                }
                else if (temp.GetType() == typeof(CheckBox))
                {
                    CheckBox cb = (CheckBox)temp;
                    if(ini.data["Settings"].ContainsKey(cb.Name))
                        cb.IsChecked = Boolean.Parse(ini.data["Settings"][cb.Name]);
                }
            }

            /*
             * Ersetzt durch List toSave
            cb_server.Text = ini.data["Settings"]["cb_server"];
            cb_mandant.Text = ini.data["Settings"]["cb_mandant"];
            cb_objektTyp.Text = ini.data["Settings"]["cb_objektTyp"];
            tx_objId.Text = ini.data["Settings"]["tx_objId"];

            if (ini.data["Settings"]["cbo_config"] == "True")
                cbo_config.IsChecked = true;

            if (ini.data["Settings"]["cbo_debug"] == "True")
                cbo_debug.IsChecked = true;
                */
        }

        
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            AddMandant w = new AddMandant(INIFILE,OBJECTFILE);
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
            Load();
            tb_status.Text = "Einstellung in " + INIFILE + " gespeichert! " + DateTime.Now.ToString("HH:mm:ss");
        }

        private void B_getId_Click(object sender, RoutedEventArgs e)
        {
            GetObject w = new GetObject(handler);

            if (((Button)sender).Name == b_get_favs.Name)
            {
                w.ShowDialog3();
                foreach (NavObject temp in w.retList)
                {
                    ini.AddFav(temp.Typ, temp.ID);
                }
                LoadFav();
            }
            else
            {
                w.ShowDialog2();
                tx_objId.Text = w.retGet.ID.ToString();
                cb_objektTyp.Text = w.retGet.Typ.ToString();
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            AddProfile w = new AddProfile(INIFILE, OBJECTFILE);
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
            }
        }
    }
}

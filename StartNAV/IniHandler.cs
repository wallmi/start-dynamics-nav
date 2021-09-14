using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.IO;

using IniParser;
using IniParser.Model;
using StartNAV.Model;

namespace StartNAV
{

    public class IniHandler
    {
        readonly FileIniDataParser ini = new FileIniDataParser();
        //readonly ObjectHandler handler;
        public IniData Data { set; get; }
        String FILENAME;
        String OBJFILE;
        enum KeyType { key,value };
        readonly Log LOG;
        readonly Dictionary<string, string> data = new Dictionary<string, string>();
        bool loaded = false;
        
        static private string DEFAULT_INSTANCE = ":7046/DynamicsNAV100_IMP";
        private readonly char SEPERATOR = '|';  //Trennung Zwischen Namen und Version, evtl. mal als Einstellung
        public bool withversion { set; get; } = false;

        public IniHandler(string file, string objfile, Log log)
        {
            SetFile(file);
            LOG = log;
            if (File.Exists(FILENAME))
                LoadFile();
            else
            {
                CreateFile();
                LoadFile();
                SetToDefault();
            }
            //handler = new ObjectHandler(objfile);
            LoadFromFile(objfile);
        }
        /// <summary>
        /// Ladet den Inhalt .ini Datei
        /// </summary>
        void LoadFile()
        {
            if (File.Exists(FILENAME))
                Data = ini.ReadFile(FILENAME);
        }
        /// <summary>
        /// Erzeugt ini File
        /// </summary>
        void CreateFile()
        {
            FileStream fs;
            fs = File.Create(FILENAME);
            fs.Close();
            LOG.Add("Setting Datei erstellt: " + FILENAME);
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetToDefault()
        {
            Data["Server"]["TEST"] = "wynvt02wn.bene.cc:7046/DynamicsNAV100";
            Data["Server"]["TEST-2"] = "wynvt03wn.bene.cc:7046/DynamicsNAV100";
            Data["Server"]["TEST (IMP)"] = "wynvt02wn.bene.cc:7046/DynamicsNAV100_IMP";

            Data["Mandanten-TEST"]["BAT_Zielsystem"] = "1";
            Data["Mandanten-TEST"]["BAT_Test"] = "1";

            Data["Mandanten-TEST-2"]["BAT_Test"] = "1";
            Data["Mandanten-TEST-2"]["BAT_Zielsystem"] = "1";

            LOG.Add("Standard Einstellungen gesetzt");
        }

        void SetFile(string FileName)
        {
            FILENAME = FileName;
        }

        public void MakeBackup()
        {
            File.Copy(FILENAME, FILENAME + ".bak",true);
            LOG.Add("Backup erstellt:" + FILENAME);
        }

        /// <summary>
        /// Löscht die ini Datei wenn vorhanden und Lädt erneut
        /// </summary>
        public string DelFile()
        {
            if (File.Exists(FILENAME))
            {
                File.Delete(FILENAME);
                LOG.Add("Datei gelöscht:" + FILENAME);
            }

            LoadFile();
            return FILENAME;
        }

        /// <summary>
        /// Schreibt IniDate in File
        /// </summary>
        public void WriteFile()
        {
            ini.WriteFile(FILENAME, Data);
            LOG.Add("Speichere Einstellungen in Datei:" + FILENAME);
        }
        public void Save()
        {
            WriteFile();
        }

        public List<String> GetServer()
        {
            return SectionStringList("Server",KeyType.key);
        }

        public List<String> GetProfile()
        {
            List<String> ret = SectionStringList("Profile", KeyType.key);
            ret.Add("<kein Profil>");
            return ret;
        }

        public List<String> GetMandanten(string server)
        {
            return SectionStringList("Mandanten-" + server, KeyType.key);
        }
        /// <summary>
        /// Inhalt einer Section als Liste
        /// </summary>
        /// <param name="sectionName">Name der Section</param>
        /// <param name="t">(KeyType) key or value</param>
        /// <returns></returns>
        List<String> SectionStringList(string sectionName, KeyType t)
        {
            return SectionStringList(sectionName, t, "");
        }

        public string GetServerAdress(string servername)
        {
            return Data["Server"][servername];
        }

        /// <summary>
        /// Gibt eine Liste mit Einträge zurück
        /// </summary>
        /// <param name="sectionName">SectionName</param>
        /// <param name="t">keyName für alle Namen, keyValue für alle Wert</param>
        /// <param name="keyPra">Nur Werte mit dem Angegeben Präfix ausgeben</param>
        /// <returns></returns>
        List<String> SectionStringList(string sectionName, KeyType t, string keyPra)
        {
            return SectionStringList(sectionName, t, keyPra,'_');
        }

        /// <summary>
        /// Gibt eine Liste mit Einträge zurück
        /// </summary>
        /// <param name="sectionName">SectionName</param>
        /// <param name="t">keyName für alle Namen, keyValue für alle Wert</param>
        /// <param name="keyPra">Nur Werte mit dem Angegeben Präfix ausgeben</param>
        /// <param name="sepChar">Trennzeichen für keyPra</param>
        /// <returns></returns>
        List<String> SectionStringList(string sectionName, KeyType t, string keyPra,char sepChar)
        {
            List<String> list = new List<string>();
            SectionData section = Data.Sections.GetSectionData(sectionName);

            if (section is null)
                return list;

            if (section.Keys.Count > 0)
                foreach (KeyData key in section.Keys)
                    if (key.KeyName.Split(sepChar)[0] == keyPra || String.IsNullOrEmpty(keyPra))
                    {
                        if (t == KeyType.key)
                            list.Add(key.KeyName);
                        if (t == KeyType.value)
                            list.Add(key.Value);
                    }
            return list;
        }

        public void AddServer (string name, string adress)
        {
            Data["Server"][name] = adress;
        }

        public void AddProfile(string name)
        {
            if (String.IsNullOrEmpty(name))
                return;

            Data["Profile"][name] = "1";
        }

        public void DelServer(string name)
        {
            Data["Server"].RemoveKey(name);
        }

        public void DelProfile(string name)
        {
            Data["Profile"].RemoveKey(name);
        }

        public void DelMandant(string server, string name)
        {
            Data["Mandanten-" + server].RemoveKey(name);
         
        }

        public void AddMandant(string server, string mandant)
        {
            Data["Mandanten-"+server][mandant] = "1";
        }

        //Add Favs
        public void AddFav(string type, int id, string group)
        {
            if (String.IsNullOrEmpty(group))
                Data["Fav"][FavName(type, id)] = "1";
            else
                Data["Fav_"+group][FavName(type, id)] = "1";
        }

        public void AddFav(string type, int id)
        {
            AddFav(type, id, null);
        }

        public void AddFav(ObjectType type, int id)
        {
            AddFav(NavObjects.GetName(type), id);
        }

        public void AddFavs(List<NavObject> favs,string group)
        {
            if (favs == null)
                return;

            foreach (NavObject temp in favs)
            {
                AddFav(temp.Typ.ToString(), temp.ID, group);
            }
        }

        //Delete Favs
        public void DeleteFav(string type, int id, string group)
        {
            string section = "Fav";
            if (!String.IsNullOrEmpty(group))
                section += "_" + group;

            if ((Data[section].ContainsKey(FavName(type, id))))
                Data[section].RemoveKey(FavName(type, id));
        }
        public void DeleteFav(string type, int id)
        {
            DeleteFav(type, id,null);
        }

        public void DeleteFav(ObjectType type, int id)
        {
            DeleteFav(NavObjects.GetName(type), id);
        }

        public void DeleteFavs(List<NavObject> favs, string group)
        {
            if (favs == null)
                return;
            foreach (NavObject temp in favs)
            {
                DeleteFav(temp.Typ.ToString(), temp.ID, group);
            }
        }

        public void DeleteFavs(List<NavObject> favs)
        {
            DeleteFavs(favs, null);
        }

       static string FavName(string type, int id)
        {
            return type + "_" + id.ToString();
        }

        public List<NavObject> GetFav(string group)
        {
            string section = "Fav";
            if (!String.IsNullOrEmpty(group))
                section += "_" + group;

            List<NavObject> ret = new List<NavObject>();
            if (Data.Sections[section] is null)
                return ret;
            foreach (KeyData key in Data.Sections[section])
            {
                string[] keyVal = key.KeyName.Split('_');
                if (keyVal.Length == 2)
                {
                    int id = int.Parse(keyVal[1]);
                    ret.Add(new NavObject(keyVal[0], id,
                        GetObjName(id, NavObjects.GetObj(keyVal[0])),
                        GetVersion(id, NavObjects.GetObj(keyVal[0]))
                    )
                    );
                }
            }
            return ret;

        }

        public List<NavObject> GetFav()
        {
            return GetFav(null);
        }

        //Fav Group
        public void AddFavGroup(string groupname)
        {
            if (String.IsNullOrEmpty(groupname))
                return;

            Data["FavGroup"][groupname] = "1";
        }

        public void DelFavGroup(string groupname)
        {
            DeleteFavs(GetFav(groupname),groupname);
            Data["FavGroup"].RemoveKey(groupname);
        }

        public List<string> GetFavGroups()
        {
            if (Data.Sections["FavGroup"] == null)
                Data.Sections.AddSection("FavGroup");

            List<string> ret = new List<string>();
            foreach (KeyData key in Data.Sections["FavGroup"])
            {
                ret.Add(key.KeyName);
            }

            return ret;
        }

        public void SetExePath(string path)
        {
            SetSettings("ExePath", path);
        }

        public void SetSettings(string key, string value)
        {
            Data["Settings"][key] = value;
        }
        public void SetSettings(string key, string value, bool savetofile)
        {
            SetSettings(key, value);
            if (savetofile)
                Save();
        }

        public string GetSetting(string key)
        {
            return Data["Settings"][key];
        }

        public bool CheckExe()
        {
            if (!Data["Settings"].ContainsKey("ExePath"))
                return false;

            if (String.IsNullOrEmpty(Data["Settings"]["ExePath"]))
                return false;

            if (File.Exists(Data["Settings"]["ExePath"]))
                return true;

            return false;
        }

        public string GetFilename()
        {
            return FILENAME;
        }


        //Added ObjectHandler
        public void LoadFromFile(string file)
        {
            if (!File.Exists(file))
            {
                MessageBox.Show("Die Datei " + file + " konnte nicht geladen werden.\n Es werden keine Objektnamen angezeigt!",
                    "Datei nicht gefunden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            OBJFILE = file;
            LoadFromFile();
        }

        public void LoadFromFile()
        {
            string[] FileLines = File.ReadAllLines(OBJFILE);

            if (data.Count > 0)
                data.Clear();

            foreach (string temp in FileLines)
            {
                string temp1 = temp.Replace("\"", "");
                string[] Line = temp1.Split(',');
                int id = -1;
                try
                {
                    id = Int32.Parse(Line[0]);
                }
                catch
                {

                }
                if (id == -1) continue;
                if (Line.Length >= 4)
                {
                    data.Add(Line[0] + "_" + Line[1], Line[2] + SEPERATOR + Version(Line));
                    withversion = true;
                }
                else if (Line.Length == 3)
                    data.Add(Line[0] + "_" + Line[1], Line[2]);

            }
            loaded = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true wenn Datei geladen wurde, ansonst false</returns>
        public bool IsLoaded()
        {
            return loaded;
        }
        /// <summary>
        /// Nav Objektnamen ermitteln
        /// </summary>
        /// <param name="objID">ID des Objectes</param>
        /// <param name="t">Der Typ des Objectes</param>
        /// <returns>Objektname</returns>
        public string GetObjName(int objID, ObjectType t)
        {
            string key = GetKeyVal(objID, t);
            if (data.ContainsKey(key))
                return data[key].Split(SEPERATOR)[0];

            return "";
        }
        /// <summary>
        /// Nav Version ermitteln
        /// </summary>
        /// <param name="objID">ID des Objectes</param>
        /// <param name="t">Der Typ des Objectes</param>
        /// <returns>Version</returns>
        public string GetVersion(int objID, ObjectType t)
        {
            string key = GetKeyVal(objID, t);
            if (data.ContainsKey(key))
                if (data[key].Contains(SEPERATOR))
                    return data[key].Split(SEPERATOR)[1];

            return "";
        }
        /// <summary>
        /// Nav Objektnamen ermitteln
        /// </summary>
        /// <param name="objID">ID des Objectes</param>
        /// <param name="objName">Der Typ des Objectes</param>
        /// <returns>Objektname</returns>
        public string GetObjName(int objID, string objName)
        {
            return GetObjName(objID, NavObjects.GetObj(objName));
        }

        string GetKeyVal(int objID, ObjectType t)
        {
            int id = (int)t;
            return id.ToString() + "_" + objID.ToString();
        }

        static public List<String> GetObjs()
        {
            List<string> ret = new List<string>();
            foreach (string temp in Enum.GetNames(typeof(ObjectType)))
                ret.Add(temp);

            return ret;
        }

        public List<NavObject> GetObjectNames()
        {
            List<NavObject> ret = new List<NavObject>();
            List<ObjectType> types = new List<ObjectType>
            {
                ObjectType.Page,
                ObjectType.Report,
                ObjectType.Table,
                ObjectType.Codeunit,
            };

            foreach (KeyValuePair<string, string> temp in data)
            {
                string[] t = temp.Key.Split('_');

                try
                {
                    int id = Int32.Parse(t[1]);
                    ObjectType Typ = NavObjects.GetObj(t[0]);
                    if (types.Contains(Typ))
                        ret.Add(new NavObject(t[0], id, temp.Value.Split(SEPERATOR)[0], temp.Value.Split(SEPERATOR)[1]));
                }
                catch
                {

                }
            }
            return ret;
        }
        /// <summary>
        /// Wenn der Server keine Port Angabe enthält dann wird der Default port + Instanz übergeben
        /// </summary>
        /// <param name="servername"></param>
        /// <returns></returns>
        static public String CheckServerString(String servername)
        {
            if (String.IsNullOrEmpty(servername)) return "";

            if (!servername.Contains(":"))
                return servername + DEFAULT_INSTANCE;

            return servername;
        }

        static private String Version(String[] data)
        {
            string ret = "";
            int i = 1;
            foreach (string temp in data)
            {
                if (i > 4)
                    ret += ",";
                if (i > 3)
                    ret += temp;

                i++;
            }
            return ret;
        }


        #region wiki IniFileParser
        //Iterate through all the sections
        /*foreach (SectionData section in data.Sections)
        {
           Console.WriteLine("[" + section.Name + "]");
   
           //Iterate through all the keys in the current section
           //printing the values
           foreach(KeyData key in section.Keys)
              Console.WriteLine(key.Name + " = " + key.Value);
        }

        public String GetObjName(int id, NavObject objtyp)
        {
            return no.GetObjName(id, objtyp);
        }

        public String GetObjName(int id, string objname)
        {
            return no.GetObjName(id, objname);
        }
        */
        #endregion
    }
}

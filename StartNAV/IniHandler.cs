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

    class IniHandler
    {
        readonly FileIniDataParser ini = new FileIniDataParser();
        readonly ObjectHandler handler;
        public IniData data;
        String FILENAME;
        enum KeyType { key,value };

        public IniHandler(string file, string objfile)
        {
            SetFile(file);
            if (File.Exists(FILENAME))
                LoadFile();
            else
            {
                CreateFile();
                SetToDefault();
            }
            handler = new ObjectHandler(objfile);
        }
        /// <summary>
        /// Lädt ini oder erstellt wenn nicht vorhande
        /// </summary>
        public void Reload()
        {
            if (File.Exists(FILENAME))
                LoadFile();
            else
            {
                CreateFile();
                SetToDefault();
            }
        }

        /// <summary>
        /// Ladet den Inhalt .ini Datei
        /// </summary>
        void LoadFile()
        {
            if (File.Exists(FILENAME))
                data = ini.ReadFile(FILENAME);
            else
                data = new IniData();
        }
        /// <summary>
        /// Erzeugt ini File
        /// </summary>
        void CreateFile()
        {
            FileStream fs;
            fs = File.Create(FILENAME);
            fs.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        public void SetToDefault()
        {
            data = ini.ReadFile(FILENAME);

            data["Server"]["TEST"] = "wynvt02wn.bene.cc:7046/DynamicsNAV100";
            data["Server"]["TEST-2"] = "wynvt03wn.bene.cc:7046/DynamicsNAV100";
            data["Server"]["TEST (IMP)"] = "wynvt02wn.bene.cc:7046/DynamicsNAV100_IMP";

            data["Mandanten-TEST"]["BAT_Zielsystem"] = "1";
            data["Mandanten-TEST"]["BAT_Test"] = "1";

            data["Mandanten-TEST-2"]["BAT_Test"] = "1";
            data["Mandanten-TEST-2"]["BAT_Zielsystem"] = "1";

            WriteFile();
            LoadFile();
        }

        void SetFile(string FileName)
        {
            FILENAME = FileName;
        }

        public void MakeBackup()
        {
            File.Copy(FILENAME, FILENAME + ".bak",true);
        }

        /// <summary>
        /// Löscht die ini Datei wenn vorhanden und Lädt erneut
        /// </summary>
        public void DelFile()
        {
            if (File.Exists(FILENAME))
                File.Delete(FILENAME);

            LoadFile();
        }

        /// <summary>
        /// Schreibt IniDate in File
        /// </summary>
        public void WriteFile()
        {
            ini.WriteFile(FILENAME, data);
        }


        public List<String> GetServer()
        {
            LoadFile();
            return SectionStringList("Server",KeyType.key);
        }

        public List<String> GetMandanten(string server)
        {
            LoadFile();
            return SectionStringList("Mandanten-" + server, KeyType.key);
        }

        List<String> SectionStringList(string sectionName, KeyType t)
        {
            return SectionStringList(sectionName, t, "");
        }

        public string GetServerAdress(string servername)
        {
            return data["Server"][servername];
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
            SectionData section = data.Sections.GetSectionData(sectionName);

            if (section is null)
                return list;

            if (section.Keys.Count > 0)
                foreach (KeyData key in section.Keys)
                    if (key.KeyName.Split(sepChar)[0] == keyPra || keyPra == "")
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
            data["Server"][name] = adress;
            WriteFile();
        }

        public void DelServer(string name)
        {
            data["Server"].RemoveKey(name);
            WriteFile();
        }

        public void DelMandant(string server, string name)
        {
            data["Mandanten-" + server].RemoveKey(name);
            WriteFile();            
        }

        public void AddMandant(string server, string mandant)
        {
            data["Mandanten-"+server][mandant] = "1";
            WriteFile();
        }

        public void AddFav(string type, int id)
        {
            data["Fav"][FavName(type,id)] = "1";
            WriteFile();
        }

        public void AddFav(ObjectTypes type, int id)
        {
            AddFav(NavObjects.GetName(type), id);
        }

        public bool DeleteFav(ObjectTypes type, int id)
        {
            return DeleteFav(NavObjects.GetName(type), id);
        }

        public bool DeleteFav(string type,int id)
        {
            if (!(data["Fav"].ContainsKey(FavName(type, id))))
                return false;

            data["Fav"].RemoveKey(FavName(type, id));
           
            WriteFile();
            return true;

        }

        string FavName(string type, int id)
        {
            return type + "_" + id.ToString();
        }

        public List<NavObject> GetFav()
        {
            List<NavObject> ret = new List<NavObject>();
            if (data.Sections["Fav"] is null)
                return ret;
            foreach (KeyData key in data.Sections["Fav"])
            {
                string[] keyVal = key.KeyName.Split('_');
                if (keyVal.Length == 2)
                {
                    int id = Int32.Parse(keyVal[1]);
                    ret.Add(new NavObject (keyVal[0], id,  handler.GetObjName(id, NavObjects.GetObj(keyVal[0]))
                    )
                    );
                }
            }
            return ret;
        }

        public void SaveSetting(string key, string value)
        {
            data["Settings"][key] = value;
            WriteFile();
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

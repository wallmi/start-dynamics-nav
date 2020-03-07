using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;

using StartNAV.Model;
using System.Windows;

namespace StartNAV
{
    /// <summary>
    /// Klasse für NAV Objecte und Namen
    /// Aus einer csv Datei Objektnamen einlsen
    /// und dannach zu verfügung Stellen
    /// </summary>
    public class ObjectHandler
    {
        readonly Dictionary<string, string> data = new Dictionary<string, string>();
        bool loaded = false;
        private string FILENAME = "";
        private readonly char SEPERATOR = '|';  //Trennung Zwischen Namen und Version, evtl. mal als Einstellung
        public bool withversion { set; get; } = false;

        public ObjectHandler () {
            
        }
        /// <summary>
        /// Initialisieren mit Daten
        /// </summary>
        /// <param name="file">csv File zum einlesen der Objektnamen</param>
        public ObjectHandler(string file)
        {
            LoadFromFile(file);
        }
        /// <summary>
        /// Lade Objektnamen
        /// </summary>
        /// <param name="file">csv File zum einlsen der Objektnamen</param>
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

            FILENAME = file;
            LoadFromFile();
        }

        public void LoadFromFile()
        {
            string[] FileLines = File.ReadAllLines(FILENAME);

            if (data.Count() > 0)
                data.Clear();

            foreach (string temp in FileLines)
            {

                string[] Line = temp.Split(',');
                int id = -1;
                try
                {
                    id = Int32.Parse(Line[0]);
                }
                catch { }
                if (id == -1) continue;
                if (Line.Length >= 4) { 
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
        public string GetObjName(int objID, ObjectTypes t)
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
        public string GetVersion(int objID, ObjectTypes t)
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
        public string GetObjName (int objID, string objName)
        {
            return GetObjName(objID, NavObjects.GetObj(objName));
        }

        string GetKeyVal(int objID, ObjectTypes t)
        {
            int id = (int)t;
            return id.ToString() + "_" + objID.ToString();
        }
        
        public List<String> GetObjs()
        {
            List<string> ret = new List<string>();
            foreach (string temp in Enum.GetNames(typeof(ObjectTypes)))
                ret.Add(temp);

            return ret;
        }

        public List<NavObject> GetObjectNames()
        {
            List<NavObject> ret = new List<NavObject>();
            List<ObjectTypes> types = new List<ObjectTypes>
            {
                ObjectTypes.Page,
                ObjectTypes.Report,
                ObjectTypes.Table
            };

            foreach (KeyValuePair<string, string> temp in data) {
                string[] t = temp.Key.Split('_');
                try { 
                int id = Int32.Parse(t[1]);
                ObjectTypes Typ = NavObjects.GetObj(t[0]);
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
        public String CheckServerString(String servername)
        {
            if (!servername.Contains(":"))
                return servername + ":7046/DynamicsNAV100_IMP";

            return servername;
        }

        private String Version(String[] data)
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
    }
}

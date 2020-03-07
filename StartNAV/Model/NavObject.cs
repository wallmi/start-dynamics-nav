using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartNAV.Model
{
    public class NavObject : ICloneable
    {
        public ObjectType Typ { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public NavObject()
        {
            Typ = ObjectType.None;
            ID = -1;
            Name = "NULL";
            Version = "NULL";
        }

        public NavObject(string _typ, int _ID, string _Name)
        {
            Typ = NavObjects.GetObj(_typ);
            ID = _ID;
            Name = _Name;
            Version = "";
        }

        public NavObject(string _typ, int _ID, string _Name, string _Version)
        {
            Typ = NavObjects.GetObj(_typ);
            ID = _ID;
            Name = _Name;
            Version = _Version;
        }

        public NavObject(ObjectType _typ, int _ID, string _Name)
        {
            Typ = _typ;
            ID = _ID;
            Name = _Name;
            Version = "";
        }

        public NavObject (ObjectType _typ, int _ID, string _Name, string _Version)
        {
            Typ = _typ;
            ID = _ID;
            Name = _Name;
            Version = _Version;
        }

        public string GetKey()
        {
            return Typ + "_" + ID.ToString();
        }
    }
}

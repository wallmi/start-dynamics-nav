using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartNAV.Model
{
    public class NavObject : ICloneable
    {
        public ObjectTypes Typ { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public NavObject()
        {
            Typ = ObjectTypes.None;
            ID = -1;
            Name = "NULL";
        }

        public NavObject(string _typ, int _ID, string _Name)
        {
            Typ = NavObjects.GetObj(_typ);
            ID = _ID;
            Name = _Name;
        }

        public NavObject (ObjectTypes _typ, int _ID, string _Name)
        {
            Typ = _typ;
            ID = _ID;
            Name = _Name;
        }
    }
}

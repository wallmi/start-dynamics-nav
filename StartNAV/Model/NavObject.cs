using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartNAV.Model
{
    public enum ObjectTypes {None = -1, Table = 0, Report = 3, Page = 8 };

    public class NavObjects
    {
        public static int GetId(string objectName)
        {
            return (int)Enum.Parse(typeof(ObjectTypes), objectName);
        }

        public static int GetId(ObjectTypes o)
        {
            return (int)o;
        }

        public static string GetName(int id)
        {
            return Enum.GetName(typeof(ObjectTypes), id);
        }
        public static string GetName(string id)
        {
            return Enum.GetName(typeof(ObjectTypes), Int32.Parse(id));
        }
        public static string GetName(ObjectTypes o)
        {
            return Enum.GetName(typeof(ObjectTypes), o);
        }

        public static ObjectTypes GetObj(string name)
        {
            return (ObjectTypes)Enum.Parse(typeof(ObjectTypes), name);
        }

        public static ObjectTypes GetObj(int id)
        {
            return GetObj(GetName(id));
        }
        public static string[] GetObjectNames() {
            return Enum.GetNames(typeof(ObjectTypes));
        }


        public class NavObject: ICloneable
        {
            public string Typ { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}

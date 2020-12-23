using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartNAV.Model
{
    public enum ObjectType {None = -1, Table = 0, Report = 3, Codeunit = 5 ,Page = 8 };

    static public class NavObjects
    {
        public static int GetId(string objectName)
        {
            return (int)Enum.Parse(typeof(ObjectType), objectName);
        }

        public static int GetId(ObjectType o)
        {
            return (int)o;
        }

        public static string GetName(int id)
        {
            return Enum.GetName(typeof(ObjectType), id);
        }
        public static string GetName(string id)
        {
            return Enum.GetName(typeof(ObjectType), Int32.Parse(id));
        }
        public static string GetName(ObjectType o)
        {
            return Enum.GetName(typeof(ObjectType), o);
        }

        public static ObjectType GetObj(string name)
        {
            return (ObjectType)Enum.Parse(typeof(ObjectType), name);
        }

        public static ObjectType GetObj(int id)
        {
            return GetObj(GetName(id));
        }

        public static string[] GetObjectNames() {
            return Enum.GetNames(typeof(ObjectType));
        }
    }
}

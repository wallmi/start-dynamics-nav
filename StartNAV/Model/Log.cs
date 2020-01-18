using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StartNAV.Model
{
    public class Log
    {
        private readonly List<LogEntry> entries = new List<LogEntry>();
        private readonly String FILENAME = "";
        private int CurrentID = 0;
        private readonly TextBlock TB;

        public Log (TextBlock tb)
        {
            FILENAME = AppDomain.CurrentDomain.BaseDirectory 
                + "Log_" 
                + DateTime.Now.ToString("dd.MM.yyyy_HHmmss") 
                + ".log";
            File.Create(FILENAME);
            TB = tb;
            Add("New Logfile created:" + Path.GetFileName(FILENAME));
        }

        public void Add(string text)
        {
            entries.Add(new LogEntry(text,CurrentID));
            CurrentID++;
            TB.Text = text;
        }
        /// <summary>
        /// Schreibt Log Einträge in eine Datei und löscht diese
        /// </summary>
        public void SaveToFile()
        {
            if (FILENAME == "") return;
            foreach(LogEntry temp in entries)
                File.AppendAllText(FILENAME, temp.ToString()+"\n");
            entries.Clear();    
            
        }
        public List<LogEntry> GetEntries()
        {
            return entries;
        }
    }

    public class LogEntry : IEquatable<LogEntry>
    {
        private int ID { get; set; }

        public string Text { get; set; }
        public DateTime DateTime { get; set; }

        public string DateTimeString { get { return DateTime.ToString("HH:mm:ss");  } }

        public LogEntry(string text, int id)
        {
            DateTime = DateTime.Now;
            Text = text;
            ID = id;
        }

        public bool Equals(LogEntry other)
        {
            if (other == null) return false;
            return (this.ID.Equals(other.ID));
        }

        public override string ToString()
        {
            return DateTime.ToString() + "|" + Text;
        }
    }
}

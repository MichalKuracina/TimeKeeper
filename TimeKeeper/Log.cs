using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TimeKeeper
{
    public class Log
    {
        private string _location;
        private string _baseDirectory;
        private string _logDirectory;
        private string _logFullPath;

        public Log()
        {
            _logDirectory = "C:\\IBOTS\\TimeKeeper\\History";
            _logFullPath = _logDirectory + "\\History.txt";
        }

        public void Create()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (!File.Exists(_logFullPath))
            {
                File.Create(_logFullPath);
            }
        }

        public string Get()
        {
            bool found = false;
            string result = "00:00:00";
            var lines = File.ReadLines(_logFullPath);

            foreach (var line in lines)
            {
                if (line.Contains(DateTime.Now.ToString("dd.MM.yyyy")))
                {
                    result = line.Substring(11);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                using (StreamWriter sw = new StreamWriter(_logFullPath, true))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd.MM.yyyy") + " " + result);
                }
            }

            return result;
        }

        public void Write(string input)
        {
            var lines = File.ReadLines(_logFullPath);
            string myLine = "";

            foreach (var line in lines)
            {
                if (line.Contains(DateTime.Now.ToString("dd.MM.yyyy")))
                {
                    myLine = line.Substring(0);
                    break;
                }
            }

            string myText = File.ReadAllText(_logFullPath);
            myText = myText.Replace(myLine, DateTime.Now.ToString("dd.MM.yyyy") + " " + input);
            File.WriteAllText(_logFullPath, myText);

        }

    }
}

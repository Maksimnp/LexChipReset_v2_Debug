using System;
using System.IO;

namespace LexChipReset_v2.NewFolder
{
    public abstract class LogBase
    {
        public abstract void Log(string Messsage);
    }

    public class Logger : LogBase
    {
        private string CurrentDirectory
        {
            get; set;
        }
        private string FilePath
        {
            get; set;
        }
        private string FileName
        {
            get; set;
        }
        public Logger()
        {
            CurrentDirectory = Directory.GetCurrentDirectory();
            FileName = "Log.txt";
            FilePath = CurrentDirectory + "/" + FileName;

        }
        public override void Log(string Messsage)
        {
            using StreamWriter streamwrt = File.AppendText(FilePath);
            streamwrt.Write("\nLog:");
            streamwrt.WriteLine("{0} : {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToLongDateString()) ;
            streamwrt.WriteLine("{0}", Messsage);
            streamwrt.WriteLine("#############################################################");
        }
    }
  }
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Mail;
using System.Net;
namespace bank
{
    class FileExplorer
    {
        public string path = System.IO.Directory.GetCurrentDirectory();
        private char os;
        public FileExplorer()
        {
            FileExplorer fe = this;
            fe.os = fe.osDivider();
        }
        public bool fileExists(string filename)
        {
            return File.Exists(path + os + filename);
        }
        // Logging exceptions
        public void log(string exception, string stacktrace)
        {
            string now = $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}";
            string error = now + "\n-----\n" + exception + "\n" + stacktrace + "\n-----";
            if (File.Exists(path + os + "error.log") == false)
            {
                createFile("error.log", error);
            }
            else
            {
                appendText("error.log", error);
            }
        }
        // This generates the necessary character that divides directories, depending on OS. Reference https://docs.microsoft.com/en-us/dotnet/api/system.platformid?view=netcore-3.1 for more information on platformID
        private char osDivider()
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            if (pid == PlatformID.Unix)
            {
                return '/';
            }
            return '\\';
        }
        // This method compresses any string[] to be a single string. Why? Well, because account files are passed as string arrays. Used for writing to files.
        public string compress(string[] s)
        {
            string temp = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                {
                    temp = s[i];
                }
                else temp += "\n" + s[i];
            }
            return temp;
        }
        // Creates a file based on given filename and contents. Requires file extension, i.e. file.txt
        public void createFile(string filename, string contents)
        {
            StreamWriter sw = new StreamWriter(this.path + this.os + filename, false);
            sw.WriteLine(contents);
            sw.Close(); // Each time a new file is created, it starts the FileStream object which occupies the newly created file. The file needs to be closed by the Filestream object before it can be accessed again. This is same with StreamWriter.
        }
        // Deletes the specified file.
        public void deleteFile(string filename)
        {
            File.Delete(this.path + this.os + filename);
        }
        // Adds a single line to a file. Does not generate a trailing \n
        public void appendText(string filename, string contents)
        {
            StreamWriter sw = File.AppendText(this.path + this.os + filename);
            sw.WriteLine(contents);
            sw.Close();
        }
        // Returns the contents of a file as a string[]
        public string[] getFileContents(String filename)
        {
            string p = this.path + os + filename;
            string[] s = System.IO.File.ReadAllLines(p);
            return s;
        }
        // Returns the contents of a directory based on the key provided. Used to scan for account numbers. Does not work on Windows due to directory permission issues.
        public string[] getMatchingFiles(String key)
        {
            Validator v = new Validator();
            string[] s = System.IO.Directory.GetFiles(this.path);
            string[] t = new string[s.Length];
            int index = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (v.validate(key, s[i]) == true)
                {
                    t[index] = s[i];
                    index++;
                }
            }
            t = t.Where(a => a != null).ToArray();// Clean null array data
            for (int i = 0; i < t.Length; i++)
            {
                FileInfo fi = new FileInfo(t[i]); // Clean filepath and file extension
                t[i] = fi.Name;
                t[i] = Path.GetFileNameWithoutExtension(t[i]);
            }
            return t;
        }
        // Converts account numbers into an int array. Shouldn't really be in FileExplorer, but didn't warrant moving to Bank().
        public int[] convertIntArray(string[] s)
        {
            int[] array = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                try
                {
                    if (s[i] != "0" && String.IsNullOrWhiteSpace(s[i]) == false) array[i] = Convert.ToInt32(s[i]); // Removes errant 0 with null value entries
                }
                catch (Exception e)
                {
                    log(e.ToString(), e.StackTrace);
                }
            }
            return array;
        }
    }
}

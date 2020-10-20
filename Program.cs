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
    class Program
    {
        static void Main(string[] args)
        {
            // The following code is the actual application start
            startup();
            Bank bank = new Bank();
            // The following code is for running tests
            // Tests tests = new Tests();
        }
        // Creates a new login.txt if none is found. Also displays a message if Windows is detected.
        private static void startup()
        {
            FileExplorer fe = new FileExplorer();
            DisplayMenu display = new DisplayMenu();
            if (File.Exists("login.txt") == false)
            {
                display.interfaceHeader("Welcome to simple banking app");
                string path = System.IO.Directory.GetCurrentDirectory();
                string filepath = path + "/login.txt";
                display.interfaceMessage("error", "'login.txt' cannot be found in " + path);
                Console.Clear();
                display.interfaceHeader("info");
                bool choice = display.interfaceModalYesNo("Do you want to create a new 'login.txt' in " + filepath + "?", 5);
                if (choice)
                { // Depending on the time of day, weather, r% and OS, folder permissions will fail.
                    try
                    {
                        string loginContents = "guest|1234\nuser1|password123\nuser2|321password";
                        fe.createFile("login.txt", loginContents);
                        Console.Clear();
                        display.interfaceHeader("info");
                        display.interfaceMessage("Success", "A new login.txt has been created.");
                        Console.Clear();
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        display.interfaceHeader("info");
                        display.interfaceMessage("System.UnauthorizedAccessException", "This application does not have access to the working directory. Please navigate to " + path + " " + "and create a new 'login.txt'.");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.Clear();
                    display.interfaceHeader("info");
                    display.interfaceMessage("Option cancelled", "Please make a login.txt file manually at " + filepath);
                    Environment.Exit(0);
                }
            }
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            if (pid != PlatformID.Unix)
            {
                display.interfaceHeader("info");
                display.interfaceMessage("Windows OS detected", "There is an unresolved file directory issue related to Windows sandboxing. To resolve this, an accounts.txt will be automatically generated for tracking accounts between sessions. If there are still errors, please run this program on a Linux-based OS instead.");
            }
        }
    }
}
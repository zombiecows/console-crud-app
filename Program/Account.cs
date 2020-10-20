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
    class Account
    {
        private string firstName;
        private string lastName;
        private string address;
        private int phoneNo;
        private string email;
        private int accountNumber;
        private int balance;
        private List<Activity> activities = new List<Activity>();
        // Overload for creating new accounts
        public Account(int[] accounts, string fn, string ln, string addr, int phone, string email, FileExplorer fe)
        {
            FirstName = fn;
            LastName = ln;
            Address = addr;
            PhoneNo = phone;
            Email = email;
            Balance = 0;
            AccountNumber = generateAccountNumber(accounts);
            string fileName = accountNumber + ".txt";
            fn = "First Name|" + fn;
            ln = "Last Name|" + ln;
            addr = "Address|" + addr;
            string phonen = "Phone Number|" + phone.ToString();
            email = "Email Address|" + email;
            string accn = "Account Number|" + accountNumber.ToString();
            string balance = "Balance|0";
            string[] fileContents = new string[7]{
                fn,ln,addr,phonen,email,accn,balance
            };
            string c = fe.compress(fileContents);
            try
            {
                // Create file with account details
                fe.createFile(fileName, c);
            }
            catch (Exception e)
            {
                fe.log(e.ToString(), e.StackTrace);
                throw new Exception();
            }
        }
        // Overload for initialising existing accounts
        public Account(int number, FileExplorer fe, Validator v)
        {
            string[] contents = fe.getFileContents(number.ToString() + ".txt");
            for (int i = 0; i < contents.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        FirstName = v.deleteFormLabel(contents[i]);
                        break;
                    case 1:
                        LastName = v.deleteFormLabel(contents[i]);
                        break;
                    case 2:
                        Address = v.deleteFormLabel(contents[i]);
                        break;
                    case 3:
                        PhoneNo = Convert.ToInt32(v.deleteFormLabel(contents[i]));
                        break;
                    case 4:
                        Email = v.deleteFormLabel(contents[i]);
                        break;
                    case 5:
                        AccountNumber = Convert.ToInt32(v.deleteFormLabel(contents[i]));
                        break;
                    case 6:
                        Balance = Convert.ToInt32(v.deleteFormLabel(contents[i]));
                        break;
                    default:
                        this.Activities.Add(parseActivity(contents[i]));
                        break;
                }
            }
        }
        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string Address { get => address; set => address = value; }
        public int PhoneNo { get => phoneNo; set => phoneNo = value; }
        public string Email { get => email; set => email = value; }
        public int AccountNumber { get => accountNumber; set => accountNumber = value; }
        public int Balance { get => balance; set => balance = value; }
        internal List<Activity> Activities { get => activities; set => activities = value; }
        // Generates a unique account number for a new account using the list of existing account numbers. Limits new numbers between 100000 and 20000000 (to prevent long numbers while meeting 6-8 digit requirement).
        private int generateAccountNumber(int[] accounts)
        {
            var random = new Random();
            int number = random.Next(100000, 99999999);
            var checkUnique = checkUniqueAccountNumber(accounts, number);
            while (checkUniqueAccountNumber(accounts, number) != true)
            {
                number = random.Next(100000, 99999999);
            }
            return number;
        }
        // Checks whether the account number is unique. Used for generating new account numbers. Returns true if unique.
        private bool checkUniqueAccountNumber(int[] accounts, int number)
        {
            if (accounts != null) // Prevents a null
            {
                int check = 0;
                foreach (int v in accounts)
                {
                    if (v == number) check++;
                }
                if (check != 0) return false;
                else return true;
            }
            else return true;
        }
        // Parses account activity from file using string[] to transform into Activity objects.
        private Activity parseActivity(string input)
        {
            string[] parsed = input.Split('|');
            DateTime parsedDate = DateTime.Now;
            string type = "type";
            int amount = 0, balance = 0;
            for (int i = 0; i < parsed.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        DateTime.TryParseExact(parsed[i], "dd.MM.yyyy", null, DateTimeStyles.None, out parsedDate);
                        break;
                    case 1:
                        type = parsed[i];
                        break;
                    case 2:
                        amount = Convert.ToInt32(parsed[i]);
                        break;
                    case 3:
                        balance = Convert.ToInt32(parsed[i]);
                        break;
                    default: break;
                }
            }
            Activity activity = new Activity(parsedDate, type, amount, balance);
            return activity;
        }
        // Account deposit function
        public Activity deposit(int amount)
        {
            balance += amount;
            Activity activity = createActivity("Deposit", amount, balance);
            return activity;
        }
        // Account withdraw function
        public Activity withdraw(int amount)
        {
            if (amount <= balance)
            {
                balance -= amount;
                Activity activity = createActivity("Withdraw", amount, balance);
                return activity;
            }
            else return null;
        }
        // Creates a new activity and adds it to existing array
        private Activity createActivity(string type, int amount, int balance)
        {
            DateTime date = DateTime.Now; // Takes today's date
            Activity activity = new Activity(date, type, amount, balance);
            Activities.Add(activity);
            return activity;
        }
        // Gets the file output of each activity for exporting to an account file.
        private List<String> getFileActivities()
        {
            List<String> contents = new List<String>();
            foreach (Activity activity in Activities)
            {
                contents.Add(activity.FileOutput()); // Gets the file-ready output of each Activity.
            }
            return contents;
        }
        public string emailOutput(FileExplorer fe)
        {
            this.Activities.Sort((x, y) => x.Date.CompareTo(y.Date)); // Compares and sorts by Datetime in each Activity. This alone is the biggest reason why Activity is an object.
            this.Activities.Reverse(); // Reverses the List to show in descending order by date
            List<string> details = new List<String>();
            foreach (Activity activity in Activities)
            {
                details.Add(activity.EmailOutput()); // Adds the email-ready output of each Activity to a List
            }
            string[] input = details.ToArray(); // Convert list into static array because that's what the function needs
            var size = 5;
            if (input.Length < size) size = input.Length; // The if statement is to prevent a null exception when printing null objects.
            Array.Resize<string>(ref input, size); // Trim array size to 5 items or less.
            string[] accountDetails = {
                "Account Number: "  + AccountNumber,
                "Account Balance: $" + Balance,
                "Name: " + FirstName + " " + LastName,
                "Address: " + Address,
                "Phone: " + PhoneNo,
                "Email: " + Email,
            };
            string part1 = fe.compress(accountDetails);
            string part2 = fe.compress(input);
            string final = "Account Details\n" + part1 + "\nLatest 5 Actions\n" + part2 + "\nEmailed from SimpleBankingApp.";
            return final;
        }
        // Used to resolve conflicting newline handling between sample files and internally generated files by rebuilding an account file.
        internal void rebuild(FileExplorer fe)
        {
            string fileName = AccountNumber.ToString() + ".txt";
            string fn = "First Name|" + FirstName;
            string ln = "Last Name|" + LastName;
            string addr = "Address|" + Address;
            string phonen = "Phone Number|" + PhoneNo.ToString();
            string email = "Email Address|" + Email;
            string accn = "Account Number|" + AccountNumber.ToString();
            string balance = "Balance|" + Balance.ToString();
            List<String> contents = new List<String> { fn, ln, addr, phonen, email, accn, balance };
            contents.AddRange(getFileActivities()); // Merges lists
            string c = fe.compress(contents.ToArray()); // Compresses a converted List into a single file-ready string
            try
            {
                fe.deleteFile(fileName); // Delete existing file
                fe.createFile(fileName, c); // Create file with account details
            }
            catch (Exception a)
            {
                fe.log(a.ToString(), a.StackTrace); // Logs error
            }
        }
    }
}

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
    class Bank
    {
        // Naming scheme: view() methods are controllers gluing the interface to other methods. Other methods are generally functions or mixed controller/functions.
        private Validator validator = new Validator(); // class for handing user input validation
        private DisplayMenu display = new DisplayMenu(); // class for interface elements
        private FileExplorer fe = new FileExplorer(); // file manipulation class
        private string[] users; // contains valid user credentials from login.txt
        private int[] accounts; // contains an int[] of existing account numbers by reading the directory
        // Main constructor
        public Bank()
        {
            this.users = fe.getFileContents("login.txt");
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            createAccountsArray();
            viewLogin();
        }
        // Overload for tests
        public Bank(bool b)
        {
            this.users = fe.getFileContents("login.txt");
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            createAccountsArray();
        }
        public void viewLogin()
        {
            string header = "login";
            string message = "Wrong username or password. If you have just generated a new login.txt, please exit the application and add your preferred credentials.";
            string prompt = "Do you want to continue logging in?";
            string[] s = display.interfaceBankLogin();
            while (checkLoginCredentials(s) != true)
            {
                Console.Clear();
                bool persistent = display.interfaceBankErrorContinue(header, message, prompt);
                if (persistent == true)
                {
                    Console.Clear();
                    s = display.interfaceBankLogin();
                } else exit();
            }
            viewMainMenu(login(s));
        }
        public void viewLogout(User user)
        {
            Console.Clear();
            display.interfaceHeader("logout");
            bool confirm = display.interfaceModalYesNo("Are you sure you want to log out?", 5);
            if (confirm == true)
            {
                logout(user);
                viewLogin();
            }
            else viewMainMenu(user);
        }
        // Bypass reading directory for files for Windows users
        private void CreateReadAccountsTxt()
        {
            string file = "accounts.txt";
            if (fe.fileExists(file) == false)
            {
                fe.createFile(file, ""); // Create empty file.
                accounts = new int[0];
            }
            else accounts = fe.convertIntArray(fe.getFileContents(file));
        }
        // Bypass reading directory for files for Windows users. Note that Windows will block updating files, so this method remakes files only.
        private void UpdateAccountsTxt(int number, bool add)
        {
            string file = "accounts.txt";
            if (add == true)
            {
                List<int> accTemp = new List<int>();
                foreach(int i in accounts)
                {
                    if (i.ToString() != "0") accTemp.Add(i);
                }
                accTemp.Add(number);
                accounts = accTemp.ToArray(); // Converts back to int[]
                fe.deleteFile(file); // Deletes existing file
                List<string> temp = new List<string>();
                foreach(int i in accounts)
                {
                    temp.Add(i.ToString());
                }
                string[] contents = temp.ToArray();
                fe.createFile(file, fe.compress(contents)); // Remakes file
            }
            else
            { // Delete account
                List<int> temp = new List<int>(); // New List to hold updated accounts array
                for (int i = 0; i < this.accounts.Length; i++)
                {
                    if (this.accounts[i].ToString() != number.ToString())
                    {
                        temp.Add(this.accounts[i]);
                    }
                }
                this.accounts = temp.ToArray(); // Update accounts
                string[] contents = new string[temp.Count]; // Convert List to string[]
                if (temp.Count > 0)
                {
                    for (int i = 0; i < temp.Count; i++)
                    {
                        contents[i] = temp[i].ToString();
                    }
                }
                fe.deleteFile(file);
                fe.createFile(file, fe.compress(contents)); // Remake file
            }
        }
        private User login(string[] data)
        {
            User user;
            bool auth = checkLoginCredentials(data);
            if (auth == true) return user = new User(data[0], data[1], this);
            else return null;
        }
        private bool checkLoginCredentials(string[] data)
        {
            // Compile the user credentials into a matchable string
            string userDetails = "";
            bool auth = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0)
                {
                    userDetails += "|";
                }
                userDetails += data[i];
            }
            // Match user credentials in plaintext
            foreach (string s in users)
            {
                if (userDetails == s)
                {
                    auth = true;
                }
            }
            return auth;
        }
        public void viewExit(User user)
        {
            display.interfaceHeader("exit");
            if (display.interfaceModalYesNo("Do you want to exit the program?", 5) == false)
            {
                viewMainMenu(user); // Returns the user to main menu
            }
            else // exits the program
            {
                logout(user);
                exit();
            }
        }
        private void exit()
        {
            Console.Clear();
            Console.ResetColor(); // Included in case Console.Color options are used.
            Environment.Exit(0);
        }
        public void viewMainMenu(User user)
        {
            display.interfaceBankMainMenu(this, user);
        }
        public void viewCreateNewAccount(User user)
        {
            string[] data = display.interfaceBankCreateAccount();
            viewCreateNewAccountConfirm(user, data);
        }
        public void viewDeleteAccount(User user)
        {
            string header = "Delete account";
            string prompt = "Continue searching for an account to delete?";
            try
            {
                Account account = loopSearchAccount(user, header, prompt);
                Console.Clear();
                bool confirm = display.interfaceBankDeleteAccount(account, header);
                if (confirm != true) viewMainMenu(user);
                else
                {
                    try
                    {
                        deleteAccount(account); // Deletes account file only
                        updateAccountsArray(account.AccountNumber, false); // Rebuild accounts array
                        account = null;
                        Console.Clear();
                        viewMainMenu(user);
                    }
                    catch (Exception e)
                    {
                        fe.log(e.ToString(), e.StackTrace);
                        Console.Clear();
                        display.interfaceHeader(header);
                        display.interfaceMessage("error", "Failed to delete the account. Please delete " + account.AccountNumber + ".txt in the program directory manually.");
                        Console.Clear();
                        viewMainMenu(user);
                    }
                }
            } catch (Exception e) // There is a non-zero possibility of loopSearchAccount() returning a null on Windows
            {
                fe.log(e.ToString(), e.StackTrace);
                Console.Clear();
                display.interfaceHeader(header);
                display.interfaceMessage("error", "An error occurred while searching for the account and the account failed to load. Please check that the account file is not corrupted. For Windows users, please check that accounts.txt contains the correct account number.");
                Console.Clear();
                viewMainMenu(user);
            }
        }
        public void viewCreateNewAccountConfirm(User user, string[] data)
        {
            bool persistent = true;
            string header = "create a new account";
            string prompt = "Do you want to continue creating an account?";
            while (persistent != false)
            {
                if (validator.validateEmptyFields(data) == true)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "Fields cannot be empty.", prompt);
                    if (persistent != false)
                    {
                        Console.Clear();
                        viewCreateNewAccount(user);
                    }
                    break;
                }
                else if (validator.validate("phone", data[3]) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "Phone numbers can only have numbers between 1-10 digits.", prompt);
                    if (persistent != false)
                    {
                        Console.Clear();
                        viewCreateNewAccount(user);
                    }
                    break;
                }
                else if (validator.validate("email", data[4]) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "Email addresses require a '@' symbol and email domains must be either gmail.com or hotmail.com.", prompt);
                    if (persistent != false)
                    {
                        Console.Clear();
                        viewCreateNewAccount(user);
                    }
                    break;
                }
                else
                {
                    Console.Clear();
                    while (display.interfaceBankCreateAccountConfirm(data) == false)
                    {
                        // Doesn't use the error message continue window because it's not an error message.
                        if (display.interfaceModalYesNo(prompt, 21) == false)
                        {
                            persistent = false;
                            break;
                        }
                        Console.Clear();
                        viewCreateNewAccount(user);
                    }
                    // Brute force a break to resolve hanging error window.
                    if (persistent == false)
                    {
                        break;
                    }
                    try
                    {
                        string fn = "";
                        string ln = "";
                        string a = "";
                        string e = "";
                        int p = 11111111;
                        for (int i = 0; i < data.Length; i++)
                        {
                            switch (i)
                            {
                                case 0: fn = data[i]; break;
                                case 1: ln = data[i]; break;
                                case 2: a = data[i]; break;
                                case 3: Int32.TryParse(data[i], out p); break;
                                case 4: e = data[i]; break;
                            }
                        }
                        Account acc = createNewAccount(user, fn, ln, a, p, e); // can return null
                        Console.Clear();
                        display.interfaceHeader(header);
                        display.interfaceMessage("success", "You have successfully created a new account. The new account ID is " + acc.AccountNumber + ". View a statement of this account to email yourself a copy.");
                        viewMainMenu(user);
                    }
                    catch (Exception e)
                    {
                        // This is a catch-all response for Account constructor failure
                        fe.log(e.ToString(), e.StackTrace); // Writes to error.log
                        display.interfaceHeader(header);
                        display.interfaceMessage("error", "An error occurred while creating a new account. Please make sure you are running this program with read/write permissions to the program's folder.");
                        viewMainMenu(user); // Redirects to main menu
                    }
                }
            }
            Console.Clear();
            viewMainMenu(user);
        }
        // Creates a new account with given details and returns an Account object.
        private Account createNewAccount(User user, string firstName, string lastName, string address, int phoneNumber, string email)
        {
            try
            {
                // Need to write to file
                Account account = new Account(this.accounts, firstName, lastName, address, phoneNumber, email, fe);
                updateAccountsArray(account.AccountNumber, true); // Rebuild accounts array
                return account;
            }
            catch (Exception e)
            {
                // There is an Exception pathing in the Account constructor due to file permission errors. This will redirect that error with a null response. 
                fe.log(e.ToString(), e.StackTrace);
                return null;
            }
        }
        // Search for account interface initialisation
        public void viewSearch(User user)
        {
            string header = "search an account";
            string prompt = "Check another account?";
            Console.Clear();
            Account account = loopSearchAccount(user, header, prompt);
            if (account != null)
            {
                Console.Clear();
                viewSearchSuccess(user, account);
            }
            else
            {
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
        }
        // Searches the stored account numbers and returns true if the account number exists.
        private bool searchExistingAccounts(int number)
        {
            foreach (int i in this.accounts)
            {
                if (number == i) return true;
            }
            return false;
        }
        // Redirects the user based on their choice on viewing the searched account details. Prevents the need for a loop in viewSearch()
        private void viewSearchSuccess(User user, Account account)
        {
            if (display.interfaceBankSearchSuccess(account) == false)
            {
                viewMainMenu(user);
            }
            else viewSearch(user);
        }
        // Creates an array with all the existing accounts based on the file directory. There is an unhandled issue with Win10 sandboxing and reading directory files that hasn't been isolated.
        private void updateAccountsArray(int number, bool add)
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            if (pid == PlatformID.Unix) accounts = fe.convertIntArray(fe.getMatchingFiles("accNo"));
            else UpdateAccountsTxt(number, add);
        }
        private void createAccountsArray()
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            if (pid == PlatformID.Unix) accounts = fe.convertIntArray(fe.getMatchingFiles("accNo"));
            else CreateReadAccountsTxt();
        }
        public void viewDeposit(User user)
        {
            string header = "deposit";
            string prompt = "Check another account?";
            string depositPrompt = "Continue with a new deposit?";
            Console.Clear();
            Account account = loopSearchAccount(user, header, prompt);
            if (account != null)
            {
                int amount = deposit(user, account, header, depositPrompt);
                if (amount != -1)
                {
                    Activity deposit = account.deposit(amount);
                    account.rebuild(fe);
                    Console.Clear();
                    display.interfaceHeader(header);
                    display.interfaceMessage("success", "You've successfully deposited $" + deposit.Amount + " into account " + account.AccountNumber + ".");
                }
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
            else
            {
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
        }
        public void viewWithdraw(User user)
        {
            string header = "withdraw";
            string prompt = "Check another account?";
            string withdrawPrompt = "Continue with a new withdrawal?";
            Console.Clear();
            Account account = loopSearchAccount(user, header, prompt);
            if (account != null)
            {
                int amount = withdraw(user, account, header, withdrawPrompt);
                if (amount != -1)
                {
                    Activity withdraw = account.withdraw(amount);
                    account.rebuild(fe);
                    Console.Clear();
                    display.interfaceHeader(header);
                    display.interfaceMessage("success", "You've successfully withdrawn $" + withdraw.Amount + " from account " + account.AccountNumber + ".");
                }
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
            else
            {
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
        }
        // Loops the user until they succeed a deposit or cancel
        private int deposit(User user, Account account, string header, string prompt)
        {
            bool persistent = true;
            while (persistent != false)
            {
                string input = display.interfaceBankDepositWithdraw(account, header)[0];
                // Sanitize string and convert to int
                if (String.IsNullOrEmpty(input))
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "A deposit cannot be empty.", prompt);
                }
                else if (validator.validate("int", input) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "A deposit must consist of numbers and it cannot exceed 9 digits.", prompt);
                }
                else
                {
                    try
                    {
                        int userInput = Convert.ToInt32(input);
                        if (userInput < 1)
                        {
                            Console.Clear();
                            persistent = display.interfaceBankErrorContinue(header, "A deposit cannot be 0 or less.", prompt);
                        }
                        else return userInput;
                    }
                    catch (Exception e)
                    {
                        fe.log(e.ToString(), e.StackTrace);
                        persistent = display.interfaceBankErrorContinue(header, "An error occurred. Check the log for more information.", prompt);
                    }
                }
            }
            return -1;
        }
        // Loops the user until they succeed a withdrawal or cancel
        private int withdraw(User user, Account account, string header, string prompt)
        {
            bool persistent = true;
            while (persistent != false)
            {
                string input = display.interfaceBankDepositWithdraw(account, header)[0];
                // Sanitize string and convert to int
                if (String.IsNullOrEmpty(input))
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "A withdrawal cannot be empty.", prompt);
                }
                else if (validator.validate("int", input) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "A withdrawal must consist of numbers and it cannot exceed 9 digits.", prompt);
                }
                else
                {
                    try
                    {
                        int userInput = Convert.ToInt32(input);
                        if (userInput > account.Balance)
                        {
                            Console.Clear();
                            persistent = display.interfaceBankErrorContinue(header, "A withdrawal cannot be more than the account balance. The current account balance is $" + account.Balance + ".", prompt);
                        }
                        else if (userInput < 1)
                        {
                            Console.Clear();
                            persistent = display.interfaceBankErrorContinue(header, "A withdrawal cannot be 0 or less.", prompt);
                        }
                        else return userInput;
                    }
                    catch (Exception e)
                    {
                        fe.log(e.ToString(), e.StackTrace);
                        persistent = display.interfaceBankErrorContinue(header, "An error occurred. Check the log for more information.", prompt);
                    }
                }
            }
            return -1;
        }
        // This forces a loop until the user cancels the search for an account. Used for search, deposit, withdraw, statement and delete.
        private Account loopSearchAccount(User user, string header, string prompt)
        {
            bool persistent = true;
            while (persistent != false)
            {
                string input = display.interfaceBankSearch(header);
                if (String.IsNullOrEmpty(input))
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "The account number cannot be empty. An account number is 6-8 digits long.", prompt);
                }
                else if (validator.validate("accountNumber", input) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "The account number could not be validated. An account number is 6-8 digits long.", prompt);
                }
                else
                {
                    try
                    {
                        int userInput = Convert.ToInt32(input);
                        if (searchExistingAccounts(userInput) == false)
                        {
                            Console.Clear();
                            persistent = display.interfaceBankErrorContinue(header, "This account number does not exist.", prompt);
                        }
                        else
                        {
                            Account account = new Account(userInput, fe, validator);
                            return account;
                        }
                    }
                    catch (Exception e)
                    {
                        fe.log(e.ToString(), e.StackTrace);
                        persistent = display.interfaceBankErrorContinue(header, "An error occurred. Check the log for more information.", prompt);
                        return null;
                    }
                }
            }
            return null;
        }
        public void viewStatement(User user)
        {
            string header = "statement";
            string prompt = "Check another account?";
            bool email = false;
            Console.Clear();
            Account account = loopSearchAccount(user, header, prompt);
            if (account != null)
            {
                Console.Clear();
                email = display.interfaceBankStatement(account);
                if (email == true) this.viewEmailStatement(user, account);
                display.interfaceBankMainMenu(this, user);
            }
            else
            {
                Console.Clear();
                display.interfaceBankMainMenu(this, user);
            }
        }
        private string[] loopEmail(Account account, string header)
        {
            bool persistent = true;
            string prompt = "Continue with the email?";
            while (persistent != false)
            {
                string[] auth = display.interfaceBankStatementEmail(account, header);
                if (String.IsNullOrEmpty(auth[0]) || String.IsNullOrEmpty(auth[1]))
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "The email or password field cannot be empty.", prompt);
                }
                else if (validator.validate("email", auth[0]) == false)
                {
                    Console.Clear();
                    persistent = display.interfaceBankErrorContinue(header, "The email must contain @. Accepted domains are gmail.com or hotmail.com.", prompt);
                }
                else return auth;
            }
            return null;
        }
        /* Main email function/controller.
        Most of the following code was superficially tested (interface flow only) because OAuth wasn't implemented (it requires additional website integration, which is outside the scope of this project). 
        The provided email credentials will act as sender and send a copy of the email statement to the receiver, which is the email specified in the account statement. This prevents the need to bake in plaintext account details, which is a major security issue.
        Mailkit was not used because it requires OAuth for Gmail. Sendgrid could be a viable alternative (commercial email API) if a project website with separate domain was set up. */
        private void viewEmailStatement(User user, Account account)
        {
            string header = "Email bank statement";
            string[] auth = new string[2];
            string _origin = "";
            string _password = "";
            try
            {
                auth = loopEmail(account, header); // Looping form to obtain email credentials from user. Used for sending. 
                _origin = auth[0];
                _password = auth[1];
            }
            catch (Exception)
            { // Catch null return
                viewMainMenu(user);
            }
            string recipient = account.FirstName + " " + account.LastName;
            string subject = "SimpleBankingApp - Bank Statement for " + account.AccountNumber;
            string body = account.emailOutput(fe);
            string[] outgoingServerList = {
                "smtp.gmail.com", // Gmail smtp server
                "smtp.live.com", // Hotmail smtp server
                "smtp-mail.outlook.com", 
            };
            string smtp = "";
            int smtpType = validator.validateEmailType(_origin);
            switch (smtpType)
            {
                case 0: smtp = outgoingServerList[0]; break;
                case 1: smtp = outgoingServerList[1]; break;
                case 2: smtp = outgoingServerList[2]; break;
            };
            try
            {
                MailMessage email = new MailMessage(_origin, account.Email)
                {
                    From = new MailAddress(account.Email),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };
                var client = new SmtpClient(smtp)
                {
                    Port = 587, // General port for TLS email connections
                    Credentials = new NetworkCredential(_origin, _password),
                    EnableSsl = true,
                };
                client.Send(email);
                display.interfaceHeader(header);
                display.interfaceMessage("success", "Please check the inbox of " + account.Email + "for a copy of the statement.");
            }
            catch (System.Net.Mail.SmtpException snmse)
            {
                fe.log(snmse.ToString(), snmse.StackTrace);
                display.interfaceHeader(header);
                display.interfaceMessage("error", "Failed to authenticate using your credentials. You may have typed in a wrong password or need an app-specific password to continue. For Gmail users, please check this answer: https://stackoverflow.com/a/26709761");
            }
            catch (Exception e)
            {
                fe.log(e.ToString(), e.StackTrace);
                try
                {
                    if (smtpType == 2)
                    { // Alternative method for Outlook
                        MailMessage email = new MailMessage(_origin, account.Email)
                        {
                            From = new MailAddress(account.Email),
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = false,
                        };
                        smtp = outgoingServerList[3]; // Switch to smtp.office365.com
                        var client = new SmtpClient(smtp)
                        {
                            Port = 587,
                            Credentials = new NetworkCredential(_origin, _password),
                            EnableSsl = true,
                        };
                        client.Send(email);
                        display.interfaceHeader(header);
                        display.interfaceMessage("success", "Please check the inbox of " + account.Email + "for a copy of the statement.");
                    }
                    else
                    {
                        display.interfaceHeader(header);
                        display.interfaceMessage("error", "An error occurred while emailing. Refer to the error log for more details.");
                    }
                }
                catch (System.Net.Mail.SmtpException snmse)
                {
                    fe.log(snmse.ToString(), snmse.StackTrace);
                    display.interfaceHeader(header);
                    display.interfaceMessage("error", "Failed to authenticate using your provided credentials. You may have typed in a wrong password or need an app-specific password to continue. For Gmail users, please check this answer: https://stackoverflow.com/a/26709761");
                }
                catch (Exception f)
                {
                    fe.log(f.ToString(), f.StackTrace);
                    display.interfaceHeader(header);
                    display.interfaceMessage("error", "An error occurred while emailing. Refer to the error log for more details.");
                }
            }
        }
        // Deletes the account file.
        private void deleteAccount(Account account)
        {
            fe.deleteFile(account.AccountNumber + ".txt");
        }
        // Logs out the user.
        private void logout(User user)
        {
            user = null;
        }
    }
}

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
    class DisplayMenu
    {
        /* 
        Manual configuration settings:
        - width = width of the rendered window
        - height = not used, but shows the max height of the rendered window
        - Box symbols:
            * topLeftCorner
            * topRightCorner
            * bottomLeftCorner
            * bottomRightCorner
            * horizontalChar
            * verticalChar
        - lineBorderPadding = padding between the box's border and the box's contents. Includes the border itself. Singular, so needs to be multiplied by 2 to calculate the total padding in a line.

        Naming scheme: 
        - All line- prefixed methods use char[] for constructing and manipulating interface lines
        - All print- prefixed methods turn lines into strings for interface()
        - All interface- prefixed methods use print(lines) and directly interact with view() methods in Bank class
        - PadRight/PadLeft isn't used because this setup allows for custom configuration.
        */
        private int width = 80;
        private int height = 20;
        private char topLeftCorner = '┌';
        private char topRightCorner = '┐';
        private char bottomLeftCorner = '└';
        private char bottomRightCorner = '┘';
        private char horizontalChar = '─';
        private char verticalChar = '│';
        private int lineBorderPadding = 6;
        public DisplayMenu()
        {
            DisplayMenu displayMenu = this;
            displayMenu.width = width;
            displayMenu.height = height;
        }
        // Renders a spaced full caps header of a window.
        private void printHeaderMessage(string header)
        {
            // Render the top line
            char[] line = new char[this.width];
            for (int j = 0; j < width; j++)
            {
                if (j == 0) line[j] = topLeftCorner;
                else if (j == width - 1) line[j] = topRightCorner;
                else line[j] = horizontalChar;
            }
            string topline = new string(line);
            string m = new string(lineHorizontalSeparator());
            string p = new string(lineFullPadding());
            string h = new string(lineTextMessage(header, true));
            // Display header
            Console.WriteLine(topline + '\n' + p + '\n' + h + '\n' +  p + '\n' + m);
        }
        // Create a full line that separates sections of the interface. Used for joining the header to the body
        private char[] lineHorizontalSeparator()
        {
            char[] line = new char[this.width];
            for (int i = 0; i < this.width; i++)
            {
                if (i == 0 || i == line.Length - 1) line[i] = verticalChar;
                else line[i] = horizontalChar; 
            }
            return line;
        }
        // Creates a padded line with no content inside.
        private char[] lineFullPadding()
        {
            char[] line = new char[this.width];
            for (int i = 0; i < this.width; i++)
            {
                if (i == 0 || i == line.Length - 1) line[i] = verticalChar;
                else line[i] = ' ';
            }
            return line;
        }
        // Creates the bottom line of an interface window
        private char[] lineBottom()
        {
            char[] line = new char[this.width];
            for (int j = 0; j < width; j++)
            {
                if (j == 0) line[j] = bottomLeftCorner;
                else if (j == width - 1) line[j] = bottomRightCorner;
                else line[j] = horizontalChar;
            }
            return line;
        }
        // Creates one half of padded spacing based on the direction of the border needed
        private char[] lineConvertPaddingWithDirection(char[] c, int direction)
        {
            // direction refers to which side of the border needs to be lined
            // left = 0
            // right = char[].Length - 1
            for (int i = 0; i < c.Length; i++)
            {
                if (i == direction)
                {
                    c[i] = verticalChar;
                }
                else
                {
                    c[i] = ' ';
                }
            }
            return c;
        }
        // Creates a centered message in a char[] array
        private char[] lineTextMessage(string s, bool caps)
        {
            // Calculate leftover width
            int left;
            int right;
            int tempWidth = this.width - s.Length;
            // Check if leftover width is even or odd for centering text
            if (tempWidth % 2 == 0)
            {
                // Even number
                int v = tempWidth / 2;
                left = v;
                right = v;
            }
            else // Odd number
            {
                double v = tempWidth / 2;
                left = Convert.ToInt32(Math.Floor(v));
                right = Convert.ToInt32(Math.Floor(v)) + 1;
            }
            // Turn the leftover width into padding. Initialise the char[] with the desired widths first
            char[] leftPadding = new char[1];
            char[] rightPadding = new char[1];
            if (left > 0 && right > 0)
            {
                leftPadding = new char[left];
                rightPadding = new char[right];
            }
            string l = new string(lineConvertPaddingWithDirection(leftPadding, 0));
            string r = new string(lineConvertPaddingWithDirection(rightPadding, rightPadding.Length - 1));
            if (caps == true)
            {
                string final = l + s.ToUpper() + r;
                return final.ToCharArray();
            }
            else
            {
                string final = l + s + r;
                return final.ToCharArray();
            }
        }
        // Creates a Form label. Very reusable.
        private char[] lineFormLabel(string s)
        {
            char[] c = new char[lineBorderPadding];
            for (int i = 0; i < c.Length; i++)
            {
                if (i == 0)
                {
                    c[i] = verticalChar;
                }
                else c[i] = ' ';
            }
            char[] d = new char[this.width - s.Length - lineBorderPadding];
            for (int i = 0; i < d.Length; i++)
            {
                if (i == d.Length - 1)
                {
                    d[i] = verticalChar;
                }
                else d[i] = ' ';
            }
            string final = new string(c) + s + new string(d);
            return final.ToCharArray();
        }
        // This method is to turn char[] (lines) into printed strings.
        private void print(char[] c)
        {
            // No need for a \n here
            string s = new string(c);
            Console.WriteLine(s);
        }
        public void interfaceBankMainMenu(Bank bank, User user)
        {
            string[] s = new string[8]{
                "1. Create a new account",
                "2. Search for an account",
                "3. Deposit",
                "4. Withdraw",
                "5. A/C statement",
                "6. Delete account",
                "7. Log out",
                "8. Exit the application"
            };
            string prompt = "Enter your choice (" + 1.ToString() + "-" + s.Length.ToString() + "): ";
            Console.Clear();
            interfaceHeader("welcome to simple banking system");
            print(lineFullPadding());
            printMultipleFormLabel(s);
            print(lineFullPadding());
            print(lineFormLabel(prompt));
            print(lineFullPadding());
            print(lineBottom());
            // Input handling
            Console.SetCursorPosition(prompt.Length + lineBorderPadding + 1, s.Length + 3 + 4);
            char userInput = '0';
            while ((userInput = inputReadChoice().KeyChar) != '8')
            {
                switch (userInput)
                {
                    case '1':
                        Console.Clear();
                        bank.viewCreateNewAccount(user);
                        break;
                    case '2':
                        Console.Clear();
                        bank.viewSearch(user);
                        break;
                    case '3':
                        bank.viewDeposit(user);
                        break;
                    case '4':
                        bank.viewWithdraw(user);
                        break;
                    case '5':
                        bank.viewStatement(user);
                        break;
                    case '6':
                        bank.viewDeleteAccount(user);
                        break;
                    case '7':
                        Console.Clear();
                        bank.viewLogout(user);
                        break;
                    default:
                        break;
                }
            }
            Console.Clear();
            bank.viewExit(user);
        }
        // This method is to create a simple yes/no form interface. It supports a single description, no header. The equivalent of a modal.
        public bool interfaceModalYesNo(string s, int posY)
        {
            // Printing message
            string question = "Type 'y' to continue, or 'n' to exit: ";
            print(lineFullPadding());
            printLineTextMessage(s, false);
            print(lineFullPadding());
            print(lineFormLabel(question));
            print(lineFullPadding());
            print(lineBottom());
            // Input handling
            Console.SetCursorPosition(question.Length + lineBorderPadding + 1, lineCount(s) + 2 + posY);
            char userInput;
            while ((userInput = inputReadChoice().KeyChar) != 'n')
            {
                if (userInput == 'y')
                {
                    Console.WriteLine();
                    print(lineFullPadding());
                    printLineTextMessage("Press any key to continue...", false);
                    print(lineFullPadding());
                    print(lineBottom());
                    Console.ReadKey(true);
                    return true;
                }
            }
            Console.WriteLine();
            print(lineFullPadding());
            printLineTextMessage("Press any key to continue...", false);
            print(lineFullPadding());
            print(lineBottom());
            Console.ReadKey(true);
            return false;
        }
        // Assists any single key input choice
        private ConsoleKeyInfo inputReadChoice()
        {
            return Console.ReadKey(true);
        }
        // Finds out the length of lines using printLineTextMessage() by retracking the same method with additional triggers. A solution to a self-made problem.
        private int lineCount(string s)
        {
            int padding = this.width - (lineBorderPadding * 2);
            if (s.Length < padding)
            {
                return 1;
            }
            else
            {
                string[] words = s.Split(' ');
                string[] lines = new string[words.Length];
                int count = 0;
                int letterCount = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Length > padding)
                    {
                        char[] c = words[i].ToCharArray();
                        string temp = "";
                        string[] tempWords = new string[c.Length];
                        int countCharIndex = 0;
                        int countWordIndex = 0;
                        for (int z = 0; z < c.Length; z++)
                        {
                            if (countCharIndex < padding)
                            {
                                temp += c[z];
                                countCharIndex++;
                            }
                            else
                            {
                                tempWords[countWordIndex] = temp;
                                temp = "";
                                temp += c[z];
                                countWordIndex++;
                                countCharIndex = 1;
                            }
                        }
                        // Store the leftover temporary string.
                        tempWords[countWordIndex] = temp;
                        // Iterate the temporary string array to add to the line array
                        foreach (string v in tempWords)
                        {
                            if (v != null)
                            {
                                count++;
                                lines[count] += v;
                            }
                        }
                        letterCount = 0;
                    }
                    else if (letterCount + words[i].Length < padding)
                    {
                        letterCount += words[i].Length + 1;
                    }
                    else
                    {
                        count++;
                        letterCount = 0;
                        letterCount += words[i].Length + 1;
                    }
                }
                return count + 1; // To compensate for the array index starting at 0, increment by 1
            }
        }
        //Handles both single line and multiple line strings and prints them without needing the manual call of a print(). Prevents long words from breaking the interface.
        private void printLineTextMessage(string s, bool caps)
        {
            int padding = this.width - (lineBorderPadding * 2);
            if (s.Length > padding)
            {
                string[] words = s.Split(' ');
                string[] lines = new string[s.ToCharArray().Length];
                int count = 0;
                int letterCount = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    /* The order of procedure is:
                    1. Create a character array from the int word called c
                    2. Create a temporary string to concatenate characters into a new string called temp
                    3. Create a new string array called tempWords
                    4. Do a for loop:
                        i. charWordIndex = a manual index of tempWords
                        ii. countCharIndex = a manual ticker of the length of each word
                        iii. the IF statement is to check if we've reached the character limit per line. 
                            a. IF smaller than line, store the new character and increment ticker
                            b. IF not, then:
                                1. Store temporary word buffer into the string array
                                2. Clear temporary word buffer
                                3. Increment string array index
                                4. Reset manual ticker
                                5. Store the new character and increment ticker
                    5. Store the remaining temporary string into the temporary string buffer. */
                    if (words[i].Length > padding)
                    {
                        char[] c = words[i].ToCharArray();
                        string temp = "";
                        string[] tempWords = new string[c.Length];
                        int countCharIndex = 0;
                        int countWordIndex = 0;
                        for (int z = 0; z < c.Length; z++)
                        {
                            if (countCharIndex < padding)
                            {
                                temp += c[z];
                                countCharIndex++;
                            }
                            else
                            {
                                tempWords[countWordIndex] = temp;
                                temp = "";
                                temp += c[z];
                                countWordIndex++;
                                countCharIndex = 1;
                            }
                        }
                        // Store the leftover temporary string.
                        tempWords[countWordIndex] = temp;
                        // Iterate the temporary string array to add to the line array
                        foreach (string v in tempWords)
                        {
                            if (v != null)
                            {
                                count++;
                                lines[count] += v;
                            }
                        }
                        letterCount = 0;
                    }
                    // Checks if the word's length exceeds the remaining line space. If it doesn't, then this stores the word.
                    else if (letterCount + words[i].Length < padding)
                    {
                        lines[count] += words[i];
                        lines[count] += " ";
                        letterCount += words[i].Length + 1;
                    }
                    // Catchall for words that exceed the line space but doesn't meet previous criteria
                    else
                    {
                        count++;
                        letterCount = 0;
                        lines[count] += words[i];
                        lines[count] += " ";
                        letterCount += words[i].Length + 1;
                    }
                }
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] != null)
                    {
                        print(lineTextMessage(lines[i], caps));
                    }
                }
            }
            else
            {
                print(lineTextMessage(s, caps));
            }
        }
        // Generic message interface window -- good for error messages, string prints, etc. Requires a key press to continue.
        public void interfaceMessage(string header, string s)
        {
            print(lineFullPadding());
            print(lineTextMessage(header, true));
            print(lineFullPadding());
            printLineTextMessage(s, false);
            print(lineFullPadding());
            print(lineTextMessage("Press any key to continue...", false));
            print(lineFullPadding());
            print(lineBottom());
            Console.ReadKey(false);
        }
        // Creates the main application header window.
        public void interfaceHeader(string s)
        {
            Console.Clear();
            printHeaderMessage(s);
        }
        // Bank create an account screen
        public string[] interfaceBankCreateAccount()
        {
            interfaceHeader("Create a new account");
            print(lineFullPadding());
            print(lineTextMessage("Enter details", true));
            print(lineFullPadding());
            string[] formLabels = new string[5]{
                "First name: ",
                "Last name: ",
                "Address: ",
                "Phone: ",
                "Email: "
                };
            bool[] hideInput = new bool[5]{
                false,
                false,
                false,
                false,
                false
                };
            string[] final = interfaceForm(formLabels.Length + 3, formLabels, hideInput);
            Console.WriteLine("\n");
            return final;
        }
        // Bank confirmation of account creation screen
        public bool interfaceBankCreateAccountConfirm(string[] s)
        {
            string[] form = new string[5]{
                "First name: ",
                "Last name: ",
                "Address: ",
                "Phone: ",
                "Email: "
                };
            for (int i = 0; i < form.Length; i++)
            {
                form[i] += s[i];
            }
            interfaceHeader("Create a new account");
            print(lineFullPadding());
            print(lineTextMessage("Confirm details", true));
            print(lineFullPadding());
            printMultipleFormLabel(form);
            print(lineFullPadding());
            print(lineHorizontalSeparator());
            bool confirm = interfaceModalYesNo("Is this information correct?", form.Length + 5 + 3 + 2);
            return confirm;
        }
        // Main Bank login interface method
        public string[] interfaceBankLogin()
        {
            interfaceHeader("welcome to simple banking system");
            print(lineFullPadding());
            print(lineTextMessage("login to start", true));
            print(lineFullPadding());
            string[] formLabels = new string[2] { "Username: ", "Password: " };
            bool[] hideInput = new bool[2] { false, true };
            string[] final = interfaceForm(8, formLabels, hideInput);
            Console.WriteLine("\n");
            return final;
        }
        // Bank search account number main interface
        public string interfaceBankSearch(string header)
        {
            interfaceHeader(header);
            print(lineFullPadding());
            print(lineTextMessage("enter the details", true));
            print(lineFullPadding());
            string[] formLabels = new string[1] { "Account number: " };
            bool[] hideInput = new bool[1] { false };
            string[] final = interfaceForm(5 + 2 + formLabels.Length, formLabels, hideInput);
            Console.WriteLine("\n");
            // Convert string[] to single string.
            string number = final[0];
            return number;
        }
        // Bank deposit menu
        public string[] interfaceBankDepositWithdraw(Account account, string header)
        {
            string forma = "Account number: " + account.AccountNumber.ToString();
            string[] formb = {
                "Amount: $"
            };
            bool[] hideInput = {
                false
            };
            interfaceHeader(header);
            print(lineFullPadding());
            print(lineTextMessage("enter the details", true));
            print(lineFullPadding());
            print(lineFormLabel(forma));
            string[] amount = interfaceForm(5 + 2 + 2, formb, hideInput);
            return amount;
        }
        // Email interface
        public string[] interfaceBankStatementEmail(Account account, string header)
        {
            string message = "Enter your email account details to send the statement to the stored email in the bank account.";
            string warning = "Gmail users: You need to set your account to receive from less secure apps since OAuth is not implemented. See https://support.google.com/accounts/answer/6010255 for more information.";
            interfaceHeader(header);
            print(lineFullPadding());
            printLineTextMessage(message, false);
            print(lineFullPadding());
            printLineTextMessage(warning, false);
            print(lineFullPadding());
            string[] formLabels = new string[2] { "Your email: ", "Password: " };
            bool[] hideInput = new bool[2] { false, true };
            string[] auth = interfaceForm(8 + lineCount(warning) + lineCount(message), formLabels, hideInput);
            Console.WriteLine("\n");
            return auth;
        }
        public bool interfaceBankDeleteAccount(Account account, string header)
        {
            string[] form = renderAccountDetails(account);
            interfaceHeader(header);
            print(lineFullPadding());
            print(lineTextMessage("account details", true));
            print(lineFullPadding());
            printMultipleFormLabel(form);
            print(lineFullPadding());
            print(lineHorizontalSeparator());
            bool confirm = interfaceModalYesNo("Delete account?", form.Length + 5 + 1 + 2 + 2);
            return confirm;
        }
        private string[] renderAccountDetails(Account account)
        {
            string[] form = new string[7]{
                "Account Number: ",
                "Account Balance: $",
                "First Name: ",
                "Last Name: ",
                "Address: ",
                "Phone: ",
                "Email: "
                };
            for (int i = 0; i < form.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        form[i] += account.AccountNumber;
                        break;
                    case 1:
                        form[i] += account.Balance;
                        break;
                    case 2:
                        form[i] += account.FirstName;
                        break;
                    case 3:
                        form[i] += account.LastName;
                        break;
                    case 4:
                        form[i] += account.Address;
                        break;
                    case 5:
                        form[i] += account.PhoneNo;
                        break;
                    case 6:
                        form[i] += account.Email;
                        break;
                }
            }
            return form;
        }
        // Bank statement menu
        public bool interfaceBankStatement(Account account)
        {
            List<String> details = new List<String>();
            account.Activities.Sort((x, y) => x.Date.CompareTo(y.Date));
            account.Activities.Reverse();
            foreach (Activity activity in account.Activities)
            {
                details.Add(activity.GetDateString() + ": " + activity.Type + " $" + activity.Amount.ToString() + " for a total of $" + activity.Balance.ToString());
            }
            string[] form = renderAccountDetails(account);
            // Convert list into static array because that's what the function needs
            string[] input = details.ToArray();
            // Trim array size to 5 items or less. The if statement is to prevent a null exception when printing null objects
            var size = 5;
            if (input.Length < size) size = input.Length;
            Array.Resize<string>(ref input, size);
            // Print statement
            int exception = 0;
            interfaceHeader("statement");
            print(lineFullPadding());
            printLineTextMessage("Account details", true);
            print(lineFullPadding());
            printMultipleFormLabel(form);
            print(lineFullPadding());
            printLineTextMessage("Latest five actions", true);
            print(lineFullPadding());
            if (input.Length > 0) { printMultipleFormLabel(input); }
            else
            {
                printLineTextMessage("This account has no transactions recorded.", false);
                exception++;
            }
            print(lineFullPadding());
            print(lineHorizontalSeparator());
            bool confirm = interfaceModalYesNo("Email a copy of this statement?", input.Length + form.Length + 2 + 3 + 2 + 5 + 1 + exception);
            return confirm;
        }
        // Bank show account details
        public bool interfaceBankSearchSuccess(Account account)
        {
            string[] form = renderAccountDetails(account);
            interfaceHeader("account details");
            print(lineFullPadding());
            printMultipleFormLabel(form);
            print(lineFullPadding());
            print(lineHorizontalSeparator());
            bool confirm = interfaceModalYesNo("Check another account?", form.Length + 5 + 1 + 2);
            return confirm;
        }
        // Generic error window with a yes/no continue check
        public bool interfaceBankErrorContinue(string header, string errorMessage, string promptMessage)
        {
            interfaceHeader(header);
            print(lineFullPadding());
            printLineTextMessage("error", true);
            print(lineFullPadding());
            printLineTextMessage(errorMessage, false);
            print(lineFullPadding());
            print(lineHorizontalSeparator());
            bool confirm = interfaceModalYesNo(promptMessage, 8 + lineCount(errorMessage) + 2);
            return confirm;
        }
        // Reusable method for printing form labels only. No padding involved.
        public void printMultipleFormLabel(string[] formLabels)
        {
            for (int i = 0; i < formLabels.Length; i++)
            {
                print(lineFormLabel(formLabels[i]));
            }
        }
        // Reusable method for printing forms, i.e. [label]: [input]. Used for input text fields
        private string[] interfaceForm(int windowBufferLineNumber, string[] formLabels, bool[] hideInput)
        {
            string[] formInput = new string[formLabels.Length];
            try
            {
                printMultipleFormLabel(formLabels);
                print(lineFullPadding());
                print(lineBottom());
                int count = windowBufferLineNumber;
                for (int i = 0; i < formInput.Length; i++)
                {
                    Console.SetCursorPosition(formLabels[i].Length + lineBorderPadding, count);
                    if (hideInput[i] == true) formInput[i] = inputMask();
                    else formInput[i] = Console.ReadLine();
                    count++;
                }
                return formInput;
            }
            catch (Exception)
            {
                return formInput;
            }
        }
        // Used to mask user input, such as passwords
        private string inputMask()
        {
            string s = "";
            ConsoleKeyInfo cki = Console.ReadKey(true);
            while (cki.Key != ConsoleKey.Enter)
            {
                if (
                    cki.Key == ConsoleKey.Tab ||
                    cki.Key == ConsoleKey.Spacebar ||
                    cki.Key == ConsoleKey.Escape
                    )
                {
                    cki = Console.ReadKey(true);
                }
                else if (cki.Key != ConsoleKey.Backspace)
                {
                    Console.Write('*');
                    s += cki.KeyChar;
                    cki = Console.ReadKey(true);
                }
                else
                {
                    if (s.Length == 0) cki = Console.ReadKey(true);
                    else
                    {
                        s = s.Substring(0, s.Length - 1);
                        int cursorPos = Console.CursorLeft;
                        Console.SetCursorPosition(cursorPos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(cursorPos - 1, Console.CursorTop);
                        cki = Console.ReadKey(true);
                    }
                }
            }
            Console.Write("\n");
            return s;
        }
    }
}

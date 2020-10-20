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
    class Validator
    {
        private string emailRegex = @"^([a-zA-Z0-9_.+-])+@(gmail\.com$|hotmail\.com$){1}$"; // Email needs to match gmail.com, hotmail.com domains and a @ must be present.
        private string integerPattern = @"^(\d{1,9}){1}$"; // To prevent invalid integers for deposit/withdrawal
        private string phonePattern = @"^(\d{1,10}){1}$"; // Prevent non-digits in phone number
        private string accountNumberPattern = @"\/([0-9]{6,8}){1}\.txt$"; // Account file pattern for matching files in directory.
        private string accountNumberSearchPattern = @"^([0-9]{6,8}){1}$"; // Account number pattern for general use
        private string[] validateKeys = { "email", "phone", "accNo", "accountNumber", "int" };
        private string formLabelPattern = @"^.+\|"; // Finds everything before a |
        private string emailHotmailPattern = @"^([a-zA-Z0-9_.+-])+@(hotmail\.com$){1}$";
        private string emailGmailPattern = @"^([a-zA-Z0-9_.+-])+@(gmail\.com$){1}$";
        private bool validateString(string pattern, string input)
        {
            return Regex.IsMatch(input, pattern);
        }
        public string deleteFormLabel(string input)
        {
            return Regex.Replace(input, formLabelPattern, "");
        }
        public bool validate(string key, string input)
        {
            int c = 0;
            for (int i = 0; i < validateKeys.Length; i++)
            {
                if (key == validateKeys[i])
                {
                    c = i;
                }
            }
            switch (c)
            {
                case 0: return validateString(emailRegex, input);
                case 1: return validateString(phonePattern, input);
                case 2: return validateString(accountNumberPattern, input);
                case 3: return validateString(accountNumberSearchPattern, input);
                case 4: return validateString(integerPattern, input);
                default: return false;
            }
        }
        public int validateEmailType(string input)
        {
            if (validate(emailHotmailPattern, input)) return 0;
            else if (validate(emailGmailPattern, input)) return 1;
            else return -1;
        }
        // This is for input forms because someone will inevitably enter blank fields and then ask why those were valid
        public bool validateEmptyFields(string[] s)
        {
            int count = 0;
            if (s.Length > 0)
            {
                foreach (string v in s)
                {
                    if (String.IsNullOrEmpty(v)) count++;
                }
            }
            if (count > 0) return true; else return false;
        }
    }
}

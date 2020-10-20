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
    class Activity
    {
        private DateTime date;
        private string type;
        private int amount;
        private int balance;
        public Activity(DateTime date, string type, int amount, int balance)
        {
            this.Date = date;
            this.Type = type;
            this.Amount = amount;
            this.Balance = balance;
        }
        public string Type { get => type; set => type = value; }
        public int Amount { get => amount; set => amount = value; }
        public int Balance { get => balance; set => balance = value; }
        public DateTime Date { get => date; set => date = value; }

        public string FileOutput() // Creates a plaintext string for account files
        {
            string output = Date.ToString("dd.MM.yyyy") + "|" + Type + "|" + Amount.ToString() + "|" + Balance.ToString();
            return output;
        }
        public string GetDateString() // Returns a formatted date string
        {
            return Date.ToString("dd.MM.yyyy");
        }
        public string EmailOutput() // Creates a plaintext string for email text parsing.
        {
            return GetDateString() + ": " + Type + " $" + Amount + " for a total of $" + Balance;
        }
    }
}
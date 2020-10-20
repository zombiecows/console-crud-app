using System;
namespace bank
{
    class User
    {
        private string username;  // unique
        private string password;
        public User(string username, string password, Bank bank)
        {
            this.username = username;
            this.password = password;
        }
    }
}

using System;
namespace PittClubManager.Models
{
    public class User
    {
        private int uid;
        private string firstName;
        private string lastName;

        public User(int uid, string firstName, string lastName)
        {
            this.uid = uid;
            this.firstName = firstName;
            this.lastName = lastName;
        }
        public string GetFirstName()
        {
            return firstName;
        }

        public string GetLastName()
        {
            return lastName;
        }
    }
}

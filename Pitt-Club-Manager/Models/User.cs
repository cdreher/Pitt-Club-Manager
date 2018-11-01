using System;
namespace PittClubManager.Models
{
    public class User
    {
        private int uid;
        private UserPermission permission;
        private string firstName;
        private string lastName;

        public User(int uid, UserPermission permission, string firstName, string lastName)
        {
            this.uid = uid;
            this.permission = permission;
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

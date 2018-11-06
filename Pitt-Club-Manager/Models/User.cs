using System;
namespace PittClubManager.Models
{
    public class User
    {
        private string id;
        private string firstName;
        private string lastName;

        public User(string id, string firstName, string lastName)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
        }

        public string GetFirstName()
        {
            return firstName;
        }

        public void SetFirstName(string firstName)
        {
            this.firstName = firstName;
        }

        public string GetLastName()
        {
            return lastName;
        }

        public void SetLastName(string lastName)
        {
            this.lastName = lastName;
        }

        public string GetId() {
            return id;
        }
       
        public void SetId(string id) {
            this.id = id;
        }

    }
}

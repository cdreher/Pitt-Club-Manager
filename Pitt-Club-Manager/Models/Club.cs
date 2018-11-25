using System;
namespace PittClubManager.Models
{
    public class Club
    {
        private string id;
        private string name;
        private User manager;
        private User[] members;
        private User[] memberRequests;
        private Event[] events;

        public Club()
        {
            this.members = new User[0];
            this.events = new Event[0];
        }

        public string GetId() {
            return id;
        }

        public void SetId(string id) {
            this.id = id;
        }

        public string GetName() {
            return name;
        }

        public void SetName(string name) {
            this.name = name;
        }

        public void SetManager(User manager) {
            this.manager = manager;
        }

        public void SetEvents(Event[] events) {
            this.events = events;
        }

        public Event[] GetEvents() {
            return events;
        }

        public User GetManager() {
            return manager;
        }

        public User[] GetMembers() {
            return members;
        }

        public void SetMembers(User[] members) {
            this.members = members;
        }

        public void SetMemberRequests(User[] memberRequests)
        {
            this.memberRequests = memberRequests;
        }

        public User[] GetMemberRequests()
        {
            return memberRequests;
        }

    }
}

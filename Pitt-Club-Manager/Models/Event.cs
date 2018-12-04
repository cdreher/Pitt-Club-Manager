using System;
namespace PittClubManager.Models
{
    public class Event
    {
        private string id;
        private DateTime start;
        private string name;
        private string location;

        public Event(string id, DateTime start, string name, string location)
        {
            this.id = id;
            this.start = start;
            this.name = name;
            this.location = location;
        }

        public string GetId() {
            return id;
        }

        public void SetId(string id) {
            this.id = id;
        }

        public DateTime GetStart() {
            return start;
        }

        public void SetStart(DateTime start) {
            this.start = start;
        }

        public string GetName() {
            return name;
        }

        public void SetName(string name) {
            this.name = name;
        }

        public string GetLocation() {
            return location;
        }

        public void SetLocation(string location) {
            this.location = location;
        }
    }
}

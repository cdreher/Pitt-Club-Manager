using System;
namespace PittClubManager.Models
{
    public class Event
    {
        private string id;
        private DateTime start;
        private string name;
        private string description;

        public Event(string id, DateTime start, string name, string description)
        {
            this.id = id;
            this.start = start;
            this.name = name;
            this.description = description;
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

        public string GetDescription() {
            return description;
        }

        public void SetDescription(string description) {
            this.description = description;
        }
    }
}

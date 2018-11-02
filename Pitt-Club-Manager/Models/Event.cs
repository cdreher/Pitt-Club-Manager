using System;
namespace PittClubManager.Models
{
    public class Event
    {
        private int id;
        private DateTime start;
        private string name;
        private string description;

        public Event(int id, DateTime start, string name, string description)
        {
            this.id = id;
            this.start = start;
            this.name = name;
            this.description = description;
        }

        public int GetId() {
            return id;
        }

        public void SetId(int id) {
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

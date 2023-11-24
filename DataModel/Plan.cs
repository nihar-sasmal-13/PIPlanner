using PIPlanner.Helpers;
using System;

namespace PIPlanner.DataModel
{
    class Plan : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _lastModifiedAt;
        public DateTime LastModifiedAt
        {
            get => _lastModifiedAt;
            set => SetProperty(ref _lastModifiedAt, value);
        }
    }
}

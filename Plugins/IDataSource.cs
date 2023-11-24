using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.Plugins
{
    internal interface IDataSource
    {
        [Flags]
        public enum ContentToFetch
        {        
            Nothing = 0,
            CRsOnly = 1,
            TeamsOnly = 2,
            SprintsOnly = 4,
            Everything = 7
        }

        List<ChangeRequest> ChangeRequests { get; }

        List<Team> Teams { get; }

        List<Sprint> Sprints { get; }

        void FetchData(List<KeyValue>? inputs, ContentToFetch fetch = ContentToFetch.Everything);
    }
}

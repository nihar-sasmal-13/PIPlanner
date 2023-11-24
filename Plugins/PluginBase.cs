using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PIPlanner.Plugins.IDataSource;

namespace PIPlanner.Plugins
{
    abstract internal class PluginBase : IDataSource
    {
        public List<ChangeRequest> ChangeRequests { get; protected set; }

        public List<Team> Teams { get; protected set; }

        public List<Sprint> Sprints { get; protected set; }

        public abstract void FetchData(List<KeyValue>? inputs, ContentToFetch fetch = ContentToFetch.Everything);        

        protected List<KeyValue> getInputsFromUser(List<KeyValue> keysWithDefaults)
        {
            UserInputDialog dlg = new UserInputDialog(keysWithDefaults);
            dlg.ShowDialog();
            return dlg.UserInputs;
        }
    }
}

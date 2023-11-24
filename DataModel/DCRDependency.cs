using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.DataModel
{
    internal class DCRDependency : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _dcr;
        [ForeignKey(nameof(DCRId))]
        public string DCRId
        {
            get => _dcr;
            set => SetProperty(ref _dcr, value);
        }

        public ChangeRequest DCR { get; set; }

        private string _dependsOnId;
        [ForeignKey(nameof(DependsOnId))]
        public string DependsOnId
        {
            get => _dependsOnId;
            set => SetProperty(ref _dependsOnId, value);
        }

        public ChangeRequest DependsOn { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIPlanner.DataModel
{
    internal class ChangeRequest : PropertyNotifier
    {
        private string _id;        
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private int? _sprintId;
        [ForeignKey(nameof(SprintId))]
        public int? SprintId
        {
            get => _sprintId;
            set => SetProperty(ref _sprintId, value);
        }

        public Sprint? Sprint { get; set; }

        private int? _teamId;
        [ForeignKey(nameof(TeamId))]
        public int? TeamId
        {
            get => _teamId;
            set => SetProperty(ref _teamId, value);
        }
        
        public Team? Team { get; set; }

        private string? _project;
        public string? Project
        {
            get => _project;
            set => SetProperty(ref _project, value);
        }

        private string? _release;
        public string? Release
        {
            get => _release;
            set => SetProperty(ref _release, value);
        }


        protected string? _summary;
        public string? Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        private int _sps;
        public int SPs
        {
            get => _sps;
            set => SetProperty(ref _sps, value);
        }

        private string? _functionalArea;
        public string? FunctionalArea
        {
            get => _functionalArea;
            set => SetProperty(ref _functionalArea, value);
        }

        private DCRStatus _status = DCRStatus.Approved;
        public DCRStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private Brush _background;
        [NotMapped]
        public Brush BackgroundColor
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        [NotMapped]
        public bool IsFeature
        {
            get => this.IsFeature();
        }
    }
}

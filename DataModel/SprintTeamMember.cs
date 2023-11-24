using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.DataModel
{
    internal class SprintTeamMember : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id; 
            set => SetProperty(ref _id, value);
        }

        private int _teamMemberId;
        [ForeignKey(nameof(TeamMemberId))]
        public int TeamMemberId
        {
            get => _teamMemberId;
            set => SetProperty(ref _teamMemberId, value);
        }

        public TeamMember TeamMember { get; set; }

        private int _sprintTeamId;
        [ForeignKey(nameof(SprintTeamId))]
        public int SprintTeamId
        {
            get => _sprintTeamId;
            set => SetProperty(ref _sprintTeamId, value);
        }

        public SprintTeam SprintTeam { get; set; }

        private float _drag;
        public float Drag
        {
            get => _drag;
            set => SetProperty(ref _drag, value);
        }

        private float _workingDays;
        public float WorkingDays
        {
            get => _workingDays;
            set => SetProperty(ref _workingDays, value);
        }
    }
}

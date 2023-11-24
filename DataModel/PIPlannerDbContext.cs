using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.DataModel
{
    internal class PIPlannerDbContext : DbContext
    {
        public PIPlannerDbContext(DbContextOptions<PIPlannerDbContext> options) : base(options) 
        {
            Database.EnsureCreated();            
        }

        public DbSet<Plan> Plans { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<SprintTeam> SprintTeams { get; set; }        
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<SprintTeamMember> SprintTeamMembers { get; set; }
        public DbSet<SprintSummary> SprintSummaries { get; set; }
        public DbSet<SprintContent> SprintContents { get; set; }
    }
}

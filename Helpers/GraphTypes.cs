using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.Helpers
{
    class NormalArrow
    {
    }

    class GitItem
    {
        public string Name { get; set; }
    }

    class GitRelease : GitItem 
    { 
    }

    class GitFeature : GitItem 
    { 
    }

    class GitFeatureSprint : GitItem 
    { 
    }

    class GitSprint : GitItem 
    { 
    }

    class GitDCR : GitItem 
    { 
    }

    class DCREdge 
    { 
    }
}

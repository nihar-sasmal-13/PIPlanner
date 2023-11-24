using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.Helpers
{
    static class ChangeTracker
    {
        private static Action _notifyChange;
        public static void Initialize(Action notifyChange)
        {
            _notifyChange = notifyChange;
        }

        public static void Notify()
        {
            if (_notifyChange != null)
                _notifyChange();
        }
    }
}

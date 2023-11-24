using PIPlanner.DataModel;
using PIPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.Helpers
{
    internal static class FunctionalAreaHelper
    {
        public static string MakeMneumonic(string funcationalArea)
        {
            //TODO: Find a way to squeeze the functional area into a mneumonic
            return funcationalArea;
        }

        public static List<string> FeatureFunctionalAreas
        {
            get => new List<string> { "__Feature - 1844951", "__Feature-PSE - 3742073" };
        }

        public static bool IsFeature(this ChangeRequest cr)
        {
            if (cr == null) return false;
            return cr.GetFunctionalAreas().Any(fc => FeatureFunctionalAreas.Contains(fc));
        }

        public static List<string> GetFunctionalAreas(this ChangeRequest cr)
        {
            return string.IsNullOrEmpty(cr.FunctionalArea) ? 
                new List<string>() :
                cr.FunctionalArea.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}

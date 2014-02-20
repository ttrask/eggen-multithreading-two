using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using System.Web.Mvc;

namespace AppStats.Models
{
    public class Chart
    {
        public Chart()
        {
        }
        public string Title;
        public List<int> Sizes = new List<int>();

        public List<ChartFilter> Filters = new List<ChartFilter>();
    }

    public class ChartData
    {
        public Language Language;
        public Environment Environment;

        public List<Tuple<int, decimal>> Data = new List<Tuple<int, decimal>>();
    }

    public class ChartFilter
    {
        public ChartFilter()
        {
            Data = new List<ChartData>();
        }
        public int LanguageId;
        public byte EnvironmentId;
        public List<int> ProcCounts;
        public List<int> TimeTypeIds;
        public List<SelectListItem> LanguageListItems;
        public IEnumerable<SelectListItem> EnvironmentListItems;
        public IEnumerable<SelectListItem> ProcCountListItems;
        public IEnumerable<SelectListItem> TimeTypeListItems;
        public int StartVal;
        public int EndVal;
        public string Name;
        public List<ChartData> Data;
    }
}
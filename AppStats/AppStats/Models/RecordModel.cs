using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppStats.DataAccess;


namespace AppStats.Models
{


    public partial class Record: AppStatsEntity
    {
        private static AppStatsContext db = new AppStatsContext();

        public Record()
        {
            CreateDtTm = DateTime.Now;

        }

        public static IEnumerable<SelectListItem> Environments
        {
            get
            {
                return db.Environments.ToList().Select(x => new SelectListItem()
                {
                    Value = x.EnvinronmentId.ToString(),
                    Text = x.Name
                });

            }

        }
        public static IEnumerable<SelectListItem> LanguagesInUse
        {
            get
            {
                return (from t in db.TimeStatistics 
                        join s in db.Languages
                            on t.LanguageId equals s.LanguageId
                        select s)
                        .Distinct()
                        .ToList().Select(x => new SelectListItem()
                {
                    Value = x.LanguageId.ToString(),
                    Text = x.Name
                });
            }
        }

        public static IEnumerable<SelectListItem> Languages
        {
            get
            {
                return (from s in db.Languages
                        select s)
                        .Distinct()
                        .ToList().Select(x => new SelectListItem()
                        {
                            Value = x.LanguageId.ToString(),
                            Text = x.Name
                        });
            }
        }


        public static IEnumerable<SelectListItem> ProcCounts
        {
            get
            {
                return db.TimeStatistics.Select(t=>t.ProcessorCount).Distinct().ToList().Select(x => new SelectListItem()
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                });
            }
        }

        public static IEnumerable<SelectListItem> TimeTypes
        {
            get
            {
                return db.TimeTypes.ToList().Select(x => new SelectListItem()
                {
                    Value = x.TimeTypeId.ToString(),
                    Text = x.Name
                });
            }
        }

    }
}
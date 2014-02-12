using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppStats.Models;
using AppStats.Models.Enums;
using AppStats.DataAccess;
using AppStats.Helpers;
using System.Web.Script.Serialization;

namespace AppStats.Controllers
{
    public class StatisticsController : Controller
    {
        //
        // GET: /Statistics/
        int goDivisor = 1000000;

        AppStatsContext db = new AppStatsContext();

        public ActionResult Index()
        {
            Chart chart = new Chart();
            chart.Filters = new List<ChartFilter>();
            chart.Filters.Add(new ChartFilter());

            return View(chart);

        }

        public ActionResult Filter(ChartFilter Filter)
        {
            if (Filter != null)
            {
                Filter.LanguageListItems = Record.LanguagesInUse;

                if (Filter.LanguageId > 0)
                {
                    Filter.LanguageListItems.Where(l => l.Value == Filter.LanguageId.ToString()).First().Selected = true;

                    Filter.EnvironmentListItems = GetEnvironsListForLang(Filter.LanguageId.ToString());
                    Filter.EnvironmentListItems.Where(l => l.Value == Filter.EnvironmentId.ToString()).First().Selected = true;


                    if (Filter.EnvironmentId > 0)
                    {
                        Filter.ProcCountListItems = GetCountListForEnvLang(Filter.EnvironmentId.ToString(), Filter.LanguageId.ToString());

                        if (Filter.ProcCounts.Any())
                            Filter.ProcCountListItems.Where(l => l.Value == Filter.ProcCounts.First().ToString()).First().Selected = true;

                        if (Filter.TimeTypeIds.Any())
                        {
                            Filter.TimeTypeListItems = GetCountListForEnvLangProc(Filter.ProcCounts.First().ToString(), Filter.EnvironmentId.ToString(), Filter.LanguageId.ToString());
                            Filter.TimeTypeListItems.Where(l => l.Value == Filter.TimeTypeIds[0].ToString()).First().Selected = true;
                        }

                    }
                }

            }


            return View(Filter);
        }

        public ActionResult UpdateStatistics()
        {

            if (new StatisticsHelper().UpdateStatistics())
            {
                //do something if it's updated successfully
            }
            else
            {
                //do something if it's not.
            }

            return RedirectToAction("Index");
        }

        public ActionResult Chart(String filterString, string title)
        {

            List<ChartFilter> filters = new List<ChartFilter>();

            Chart chart = new Chart();

            if (!String.IsNullOrEmpty(filterString))
            {
                try
                {
                    filters = (List<ChartFilter>)(new JavaScriptSerializer().Deserialize(filterString, typeof(List<ChartFilter>)));

                }
                catch
                {

                }


                if (!String.IsNullOrEmpty(title))
                {
                    chart.Title = title;
                }
                else
                {
                    chart.Title = "GO vs SCALA";
                }


                foreach (ChartFilter filter in filters)
                {


                    if (chart.Filters == null)
                    {
                        chart.Filters = new List<ChartFilter>();
                    }

                    var langId = filter.LanguageId;

                    byte envId = filter.EnvironmentId;

                    var pc = filter.ProcCounts[0];
                    var tt = filter.TimeTypeIds[0];

                    Language lang = null;
                    AppStats.Models.Environment env = null;
                    if (langId != null && envId != null)
                    {
                        lang = db.Languages.Where(l => l.LanguageId == langId).FirstOrDefault();
                        env = db.Environments.FirstOrDefault(e => e.EnvinronmentId == envId);



                        List<TimeStatistic> recs = db.TimeStatistics.Where(t => t.LanguageId == lang.LanguageId && t.EnvironmentId == env.EnvinronmentId && t.ProcessorCount == pc && t.TimeTypeId == tt && t.DatasetSize >= filter.StartVal && t.DatasetSize <= filter.EndVal).OrderBy(o => o.DatasetSize).ToList();

                        //chart.Sizes.AddRange( recs.Select(r => r.DatasetSize).Distinct().ToList());

                        filter.Data = new List<ChartData>();

                        ChartData cd = new ChartData() { Environment = null, Language = lang };

                        foreach (TimeStatistic stat in recs)
                        {
                            if (lang.Name.ToLower().Contains("go"))
                            {
                                cd.Data.Add(new Tuple<int, decimal>(stat.DatasetSize, stat.MeanAverage / goDivisor));
                            }
                            else
                            {
                                cd.Data.Add(new Tuple<int, decimal>(stat.DatasetSize, stat.MeanAverage));
                            }
                        }
                        if (String.IsNullOrEmpty(filter.Name))
                        {
                            if (lang.Name.ToLower().Contains("stock"))
                            {
                                filter.Name = String.Format("{0}", lang.Name, env.Name, filter.ProcCounts[0]);
                            }
                            else
                            {
                                filter.Name = String.Format("{0}-{2}pc", lang.Name, env.Name, filter.ProcCounts[0]);
                            }
                        }
                        filter.Data.Add(cd);

                        chart.Filters.Add(filter);

                    }
                }
            }
            else
            {
                chart.Filters.Add(new ChartFilter());
            }

            //chart.Sizes = chart.Sizes.Distinct().ToList() ;

            return View(chart);
        }

        public List<SelectListItem> GetEnvironsListForLang(String languageId)
        {
            if (!String.IsNullOrEmpty(languageId))
            {
                int langId = Int32.Parse(languageId);
                ViewBag.LanguageId = langId;

                List<SelectListItem> set = new List<SelectListItem>() { };


                set.AddRange((from s in db.TimeStatistics
                              join e in db.Environments
                              on s.EnvironmentId equals e.EnvinronmentId
                              where s.LanguageId == langId
                              select e).Distinct().ToList().Select(x => new SelectListItem()
                              {
                                  Value = x.EnvinronmentId.ToString(),
                                  Text = x.Name
                              }));

                if (set.Count > 1)
                {
                    set.Insert(0, new SelectListItem() { Text = "", Value = "" });
                }

                return set;

            }
            else
            {
                return null;
            }
        }


        public String GetEnvironsForLang(String languageId)
        {
            return new JavaScriptSerializer().Serialize(GetEnvironsListForLang(languageId));
        }


        public List<SelectListItem> GetCountListForEnvLang(String EnvironmentId, string LanguageId)
        {
            if (EnvironmentId != null)
            {
                Int32 environId = Int32.Parse(EnvironmentId);
                Int32 languageId = Int32.Parse(LanguageId);

                ViewBag.EnvironmentId = environId;

                List<SelectListItem> set = new List<SelectListItem>() { };

                set.AddRange((from s in db.TimeStatistics
                              where s.LanguageId == languageId
                                 && s.EnvironmentId == environId
                              select s.ProcessorCount).Distinct().ToList().OrderBy(s => s).Select(x => new SelectListItem()
                {
                    Value = x.ToString(),
                    Text = x.ToString()
                }));

                if (set.Count > 1)
                {
                    set.Insert(0, new SelectListItem() { Text = "", Value = "" });
                }

                return set;

            }
            else
            {
                return null;
            }
        }
        public String GetCountForEnvLang(String EnvironmentId, string LanguageId)
        {
            return new JavaScriptSerializer().Serialize(GetCountListForEnvLang(EnvironmentId, LanguageId));
        }

        public List<SelectListItem> GetCountListForEnvLangProc(String ProcCount, string EnvironmentId, string LanguageId)
        {
            if (ProcCount != null)
            {

                Int32 environId = Int32.Parse(EnvironmentId);
                Int32 languageId = Int32.Parse(LanguageId);
                Int32 procCount = Int32.Parse(ProcCount);

                List<SelectListItem> set = new List<SelectListItem>() { };

                set.AddRange((from s in db.TimeStatistics
                              join tt in db.TimeTypes
                                on s.TimeTypeId equals tt.TimeTypeId
                              where s.LanguageId == languageId
                                 && s.EnvironmentId == environId
                                 && s.ProcessorCount == procCount
                              select tt).Distinct().OrderBy(o => o.TimeTypeId).ToList().Select(x => new SelectListItem()
                              {
                                  Value = x.TimeTypeId.ToString(),
                                  Text = x.Name
                              }));

                if (set.Count > 1)
                {
                    set.Insert(0, new SelectListItem() { Text = "", Value = "" });
                }

                return set;
            }
            else
            {
                return null;
            }
        }


        public string GetCountForEnvLangProc(String ProcCount, string EnvironmentId, string LanguageId)
        {

            return new JavaScriptSerializer().Serialize(GetCountListForEnvLangProc(ProcCount, EnvironmentId, LanguageId));

        }


    }
}

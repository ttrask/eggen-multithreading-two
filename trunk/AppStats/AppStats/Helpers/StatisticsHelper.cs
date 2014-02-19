using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Web;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Transactions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using AppStats.Models;
using AppStats.Models.Enums;
using AppStats.DataAccess;

namespace AppStats.Helpers
{
    public class StatisticsHelper
    {
        //
        // GET: /StatisticsHelper/
        AppStatsContext _db = new AppStatsContext();


        public Boolean UpdateStatistics()
        {
            //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 15, 0)))
            //{

            DateTime dt_0 = DateTime.Now;
            try
            {

                _db.Database.ExecuteSqlCommand("DELETE FROM TimeStatistics");
                //var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;

                //objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Statistics.TimeStatistics]");

                //get active set


                var tups = (from r in _db.Records select new { r.LanguageId, r.EnvironmentId, r.ProcessorCount, r.Size }).Distinct().ToList();
                var ts = tups.GroupBy(t => new { t.ProcessorCount, t.Size }).Select(group => new { CompositeKey = group.Key, CompositeValues = group.Select(s => new { s.LanguageId, s.EnvironmentId }).Distinct() });

                //List<long> langIds = (from r in _db.DropFiles select r.LanguageId).Distinct().ToList();
                //List<long> envids = (from r in _db.DropFiles select r.EnvironmentId).Distinct().ToList();
                //List<int> procCounts = (from r in _db.Records select r.ProcessorCount).Distinct().ToList();
                //List<long> timeTypes = (from t in _db.TimeTypes select t.TimeTypeId).Distinct().ToList();
                //List<int> ns = (from r in _db.Records select r.Size).Distinct().ToList();

                AppStatsContext db = _db;


                try
                {
                    ConcurrentBag<TimeStatistic> stats = new ConcurrentBag<TimeStatistic>();


                    //foreach (var tup in ts)
                    Parallel.ForEach(ts, tup =>
                    {
                        
                        int procCount = tup.CompositeKey.ProcessorCount;
                        int size = tup.CompositeKey.Size;

                        //Parallel.ForEach(langIds, langId =>

                        //Parallel.ForEach(envids, envid =>

                        //Parallel.ForEach(procCounts, procCount =>

                        DateTime batchTime = DateTime.Now;
                        DateTime dt1, dt2 = DateTime.Now;


                        //Parallel.ForEach(tup.CompositeValues, s =>
                        foreach (var s in tup.CompositeValues)
                        {
                            long langId = s.LanguageId;
                            long envid = s.EnvironmentId;



                            //Dictionary<long, IEnumerable<Record>> timeRecords = new Dictionary<long, IEnumerable<Record>>();




                            System.Diagnostics.Debug.WriteLine(String.Format("Time Sets:{0}-{1}-{2}-{3}", langId, envid, procCount, size));


                            dt1 = DateTime.Now;

                            (from r in db.Records
                             join d in db.DropFiles
                             on r.DropFileId equals d.DropFileId
                             where d.IsActive == true
                             && r.Size == size
                             && d.LanguageId == langId
                             && d.EnvironmentId == envid
                             && r.ProcessorCount == procCount
                             select r).GroupBy(t => t.TimeTypeId)
                             .ToList().ForEach(timeSets =>
                            {

                                //System.Diagnostics.Debug.WriteLine("Time To execute LINQ:" + new TimeSpan(DateTime.Now.Ticks - dt1.Ticks).Milliseconds);



                                if (timeSets != null && timeSets.Any())
                                {


                                    //IEnumerable<Time> timeSets = list;

                                  //  Record set = timeSets.First();




                                    //foreach (IGrouping<long, Record> timeSet in timeSets.Where(t => t.TimeTypeId > 0).GroupBy(t => t.TimeTypeId))
                                    //{
                                    //    timeRecords.Add(timeSet.Key, timeSet.Select(t => t));
                                    //};

                                    System.Diagnostics.Debug.WriteLine("Time To Split Times " + timeSets.Count() + " by TimeType:" + new TimeSpan((DateTime.Now.Ticks - dt1.Ticks)).Milliseconds);

                                    dt1 = DateTime.Now;

                                    //Parallel.ForEach(timeRecords.Keys, tt =>

                                    List<double> times = new List<double>(timeSets.Select(t => Convert.ToDouble(t.TimeValue)).ToList());


                                    if (times.Any())
                                    {

                                        DescriptiveStatistics statVals = new MathNet.Numerics.Statistics.DescriptiveStatistics(times);

                                       //Record r = set;

                                        TimeStatistic stat = new TimeStatistic()
                                        {
                                            BatchTime = batchTime,
                                            LanguageId = Convert.ToByte(langId),
                                            DatasetSize = times.Count(),
                                            ProcessorCount = timeSets.First().ProcessorCount,
                                            EnvironmentId = Convert.ToByte(envid),
                                            TimeTypeId = timeSets.Key,
                                            Mean = Convert.ToDecimal(statVals.Mean),
                                            Median = Convert.ToDecimal(Statistics.Median(times)),
                                            Average = Convert.ToDecimal(times.Average()),
                                            Q1Mean = Convert.ToDecimal(Statistics.LowerQuartile(times)),
                                            Q3Mean = Convert.ToDecimal(Statistics.UpperQuartile(times)),
                                            Min = Convert.ToDecimal(statVals.Minimum),
                                            Max = Convert.ToDecimal(statVals.Maximum),
                                            RecordCount = times.Count(),
                                            MeanAverage = GetMeanAverage(times.ToList().ConvertAll<Decimal>(d => Convert.ToDecimal(d)))
                                        };
                                        stats.Add(stat);
                                    }

                                    //);

                                    System.Diagnostics.Debug.WriteLine(String.Format("Time To Generate {0} Statistics: {1}", stats.Count, new TimeSpan((DateTime.Now.Ticks - dt1.Ticks)).Milliseconds));

                                }


                            });
                        }
                        //);

                      
                    });


                    if (stats.Any())
                    {

                        new SqlBulkCopyHelper().SqlBulkInsertRecords("TimeStatistics", stats.ToList(), new List<string> { "TimeStatisticId" }, true);
                        stats = new ConcurrentBag<TimeStatistic>();
                    }


                }

                finally
                {
                    if (db.Database.Connection.State == System.Data.ConnectionState.Open)
                    {
                        db.Database.Connection.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //scope.Dispose();
            }
            //}

            System.Diagnostics.Debug.WriteLine(String.Format("Updated Statistics in {0}s", new TimeSpan((DateTime.Now.Ticks - dt_0.Ticks)).TotalSeconds));

            return true;
        }




        public Decimal GetMeanAverage(List<Decimal> list)
        {
            try
            {
                var sublist = list.GetRange((list.Count / 4), list.Count / 2);
                return Math.Round(sublist.Average(), 1);
            }
            catch
            {

                var sublist = list;
                return Math.Round(sublist.Average(), 1);
            }


        }

        private bool SaveEntity<T>(T entity) where T : class
        {
            return SaveRecords(new List<T>() { entity });
        }

        private bool SaveRecords<T>(List<T> records) where T : class
        {

            AppStatsContext context = null;
            try
            {
                context = new AppStatsContext();
                context.Configuration.AutoDetectChangesEnabled = false;

                int count = 0;
                foreach (var entityToInsert in records)
                {
                    ++count;
                    context = AddToContext<T>(context, entityToInsert, count, 100, true);
                }

                context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }

            return true;

        }

        private AppStatsContext AddToContext<T>(AppStatsContext context, T entity, int count, int commitCount, bool recreateContext) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new AppStatsContext();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }


    }

    public static class AppStatsEntitiesExension
    {
        public static void ClearCache(this AppStatsContext context)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.GetType().GetMethod("ClearCache", FLAGS);

            method.Invoke(context, null);
        }
    }


}

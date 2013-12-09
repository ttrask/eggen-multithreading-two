using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
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

namespace AppStats.Helpers
{
    public class StatisticsHelper
    {
        //
        // GET: /StatisticsHelper/
        AppStatsEntities1 db = new AppStatsEntities1();


        public Boolean UpdateStatistics()
        {
            //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 15, 0)))
            //{
                try
                {

                    db.Database.ExecuteSqlCommand("DELETE FROM Stats.TimeStatistics");
                    //var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;

                    //objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Statistics.TimeStatistics]");

                    //get active set

                    List<byte> langIds = (from r in db.DropFiles select r.LanguageId).Distinct().ToList();
                    List<byte> envids = (from r in db.DropFiles select r.EnvironmentId).Distinct().ToList();
                    List<int> procCounts = (from r in db.Records select r.ProcessorCount).Distinct().ToList();
                    List<TimeType> timeTypes = (from t in db.TimeTypes select t).Distinct().ToList();
                    List<int> ns = (from r in db.Records select r.Size).Distinct().ToList();

                    

                    foreach (int langId in langIds)
                    {
                        foreach (int envid in envids)
                        {
                            //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(0, 15, 0)))
                            //{
                                //Parallel.ForEach(procCounts, procCount =>
                                foreach (int procCount in procCounts)
                                {
                                    DateTime batchTime = DateTime.Now;
                                    DateTime dt1, dt2;
                                    List<TimeStatistic> stats = new List<TimeStatistic>();

                                

                                    foreach (int size in ns)
                                    {

                                        System.Diagnostics.Debug.WriteLine(String.Format("Time Sets:{0}-{1}-{2}-{3}", langId, envid, procCount, size));


                                        dt1 = DateTime.Now;

                                        IEnumerable<Record> timeSets = (from  r in db.Records
                                                                      join d in db.DropFiles
                                                                      on r.DropFileId equals d.DropFileId
                                                                      where d.IsActive == true
                                                                      && r.Size == size
                                                                      && d.LanguageId == langId
                                                                      && d.EnvironmentId == envid
                                                                      && r.ProcessorCount == procCount
                                                                      select r);

                                       
                                        System.Diagnostics.Debug.WriteLine("Time To execute LINQ:" + (DateTime.Now.Ticks - dt1.Ticks));


                                        if (timeSets.Any())
                                        {

                                            //IEnumerable<Time> timeSets = list;

                                            Record set = timeSets.First();

                                            Dictionary<TimeType, IEnumerable<Record>> timeRecords = new Dictionary<TimeType, IEnumerable<Record>>();

                                            dt1 = DateTime.Now;

                                            foreach (IGrouping<TimeType, Record> timeSet in timeSets.GroupBy(t => t.TimeType))
                                            {
                                                timeRecords.Add(timeSet.Key, timeSet.Select(t => t));
                                            };

                                            System.Diagnostics.Debug.WriteLine("Time To Split Times" + timeSets.Count() + " by TimeType:" + (DateTime.Now.Ticks - dt1.Ticks));

                                            dt1 = DateTime.Now;
                                            foreach (TimeType tt in timeRecords.Keys)
                                            {
                                                List<double> times = timeRecords[tt].Select(t => Convert.ToDouble(t.TimeValue)).ToList();

                                                if (times.Any())
                                                {

                                                    times.Sort();


                                                    DescriptiveStatistics statVals = new MathNet.Numerics.Statistics.DescriptiveStatistics(times);

                                                    Record r = set;

                                                    TimeStatistic stat = new TimeStatistic()
                                                    {
                                                        BatchTime = batchTime,
                                                        LanguageId = Convert.ToByte(langId),
                                                        DatasetSize = r.Size,
                                                        ProcessorCount = r.ProcessorCount,
                                                        EnvironmentId = Convert.ToByte(envid),
                                                        TimeTypeId = tt.TimeTypeId,
                                                        Mean = Convert.ToDecimal(statVals.Mean),
                                                        Median = Convert.ToDecimal(Statistics.Median(times)),
                                                        Average = Convert.ToDecimal(times.Average()),
                                                        Q1Mean = Convert.ToDecimal(Statistics.LowerQuartile(times)),
                                                        Q3Mean = Convert.ToDecimal(Statistics.UpperQuartile(times)),
                                                        Min = Convert.ToDecimal(statVals.Minimum),
                                                        Max = Convert.ToDecimal(statVals.Maximum),
                                                        RecordCount = times.Count(),
                                                        MeanAverage = GetMeanAverage(times.ConvertAll<Decimal>(d => Convert.ToDecimal(d)))
                                                    };
                                                    stats.Add(stat);
                                                }
                                            }
                                            System.Diagnostics.Debug.WriteLine("Time To Generate Statistics:" + (DateTime.Now.Ticks - dt1.Ticks));
                                            timeRecords.Clear();
                                        }
                                    }

                                    SaveRecords(stats);
                                    stats.Clear();

                                    db = new AppStatsEntities1();
                                   
                                }

                            //    scope.Complete();
                            //}
                        }

                    }

                   

                    //scope.Complete();

                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //scope.Dispose();
                }
            //}
            return true;
        }

       


        public Decimal GetMeanAverage(List<Decimal> list)
        {
                try{    
                    var sublist = list.GetRange((list.Count / 4), list.Count / 2);
                    return Math.Round(sublist.Average(), 1);
                }
                catch{
                
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

            AppStatsEntities1 context = null;
            try
            {
                context = new AppStatsEntities1();
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

        private AppStatsEntities1 AddToContext<T>(AppStatsEntities1 context, T entity, int count, int commitCount, bool recreateContext) where T : class
        {
            context.Set<T>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new AppStatsEntities1();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }


    }

    public static class AppStatsEntitiesExension{
        public static void ClearCache(this AppStatsEntities1 context)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var method = context.GetType().GetMethod("ClearCache", FLAGS);

            method.Invoke(context, null);
        }
    }


}

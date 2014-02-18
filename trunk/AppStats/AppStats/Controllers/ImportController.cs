using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using AppStats.Models;
using AppStats.DataAccess;
using AppStats.Models.Enums;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.Sql;

namespace AppStats.Controllers
{
    public class ImportController : Controller
    {
        private static AppStatsContext db = new AppStatsContext();
        private static List<TimeType> _timeTypesCache;
        //
        // GET: /Import/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportData()
        {
            var model = new DropFile();

            return View(model);
        }

        [HttpPost]
        public ActionResult ImportData(DropFile df, HttpPostedFileBase uploadedFile)
        {
            try
            {
                List<KeyValuePair<String, int>> columnIndices = new List<KeyValuePair<String, int>>();

                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    if (uploadedFile.FileName.ToLower().EndsWith("csv"))
                    {


                        var fileName = Path.GetFileName(uploadedFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName.Substring(0, fileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMM.hhmm") + ".csv");

                        uploadedFile.SaveAs(path);

                        List<Record> records = new List<Record>();

                        List<Tuple<string, int, ColumnTypeEnum>> cols = GetColumnIndices(path);

                        if (cols == null)
                        {
                            throw new Exception("Invalid Header");
                        }

                        try
                        {
                            df.Filename = uploadedFile.FileName;
                            df.CraeteDtTm = DateTime.Now;
                            df.IsActive = true;
                            df.Environment = db.Environments.First(e => e.EnvinronmentId == df.EnvironmentId);
                            df.Language = db.Languages.First(e => e.LanguageId == df.LanguageId);

                            db.DropFiles.Add(df);
                            db.SaveChanges();

                            db.DropFileStores.Add(new DropFileStore() { DropFileId = df.DropFileId, DropFileRawData = System.IO.File.ReadAllBytes(path) });
                            db.SaveChanges();

                            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(path)))
                            {
                                sr.ReadLine();

                                Record r;
                                var cs = cols.Where(c => c.Item2 >= 0);

                                while (!sr.EndOfStream)
                                {
                                NewRecord:


                                    string[] vals = sr.ReadLine().Split(',');

                                    r = new Record() { DropFileId = df.DropFileId, CreateUser = "", ExecuteDtTm = DateTime.Now, LanguageId = df.LanguageId, EnvironmentId = df.EnvironmentId };

                                    foreach (Tuple<string, int, ColumnTypeEnum> col in cs)
                                    {


                                        if (col.Item3 == ColumnTypeEnum.TimeType)
                                        {
                                            //if time conversion fails go to next record.

                                            decimal d = 0;
                                            if (decimal.TryParse(vals[col.Item2], out d))
                                            {
                                                r.TimeValue = d;
                                            }
                                            else
                                            {
                                                //log an error and go to new record.
                                                goto NewRecord;
                                            }

                                            r.TimeTypeId = GetTimeTypeId(col.Item1);

                                            Record r2 = new Record() { DropFileId = r.DropFileId, EnvironmentId = r.EnvironmentId, LanguageId = r.LanguageId, TimeValue = r.TimeValue, CreateUser = r.CreateUser, ProcessorCount = r.ProcessorCount, Size = r.Size, TimeTypeId = r.TimeTypeId };

                                            records.Add(r2);
                                        }
                                        else
                                        {
                                            switch (col.Item1.ToLower())
                                            {
                                                case "n":
                                                    int n;
                                                    Int32.TryParse(vals[col.Item2], out n);
                                                    r.Size = n;
                                                    break;
                                                case "t":
                                                    int t;
                                                    Int32.TryParse(vals[col.Item2], out t);
                                                    r.ProcessorCount = t;
                                                    break;
                                            };
                                        }


                                    };


                                }

                                //these columns we don't want copied into the database on account of them being inherited by the ORM and not being represented in the db.
                                List<string> bannedColumns = new List<string> { "TimeType", "DropFile", "Language", "Environment", "RecordId" };

                                //add conditional for if using Sqlite.
                                new SqlBulkCopyHelper().SqlBulkInsertRecords("Records", records, bannedColumns, true);
                            }


                            //scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            //scope.Dispose();
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return View(df);
        }

        private Int64 GetTimeTypeId(string name)
        {

            if (_timeTypesCache == null)
            {
                _timeTypesCache = db.TimeTypes.ToList();
            }

            TimeType t = _timeTypesCache.Where(t2 => t2.Name.ToLower() == name.ToLower()).FirstOrDefault<TimeType>();

            if (t == null)
            {
                db.TimeTypes.Add(new TimeType() { Name = name });
                db.SaveChanges();
                _timeTypesCache = db.TimeTypes.ToList();

                return GetTimeTypeId(name);
            }

            return t.TimeTypeId;
        }

        private List<Tuple<string, int, ColumnTypeEnum>> GetColumnIndices(string file)
        {
            List<String> colNames = new List<string>() { "n", "t", "sorttime", "combinetime", "totaltime", "executedttm", "generationtime", "nocalltime", "arraycalltime" };

            List<Tuple<string, int, ColumnTypeEnum>> cols = new List<Tuple<string, int, ColumnTypeEnum>>();

            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                if (!sr.EndOfStream)
                {

                    List<String> topRow = sr.ReadLine().Split(',').Select(s => s.ToLower()).ToList();

                    colNames.ForEach(delegate(String s)
                    {
                        if (topRow.Contains(s.ToLower()))

                            topRow.ForEach(delegate(String name)
                            {
                                if (name.Trim().ToLower().CompareTo(s.Trim().ToLower()) == 0)
                                    cols.Add(new Tuple<string, int, ColumnTypeEnum>(s, topRow.IndexOf(name), s.ToLower().Contains("time") ? ColumnTypeEnum.TimeType : ColumnTypeEnum.NType));
                            });


                    });



                }

            }
            cols = cols.Distinct().ToList();
            return cols;


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
                context.Configuration.ValidateOnSaveEnabled = false;

                int count = 0;
                foreach (var entityToInsert in records)
                {
                    ++count;
                    context = AddToContext<T>(context, entityToInsert, count, 1000, true);
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
                    context.Configuration.ValidateOnSaveEnabled = false;

                }
            }

            return context;
        }





        public class TypeSwitch
        {
            Dictionary<Type, Action<object>> matches = new Dictionary<Type, Action<object>>();
            public TypeSwitch Case<T>(Action<T> action) { matches.Add(typeof(T), (x) => action((T)x)); return this; }
            public void Switch(object x) { matches[x.GetType()](x); }
        }
    }
}

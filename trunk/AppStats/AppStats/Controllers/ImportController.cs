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
using System.Data.SqlClient;
using System.Data.Sql;

namespace AppStats.Controllers
{
    public class ImportController : Controller
    {
        private static AppStatsEntities1 db = new AppStatsEntities1();
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

                                    r = new Record() { DropFileId = df.DropFileId, CreateDtTm = DateTime.Now, CreateUser = "", ExecuteDtTm = DateTime.Now, LanguageId = df.LanguageId, EnvironmentId = df.EnvironmentId };

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


                                SqlBulkInsertRecords(records);
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

        public bool SqlBulkInsertRecords(List<Record> recs)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    cn.Open();

                    List<string> bannedColumns = new List<string> { "TimeType", "DropFile", "Language", "Environment" };

                    DataTable tbl = recs.ToDataTable();

                    bannedColumns.ForEach(delegate(String bc)
                    {
                        if (tbl.Columns.Contains(bc))
                        {
                            tbl.Columns.Remove(bc);
                        }
                    });



                    using (SqlBulkCopy bulk = new SqlBulkCopy(cn))
                    {

                        bulk.DestinationTableName = "Records";
                        bulk.NotifyAfter = 1000;
                        bulk.SqlRowsCopied += new SqlRowsCopiedEventHandler(s_SqlRowsCopied);
                        bulk.WriteToServer(tbl);
                        bulk.Close();
                    }

                    cn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void s_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine("-- Copied {0} rows.", e.RowsCopied);
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
                    context.Configuration.ValidateOnSaveEnabled = false;

                }
            }

            return context;
        }



        private byte GetTimeTypeId(string name)
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

    }

    public static class ListExtender
    {
        public static DataTable ToDataTable<T>(this IList<T> data)
        {


            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];

                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

    }
}

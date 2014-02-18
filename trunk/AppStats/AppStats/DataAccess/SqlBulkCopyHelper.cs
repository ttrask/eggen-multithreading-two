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
using System.Threading;

namespace AppStats.DataAccess
{
    public class SqlBulkCopyHelper
    {

        // GET: /SqlBulkCopyHelper/

        private static AppStatsContext db = new AppStatsContext();
        private static List<TimeType> _timeTypesCache;

        private SQLiteConnection _sqliteCn;
        List<string> _bannedColumns = new List<string>();

        public SqlBulkCopyHelper(){

        }

        public SqlBulkCopyHelper(SQLiteConnection cn){
            _sqliteCn = cn;
        }

        public bool SqlBulkInsertRecords<T>(string dataTableName, List<T> recs, List<string> bannedColumns, bool useSqlite = false)
        {
            DateTime dt1 = DateTime.Now;

            if (bannedColumns != null)
            {
                _bannedColumns = bannedColumns;
            }

            

            DataTable tbl = recs.ToDataTable();

            _bannedColumns.ForEach(delegate(String bc)
            {
                if (tbl.Columns.Contains(bc))
                {
                    tbl.Columns.Remove(bc);
                }
            });

            try
            {
                if (useSqlite)
                {
                    if (_sqliteCn != null)
                    {
                        return SqliteBulkInsertRecords(dataTableName, tbl, _sqliteCn);
                    }
                    return SqliteBulkInsertRecords(dataTableName, tbl);
                }
                else
                {
                    return TSqlBulkBulkInsertRecords(dataTableName, tbl);
                }
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Time To Bulk Insert {0} records: {1}", tbl.Rows.Count,new TimeSpan(DateTime.Now.Ticks - dt1.Ticks).Milliseconds));
            }
        }

        private bool SqliteBulkInsertRecords<T>(string dbname, List<T> recs)
        {
            return TSqlBulkBulkInsertRecords(dbname, recs.ToDataTable());
        }

        private bool SqliteBulkInsertRecords(string dbname, DataTable tbl)
        {
            using (SQLiteConnection cn = new SQLiteConnection(db.Database.Connection.ConnectionString))
            {
                cn.Open();
                return SqliteBulkInsertRecords(dbname, tbl, cn);
                cn.Close();
            }
            return true;

        }

        private bool SqliteBulkInsertRecords(string dbname, DataTable tbl, SQLiteConnection cn)
        {
            string columns = "";
            string s = "";
            string values = "";
            bool cnIsOpen = false;

            while (cn.State == ConnectionState.Executing)
            {
                System.Diagnostics.Debug.WriteLine("Database is busy.  Sleeping 500ms");
                Thread.Sleep(1000);
            }

            foreach (DataColumn dc in tbl.Columns)
            {
                columns += dc.ColumnName + ",";

            }

            columns = columns.Substring(0, columns.LastIndexOf(','));

            using (var cmd = new SQLiteCommand(cn))
            {
                using (var txn = cn.BeginTransaction())
                {
                    foreach (DataRow dr in tbl.Rows)
                    {
                        values = "";

                        foreach (DataColumn dc in tbl.Columns)
                        {
                            switch (Type.GetTypeCode(dc.DataType))
                            {
                                case TypeCode.DateTime:
                                    s = "'" + ((DateTime)dr[dc]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                    break;
                                case TypeCode.String:
                                    s = "'" + dr[dc].ToString() + "'";
                                    break;
                                default:
                                    s = dr[dc].ToString();
                                    break;

                            }

                            values += s + ",";
                        }

                        values = values.Substring(0, values.LastIndexOf(','));

                        cmd.CommandText = String.Format("Insert into {0} ({1}) values ({2});", dbname, columns, values);

                        cmd.ExecuteNonQuery();
                    }

                    txn.Commit();
                }
            }
            return true;
        }

        private bool TSqlBulkBulkInsertRecords<T>(string dbname, List<T> recs)
        {
            return SqliteBulkInsertRecords(dbname, recs.ToDataTable());
        }

        private bool TSqlBulkBulkInsertRecords(string dataTableName, DataTable tbl)
        {
            using (SqlConnection cn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                cn.Open();

                using (SqlBulkCopy bulk = new SqlBulkCopy(cn))
                {

                    bulk.DestinationTableName = dataTableName;
                    bulk.NotifyAfter = 1000;
                    bulk.SqlRowsCopied += new SqlRowsCopiedEventHandler(s_SqlRowsCopied);
                    bulk.WriteToServer(tbl);
                    bulk.Close();
                }

                cn.Close();
            }
            return true;
        }
        
        static void s_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine("-- Copied {0} rows.", e.RowsCopied);
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

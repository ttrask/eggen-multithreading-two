using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppStats.Models;
<<<<<<< .mine
using AppStats.DataAccess;
//using EntityFramework.Extensions;
=======
using AppStats.DataAccess;
>>>>>>> .r4

namespace AppStats.Controllers
{
    public class DropFileController : Controller
    {
        private AppStatsContext db = new AppStatsContext();

        

        

        //
        // GET: /Default1/

        public ActionResult Index()
        {
            
            return View(db.DropFiles.ToList());
        }

        //
        // GET: /Default1/Details/5

        public ActionResult Details(int id = 0)
        {
            DropFile dropfile = db.DropFiles.Find(id);
            if (dropfile == null)
            {
                return HttpNotFound();
            }
            return View(dropfile);
        }

        //
        // GET: /Default1/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Default1/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DropFile dropfile)
        {
            if (ModelState.IsValid)
            {
                db.DropFiles.Add(dropfile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dropfile);
        }

        //
        // GET: /Default1/Edit/5

        public ActionResult Edit(int id = 0)
        {
            DropFile dropfile = db.DropFiles.Find(id);
            if (dropfile == null)
            {
                return HttpNotFound();
            }
            return View(dropfile);
        }

        //
        // POST: /Default1/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DropFile dropfile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dropfile).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dropfile);
        }

        //
        // GET: /Default1/Delete/5

        public ActionResult Delete(int id = 0)
        {

            DropFile dropfile = db.DropFiles.Find(id);

            return View(dropfile);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DropFile dropfile = db.DropFiles.Find(id);

            //if (dropfile == null)
            //{
            //    return HttpNotFound();
            //}
            db.Database.ExecuteSqlCommand("PRAGMA foreign_keys = true;");
            db.Database.ExecuteSqlCommand(String.Format("Delete from DropFiles where DropFileId = {0}", id));

            //db.DropFileStores(t => t.DropFileId == id);

            //db.Records.Delete(r => r.DropFileId == id);

            //db.DropFiles.Where(d => d.DropFileId == id).ToList().ForEach(d => db.DropFiles.Remove(d));

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
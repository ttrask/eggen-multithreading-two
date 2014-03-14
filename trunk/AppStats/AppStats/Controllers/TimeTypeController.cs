using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppStats.Models;
using AppStats.DataAccess;

namespace AppStats.Controllers
{
    public class TimeTypeController : Controller
    {
        private AppStatsContext db = new AppStatsContext();

        //
        // GET: /TimeType/

        public ActionResult Index()
        {
            return View(db.TimeTypes.ToList());
        }

        //
        // GET: /TimeType/Details/5

        public ActionResult Details(long id = 0)
        {
            TimeType timetype = db.TimeTypes.Find(id);
            if (timetype == null)
            {
                return HttpNotFound();
            }
            return View(timetype);
        }

        //
        // GET: /TimeType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /TimeType/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TimeType timetype)
        {
            if (ModelState.IsValid)
            {
                db.TimeTypes.Add(timetype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(timetype);
        }

        //
        // GET: /TimeType/Edit/5

        public ActionResult Edit(long id = 0)
        {
            TimeType timetype = db.TimeTypes.Find(id);
            if (timetype == null)
            {
                return HttpNotFound();
            }
            return View(timetype);
        }

        //
        // POST: /TimeType/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TimeType timetype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(timetype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(timetype);
        }

        //
        // GET: /TimeType/Delete/5

        public ActionResult Delete(long id = 0)
        {
            TimeType timetype = db.TimeTypes.Find(id);
            if (timetype == null)
            {
                return HttpNotFound();
            }
            return View(timetype);
        }

        //
        // POST: /TimeType/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            TimeType timetype = db.TimeTypes.Find(id);
            db.TimeTypes.Remove(timetype);
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
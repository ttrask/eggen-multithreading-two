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
    public class EnvironmentController : Controller
    {
        private AppStatsContext db = new AppStatsContext();

        //
        // GET: /Default1/

        public ActionResult Index()
        {
            return View(db.Environments.ToList());
        }

        //
        // GET: /Default1/Details/5

        public ActionResult Details(byte id = 0)
        {
            AppStats.Models.Environment envrionment = db.Environments.Find(id);
            if (envrionment == null)
            {
                return HttpNotFound();
            }
            return View(envrionment);
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
        public ActionResult Create(AppStats.Models.Environment envrionment)
        {
            if (ModelState.IsValid)
            {
                db.Environments.Add(envrionment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(envrionment);
        }

        //
        // GET: /Default1/Edit/5

        public ActionResult Edit(byte id = 0)
        {
            AppStats.Models.Environment envrionment = db.Environments.Find(id);
            if (envrionment == null)
            {
                return HttpNotFound();
            }
            return View(envrionment);
        }

        //
        // POST: /Default1/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppStats.Models.Environment envrionment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(envrionment).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(envrionment);
        }

        //
        // GET: /Default1/Delete/5

        public ActionResult Delete(byte id = 0)
        {
            AppStats.Models.Environment envrionment = db.Environments.Find(id);
            if (envrionment == null)
            {
                return HttpNotFound();
            }
            return View(envrionment);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            AppStats.Models.Environment envrionment = db.Environments.Find(id);
            db.Environments.Remove(envrionment);
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
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
    public class LanguageController : Controller
    {
        private AppStatsContext db = new AppStatsContext();

        //
        // GET: /Language/

        public ActionResult Index()
        {
            return View(db.Languages.ToList());
        }

        //
        // GET: /Language/Details/5

        public ActionResult Details(byte id = 0)
        {
            Language language = db.Languages.Find(id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // GET: /Language/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Language/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Language language)
        {
            if (ModelState.IsValid)
            {
                db.Languages.Add(language);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(language);
        }

        //
        // GET: /Language/Edit/5

        public ActionResult Edit(byte id = 0)
        {
            Language language = db.Languages.Find(id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // POST: /Language/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Language language)
        {
            if (ModelState.IsValid)
            {
                db.Entry(language).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(language);
        }

        //
        // GET: /Language/Delete/5

        public ActionResult Delete(byte id = 0)
        {
            Language language = db.Languages.Find(id);
            if (language == null)
            {
                return HttpNotFound();
            }
            return View(language);
        }

        //
        // POST: /Language/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            Language language = db.Languages.Find(id);
            db.Languages.Remove(language);
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
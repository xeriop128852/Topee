using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Topee.Models;

namespace Topee.Controllers
{
    public class favorite_storeController : Controller
    {
        private TopeeDatabaseEntities db = new TopeeDatabaseEntities();

        [HttpPost]
        public void Add_Follow_Store(int id)
        {
            var info = User.Identity.Name;

            var userid = (from a in db.User
                          where a.username == info
                          select a.id).FirstOrDefault();

            var twice = (from a in db.favorite_store
                         where a.sid == id && userid == a.uid
                         select a).FirstOrDefault();

            if(twice == null)
            {
                var follow = new favorite_store()
                {
                    sid = id,
                    uid = userid
                };

                db.favorite_store.Add(follow);
                db.SaveChanges();
            }
        }

        // GET: favorite_store
        [Authorize]
        public ActionResult Index()
        {
            var info = User.Identity.Name;
            var favorite_store = (from a in db.User
                                  from b in db.favorite_store
                                  where a.username == info && b.uid == a.id
                                  select b).ToList();
            if (favorite_store.Count() == 0)
            {
                ViewBag.message = "無追蹤店家";
            }
            return View(favorite_store);
        }

        // GET: favorite_store/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            favorite_store favorite_store = db.favorite_store.Find(id);
            if (favorite_store == null)
            {
                return HttpNotFound();
            }
            return View(favorite_store);
        }

        // POST: favorite_store/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            favorite_store favorite_store = db.favorite_store.Find(id);
            db.favorite_store.Remove(favorite_store);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

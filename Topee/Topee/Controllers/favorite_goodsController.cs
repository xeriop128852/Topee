using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Topee.Models;

namespace Topee.Controllers
{
    public class favorite_goodsController : Controller
    {
        private TopeeDatabaseEntities db = new TopeeDatabaseEntities();

        [HttpPost]
        public void Add_Like_Goods(int id)
        {
            var username = User.Identity.Name;

            var userid = (from a in db.User
                          where a.username == username
                          select a.id).FirstOrDefault();

            var twice = (from a in db.favorite_goods
                         where a.gid == id && a.uid == userid
                         select a).FirstOrDefault();

            if(twice == null)
            {
                var like = new favorite_goods()
                {
                    gid = id,
                    uid = userid
                };

                db.favorite_goods.Add(like);
                db.SaveChanges();
            }
        }


        // GET: favorite_goods
        [Authorize]
        public ActionResult Index()
        {
            var info = User.Identity.Name;

            var userid = (from a in db.User
                          where a.username == info
                          select a.id).FirstOrDefault();

            var favorite_goods = (from a in db.favorite_goods
                                  where a.uid == userid
                                  select a).ToList();

            //var favorite_goods = db.favorite_goods.Include(f => f.Goods).Include(f => f.User);
            if (favorite_goods.Count() == 0)
            {
                ViewBag.message = "無收藏商品";
            }
            return View(favorite_goods);
        }


        // GET: favorite_goods/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            favorite_goods favorite_goods = db.favorite_goods.Find(id);
            if (favorite_goods == null)
            {
                return HttpNotFound();
            }
            return View(favorite_goods);
        }

        // POST: favorite_goods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            favorite_goods favorite_goods = db.favorite_goods.Find(id);
            db.favorite_goods.Remove(favorite_goods);
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

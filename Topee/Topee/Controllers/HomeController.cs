using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;
using Topee.Models;

namespace Topee.Controllers
{
    public class HomeController : Controller
    {
        private TopeeDatabaseEntities db = new TopeeDatabaseEntities();

        public static string Encrypt(string plainText)
        {
            byte[] data = Encoding.Default.GetBytes(plainText);
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] result = sha256.ComputeHash(data);

            return Convert.ToBase64String(result);
        }
        public ActionResult Login(InputForm form)
        {
            if(form.Username == null || form.Password == null)
            {
                    ViewBag.message = "輸入不可為空";
                    return View();
            }
            ViewBag.username = form.Username;
            ViewBag.password = form.Password;
            var r = (from a in db.User
                     where a.username == form.Username
                     select a).FirstOrDefault();
            if (r == null)
            {
                ViewBag.message = "帳號密碼錯誤";
                return View();
            }
            string SaltAndFormPassword = string.Concat(r.sault, form.Password);
            string FormpPassword = Encrypt(SaltAndFormPassword);
            ViewBag.inputHPW = FormpPassword;
            ViewBag.savedHPW = r.password;
            if (string.Compare(FormpPassword, r.password, false) == 0)
            {
                Session.RemoveAll();
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    form.Username,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    "user data",
                    FormsAuthentication.FormsCookiePath);

                string encTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                    encTicket);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                return RedirectToAction("Home");
            }
            else ViewBag.message = "帳號密碼錯誤";
            return View();
        }

        [Authorize]
        public ActionResult Home()
        {
            var ClassifyList = (from a in db.Classify
                                select a.name).Distinct().ToList();
            ViewBag.classify = new SelectList(ClassifyList);

            var sub_ClassifyList = "";
            ViewBag.sub_classify = new SelectList(sub_ClassifyList);

            return View();
        }

        [HttpPost]
        public ActionResult Select_Search(string name, string sub_name)
        {
            var r = (from a in db.Classify
                     where a.name == name && a.sub_name == sub_name
                     select a.id).FirstOrDefault();

            var s = (from b in db.hash_tag
                     from c in db.Goods
                     where b.cid == r && c.id == b.gid
                     select c).ToList();

            ViewData["result"] = s;
            return PartialView("_SelectSearch");
        }

        [HttpPost]
        public ActionResult Keyword_Search(string keyword)
        {
            var s = (from c in db.Goods
                     where c.name.Contains(keyword)
                     select c).ToList();

            ViewData["result"] = s;
            return PartialView("_KeywordSearch");
        }

        [HttpPost]
        public ActionResult Page_Default()
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int r = rnd.Next(1,16);

            var s = (from a in db.Goods
                     where a.id%16 == r
                     select a).ToList();

            ViewData["result"] = s;
            return PartialView("_PageDefault");
        }


        [Authorize]
        public ActionResult Shop_car()
        {
            ViewBag.message = "";
            var info = User.Identity.Name;
            var car = (from a in db.shop_car
                       from b in db.User
                       where b.username == info
                       where a.uid == b.id
                       select a).ToList();
            if(car.Count == 0)
                ViewBag.message = "購物車為空";

            var good = (from a in car
                       from c in db.Goods
                       where a.gid == c.id
                       select c).ToList();

            var total_price = (from a in good
                         select a.price).Sum();

            ViewBag.price = total_price;

            ViewData["result"] = car;
;
            return View(good);
        }

        public ActionResult Shop_car_Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            shop_car goods = db.shop_car.Find(id);

            if (goods == null)
            {
                return HttpNotFound();
            }
            
            return View(goods);
        }

        [HttpPost, ActionName("Shop_car_Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Shop_car_DeleteConfirmed(int id)
        {
            shop_car goods = db.shop_car.Find(id);

            db.shop_car.Remove(goods);
            db.SaveChanges();
            return RedirectToAction("Shop_car");
        }

        
        public ActionResult Bill()
        {
            var info = User.Identity.Name;

            var r = (from a in db.User
                     from b in db.shop_car
                     where a.username == info && a.id == b.uid
                     select b).ToList();
            ViewBag.coin = (int)(from a in db.User
                            where a.username == info
                            select a.coin).FirstOrDefault();
            return View(r);
        }
        [HttpPost, ActionName("Bill")]
        [ValidateAntiForgeryToken]
        public ActionResult BillConfirmed(InputForm form)
        {
            var info = User.Identity.Name; 
            var r = (from a in db.User
                     from b in db.shop_car
                     where a.username == info && a.id == b.uid
                     select b).ToList(); 
            for(int i=0; i<r.Count(); i++)
            {
                db.shop_car.Remove(r[i]);
            }
            
            var user = (from a in db.User
                          where a.username == info
                          select a).FirstOrDefault();

            decimal coin = user.coin;
            coin = coin - form.Discount + form.Bonus;
            user.coin = coin;
            db.SaveChanges();

            return RedirectToAction("Shop_car");
        }

        public ActionResult Goods_Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Goods goods = db.Goods.Find(id);
            if (goods == null)
            {
                return HttpNotFound();
            }
            
            return View(goods);
        }

        public ActionResult Store_page(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Store store = db.Store.Find(id);
            if (store == null)
            {
                return HttpNotFound();
            }

            return View(store);
        }

        [HttpPost]
        public ActionResult Store_Goods(int id)
        {
            var r = (from a in db.Goods
                     where a.sid == id
                     select a).ToList();

            ViewData["result"] = r;
            return PartialView("_StoreGoods");
        }

        [HttpPost]
        public void Add_Shop_car(int id)
        {
            var username = User.Identity.Name;
            var userid = (from a in db.User
                          where a.username == username
                          select a.id).FirstOrDefault();

            var car = new shop_car()
            {
                gid = id,
                uid = userid
            };
            
            db.shop_car.Add(car);
            db.SaveChanges();
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // GET: Users
        public ActionResult Index()
        {

            return View(db.User.ToList());
        }

        public ActionResult Details()
        {
            var username = User.Identity.Name;

            var user = (from a in db.User
                        where a.username == username
                        select a).FirstOrDefault();

            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,username,password,coin,sault")] User user, InputForm form)
        {
            if (ModelState.IsValid)
            {
                var r = (from a in db.User
                         where a.username == user.username
                         select a).FirstOrDefault();

                if (r != null)
                {
                    ViewBag.message = "帳戶名已被使用";
                    return View();
                }

                if (string.Compare(user.password, form.Password_confirm, false) != 0)
                {
                    ViewBag.message = "密碼錯誤";
                    return View();
                }

                user.sault = Guid.NewGuid();
                string SaltAndPassword = String.Concat(user.sault, user.password);
                string hashedPW = Encrypt(SaltAndPassword);
                user.password = hashedPW;
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,username,password,coin,sault")] User user, InputForm form)
        {
            var username = User.Identity.Name;
            var new_info = (from a in db.User
                     where a.username == username
                            select a).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (string.Compare(user.password, form.Password_confirm, false) != 0)
                {
                    ViewBag.message = "密碼錯誤";
                    return View(user);
                }

                new_info.sault = Guid.NewGuid();
                string SaltAndPassword = String.Concat(new_info.sault, user.password);
                string hashedPW = Encrypt(SaltAndPassword);
                new_info.password = hashedPW;
                db.SaveChanges();

                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FormsAuthentication.SignOut();

            User user = db.User.Find(id);
            db.User.Remove(user);
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


        public JsonResult Classify(string name)
        {
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var sub = this.Getsub(name);
                if (sub.Count() > 0)
                {
                    foreach (var a in sub)
                    {
                        items.Add( new KeyValuePair<string, string>( a.id.ToString(), a.sub_name));
                    }
                }
            }
            return this.Json(items);
        }

        private IEnumerable<Classify> Getsub(string name)
        {
            var query = (from a in db.Classify
                         where a.name == name
                         select a).ToList();
            return query;
        }

    }
}

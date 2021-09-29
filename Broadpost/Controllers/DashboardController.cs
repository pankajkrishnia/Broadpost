using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Broadpost.Models;
using System.Collections;

namespace Broadpost.Controllers
{
    public class DashboardController : Controller
    {
        private int _sessionUserId;
        private bool isSessionValid()
        {
            _sessionUserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            byte[] data;
            return HttpContext.Session.TryGetValue("UserId", out data);
        }

        //Dashboard Index============================================================================
        public ActionResult Index()
        {
            if( isSessionValid() )
            {
                using(var db = new BroadpostDbContext())
                {
                    var userData = db.Users.FirstOrDefault(u => u.UserId == _sessionUserId);
                    return View(userData);
                }
            }
            return RedirectToAction("Login", "User");
        }


            









//=====================================================================================================================================
        //User Profile============================================================================
        public ActionResult Profile()
        {
            if (isSessionValid())
            {
                using(var db = new BroadpostDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.UserId == _sessionUserId);
                    return View(user);
                }
            }
            return RedirectToAction("Login","User");
        }


        //Edit User Profile============================================================================
        public ActionResult EditProfile(int? id)
        {
            if (isSessionValid())
            {
                using (var db = new BroadpostDbContext())
                {
                    var entity = db.Users.FirstOrDefault(u=>u.UserId == id);
                    if (entity == null)
                    {
                        return NotFound();
                    }
                    return View(entity);
                }
            }
            return RedirectToAction("Login", "User");
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult EditProfile(int id, User user)
        {
            if (isSessionValid())
            {
                if (id == user.UserId)
                {
                    if (ModelState.IsValid)
                    {
                        using (var db = new BroadpostDbContext())
                        {
                            var entity = db.Users.FirstOrDefault(c => c.UserId == id);
                            entity.UserName = user.UserName;
                            entity.Email = user.Email;
                            entity.Region = user.Region;
                            entity.UpdatedAt = DateTime.Now;

                            db.SaveChanges();

                            return RedirectToAction(nameof(Profile));
                        }
                    }
                    return View(user);
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Login", "User");
        }


    }
}

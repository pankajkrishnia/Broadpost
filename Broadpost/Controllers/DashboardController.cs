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
                if(id == _sessionUserId)
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
                return RedirectToAction(nameof(Profile));
            }
            return RedirectToAction("Login", "User");
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult EditProfile(User user)
        {
            if (isSessionValid())
            {
                if (user.UserId == _sessionUserId)
                {
                    if (ModelState.IsValid)
                    {
                        using (var db = new BroadpostDbContext())
                        {
                            var entity = db.Users.FirstOrDefault(c => c.UserId == user.UserId);
                            entity.UserName = user.UserName;
                            entity.Email = user.Email;
                            entity.Region = user.Region;
                            entity.Gender = user.Gender;
                            entity.Age = user.Age;
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


        //Change Password============================================================================
        public ActionResult ChangePassword(int? id)
        {
            if (isSessionValid())
            {
                if(id == _sessionUserId)
                {
                    ChangePassword changePassword = new ChangePassword
                    {
                        UserId = (int)id
                    };
                    return View(changePassword);
                }
                return RedirectToAction("Profile");
            }
            return RedirectToAction("Login", "User");
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        public ActionResult ChangePassword(ChangePassword userPassword)
        {
            if (isSessionValid())
            {
                if(userPassword.UserId == _sessionUserId)
                {
                    using (var db = new BroadpostDbContext())
                    {
                        var entity = db.Users.FirstOrDefault(u => u.UserId == _sessionUserId);
                        if(entity.Password == userPassword.CurrentPassword)
                        {
                            entity.Password = userPassword.NewPassword;
                            entity.PasswordVerify = userPassword.PasswordVerify;

                            db.SaveChanges();

                            ViewBag.message = "Password changed successfully";
                            return View();
                        }
                        ViewBag.message = "Current Password is Wrong";
                        return View();
                    }
                }
                return RedirectToAction("Profile");
            }
            return RedirectToAction("Login", "User");
        }
    }

}

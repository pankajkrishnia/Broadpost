﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Broadpost.Models;
using Newtonsoft.Json;

namespace Broadpost.Controllers
{
    public class UserController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        //User Login============================================================================
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            using(var db = new BroadpostDbContext())
            {
                var entity = db.Users.FirstOrDefault(u => u.UserName == user.UserName && u.Password == user.Password);

                if(entity == null)
                {
                    ViewBag.Message = "Invalid Username/Password";
                    return View(user);
                }

                HttpContext.Session.SetString("UserId", entity.UserId.ToString());
                HttpContext.Session.SetString("UserName", entity.UserName.ToString());
                return RedirectToAction("Index","Dashboard");
            }
        }



        //User Logout============================================================================
        public ActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction(nameof(Login));
        }



        //User Register============================================================================
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                using(var db = new BroadpostDbContext())
                {
                    var isUserExist = db.Users.FirstOrDefault(u => u.Email == user.Email || u.UserName == user.UserName);
                    if(isUserExist == null)
                    {
                        db.Users.Add(user);
                        db.SaveChanges();

                        return RedirectToAction(nameof(Login));
                    }
                    else if(db.Users.FirstOrDefault(u => u.Email == user.Email) != null)
                    {
                        ViewBag.Message = "Email is already registered";
                        return View(user);
                    }

                    ViewBag.Message = "User Name is already taken";
                    return View(user);
                }
            }
            return View(user);
        }

        
        public string getChannelUsers([FromBody] ChannelIdBinderr obj)
        {
            using(var db = new BroadpostDbContext())
            {
                var channelId = obj.ChannelId;

                var entities =  from u in db.Users
                                where !(from cu in db.ChannelUsers
                                        where cu.ChannelId == Convert.ToInt32(channelId)
                                        select cu.UserId)
                                        .Contains(u.UserId) &&
                                        !(from i in db.Invitations
                                          select i.ReceverUserId)
                                          .Contains(u.UserId)
                              select new 
                              { 
                                u.UserId,
                                u.UserName
                              };

                return JsonConvert.SerializeObject(entities);

            }
        }
    }

    public class ChannelIdBinderr
    {
        public string ChannelId { get; set; }
    }


}

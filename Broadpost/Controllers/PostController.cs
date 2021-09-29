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
    public class PostController : Controller
    {
        private int _sessionUserId;
        private int _channelId;
        private bool isSessionValid()
        {
            _sessionUserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            _channelId = Convert.ToInt32(HttpContext.Session.GetString("ChannelId"));
            byte[] data;
            return HttpContext.Session.TryGetValue("UserId", out data);
        }

        //Channel Posts============================================================================
        public ActionResult Index()
        {
            if (isSessionValid())
            {
                using (var db = new BroadpostDbContext())
                {
                    var posts = from p in db.Posts
                                 where p.ChannelId == _channelId
                                 select p;

                    var postsInfo = (from u in db.Users
                                     join p in posts
                                     on u.UserId equals p.UserId
                                     select new UserPost
                                     {
                                         user = u,
                                         post = p
                                     }).ToList();

                    ViewBag.postsInfo = postsInfo;
                    ViewBag.ChannelName = db.Channels.FirstOrDefault(c => c.ChannelId == _channelId).ChannelName;
                    return View();
                }
            }
            return RedirectToAction("Login", "User");
        }
        public ActionResult SetChannelId(int id)
        {
            HttpContext.Session.SetString("ChannelId", id.ToString());
            return RedirectToAction(nameof(Index));
        }


        //Create Post============================================================================
        public ActionResult CreatePost()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreatePost(Post post)
        {
            if (isSessionValid())
            {
                if (ModelState.IsValid)
                {
                    using(var db = new BroadpostDbContext())
                    {
                        //Updating total post in channel
                        var channelEntity = db.Channels.FirstOrDefault(c => c.ChannelId == _channelId);
                        channelEntity.TotalPost++;

                        //Adding Post
                        post.UserId = _sessionUserId;
                        post.ChannelId = _channelId;

                        db.Posts.Add(post);
                        db.SaveChanges();

                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(post);
            }
            return RedirectToAction("Login", "User");
        }

        //Edit Post============================================================================
        public ActionResult EditPost(int? id)
        {
            if (isSessionValid())
            {
                using(var db = new BroadpostDbContext())
                {
                    var entity = db.Posts.FirstOrDefault(p=> p.PostId == id);
                    if(entity == null)
                    {
                        return NotFound();
                    }
                    return View(entity);
                }
            }
            return RedirectToAction("Login", "User");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditPost(int id, Post post)
        {
            if (isSessionValid())
            {
                if(id == post.PostId)
                {
                    if (ModelState.IsValid)
                    {
                        using(var db = new BroadpostDbContext())
                        {
                            var entity = db.Posts.FirstOrDefault(p => p.PostId == post.PostId);
                            entity.PostMessage = post.PostMessage;
                            entity.UpdatedAt = DateTime.Now;

                            db.SaveChanges();

                            return RedirectToAction("Index");
                        }
                    }
                    return View(post);
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login","User");
        }

        //Delete Post============================================================================
        public ActionResult DeletePost(int id)
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeletePost(int id, Post post)
        {
            return View();
        }
    }

    public class UserPost
    {
        public User user { get; set; }
        public Post post { get; set; }
    }
}

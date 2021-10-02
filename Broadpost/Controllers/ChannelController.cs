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
    public class ChannelController : Controller
    {
        private int _sessionUserId;
        private string _sessionUserName;

        private bool isSessionValid()
        {
            _sessionUserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            _sessionUserName = HttpContext.Session.GetString("UserName");
            byte[] data;
            return HttpContext.Session.TryGetValue("UserId", out data);
        }

        //Channel Index============================================================================
        public ActionResult Index()
        {
            if (isSessionValid())
            {
                using (var db = new BroadpostDbContext())
                {
                    var userPersonalChannels = db.Channels.Where(c => c.UserId == _sessionUserId).ToList();

                    var userJoinedChannelsId = from cu in db.ChannelUsers
                                               where cu.UserId == _sessionUserId 
                                               select cu;

                    var userJoinedChannels = (from c in db.Channels
                                              join ci in userJoinedChannelsId
                                              on c.ChannelId equals ci.ChannelId
                                              where c.UserId != _sessionUserId
                                              select c).ToList();

                    var userChannels = new ArrayList() { userPersonalChannels, userJoinedChannels };

                    ViewBag.ChannelCreatedAndJoinedList = userChannels;
                    return View();
                }
            }
            return RedirectToAction("Login", "User");
        }

        //Create Channel============================================================================
        public ActionResult CreateChannel()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateChannel(Channel channel)
        {
            if (isSessionValid())
            {
                if (ModelState.IsValid)
                {
                    using (var db = new BroadpostDbContext())
                    {
                        var entity = db.Channels.FirstOrDefault(c => c.ChannelName == channel.ChannelName);
                        if (entity == null)
                        {
                            //adding new channel
                            channel.UserId = _sessionUserId;
                            channel.Admin = _sessionUserName;

                            db.Channels.Add(channel);
                            db.SaveChanges();


                            //updating ChannelUsers
                            var ch = db.Channels.FirstOrDefault(c => c.ChannelName == channel.ChannelName);
                            var channelUser = new ChannelUser();
                            channelUser.UserId = ch.UserId;
                            channelUser.ChannelId = ch.ChannelId;

                            db.ChannelUsers.Add(channelUser);
                            db.SaveChanges();

                            return RedirectToAction(nameof(Index));
                        }
                        ViewBag.Message = "Channel Name already exist";
                        return View(channel);
                    }
                }
                return View(channel);
            }
            return RedirectToAction("Login", "User");
        }



        //Edit Channel============================================================================
        public ActionResult EditChannel(int? id)
        {
            if (isSessionValid())
            {
                using (var db = new BroadpostDbContext())
                {
                    var entity = db.Channels.FirstOrDefault(c => c.ChannelId == id);
                    if (entity == null)
                    {
                        return NotFound();
                    }
                    else if (entity.UserId != _sessionUserId)
                    {
                        ViewBag.Message = "You have no Previlages to update this channel";
                    }
                    return View(entity);
                }
            }
            return RedirectToAction("Login", "User");
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult EditChannel(int id, Channel channel)
        {
            if (isSessionValid())
            {
                if (id == channel.ChannelId && Convert.ToInt32(channel.UserId) == _sessionUserId)
                {
                    if (ModelState.IsValid)
                    {
                        using (var db = new BroadpostDbContext())
                        {
                            var entity = db.Channels.FirstOrDefault(c => c.ChannelId == id);
                            entity.ChannelName = channel.ChannelName;
                            entity.ChannelDesc = channel.ChannelDesc;
                            entity.UpdatedAt = DateTime.Now;

                            db.SaveChanges();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                    return View(channel);
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Login", "User");
        }

        //Delete Channel============================================================================
        public ActionResult DeleteChannel(int? id)
        {
            //Pass----panding
            return View();
        }

    }
}

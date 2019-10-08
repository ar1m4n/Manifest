using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Manifest.Models;
using Manifest.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Manifest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);

            if(!_userManager.IsInRoleAsync(currentUser, "ok").Result)
            {
                ViewData["error"] = true;
                return View();
            }

            var users = _userManager.Users
                .Include(x => x.CommentsFrom)
                    .ThenInclude(x => x.ToUser)
                .Select(x => new ApplicationUser {
                    FbName = x.FbName,
                    Id = x.Id,
                    FbProfilePicUrl = x.FbProfilePicUrl,
                    FbProfilePicLargeUrl = x.FbProfilePicLargeUrl,
                    Email = x.Email,
                    CommentsFrom = x.CommentsFrom.Where(c => c.FromUserId == currentUser.Id).ToList(),
                }).Where(x => x.Email != User.Identity.Name).ToList();

            foreach(var c in users)
            {
                if(c.CommentsFrom.Count() == 0)
                    c.CommentsFrom.Add(new ApplicationUserComment{FromUserId = currentUser.Id, ToUserId = c.Id, ToUser = c});
            }
            return View(users);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeComment(ApplicationUserCommentModel appComment)
        {
            if(ModelState.IsValid)
            {
                var found = await _context.UserComments.Include(x => x.ToUser)
                    .FirstOrDefaultAsync(x => x.FromUserId == appComment.FromId && x.ToUserId == appComment.ToId);

                if(found != null)
                {
                    found.Comment = appComment.Comment;
                    _context.UserComments.Update(found);
                }
                else
                {
                    var a = await _context.UserComments.AddAsync(new ApplicationUserComment{ FromUserId = appComment.FromId, ToUserId= appComment.ToId, Comment = appComment.Comment });
                }

                await _context.SaveChangesAsync();

                if(User.IsInRole("admin"))
                {
                    var toUser = await _userManager.FindByIdAsync(appComment.ToId);
                    bool userInRole = await _userManager.IsInRoleAsync(toUser, "ok");
                    if(!appComment.IsInOkRole && userInRole)
                    {
                        await _userManager.RemoveFromRoleAsync(toUser, "ok");
                    }
                    else if(appComment.IsInOkRole && !userInRole)
                    {
                        await _userManager.AddToRoleAsync(toUser, "ok");
                    }
                }

            }
            return RedirectToAction("Index");
        }
    }
}

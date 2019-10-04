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

namespace Manifest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        ApplicationUser m_currentUser;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if(m_currentUser == null)
                m_currentUser = await _userManager.GetUserAsync(User);

            var users = _userManager.Users
                .Include(x => x.CommentsFrom)
                .Select(x => new ApplicationUser {
                    FbName = x.FbName,
                    Id = x.Id,
                    FbProfilePicUrl = x.FbProfilePicUrl,
                    Email = x.Email,
                    CommentsFrom = x.CommentsFrom.Where(c => c.FromUserId == m_currentUser.Id).ToList()
                }).ToList();

            foreach(var c in users)
            {
                if(c.CommentsFrom.Count() == 0)
                    c.CommentsFrom.Add(new ApplicationUserComment{FromUserId = m_currentUser.Id, ToUserId = c.Id});
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
        public async Task<IActionResult> ChangeComment([Bind("FromUserId,ToUserId,Comment")]ApplicationUserComment appComment)
        {
            if(ModelState.IsValid)
            {
                var found = await _context.UserComments.FindAsync(appComment.FromUserId, appComment.ToUserId);
                if(found != null)
                {
                    found.Comment = appComment.Comment;
                    _context.UserComments.Update(found);
                }
                else
                {
                    await _context.UserComments.AddAsync(appComment);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}

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

            var users = await _userManager.Users
                .Include(x => x.CommentsFrom)
                    .ThenInclude(x => x.ToUser)
                .Select(x => new ApplicationUser {
                    FbName = x.FbName,
                    Id = x.Id,
                    FbProfilePicUrl = x.FbProfilePicUrl,
                    FbProfilePicLargeUrl = x.FbProfilePicLargeUrl,
                    Email = x.Email,
                    CommentsFrom = x.CommentsFrom.Where(c => c.FromUserId == currentUser.Id).Select(y => new ApplicationUserComment{
                        FromUser = currentUser, ToUser = x, FromUserId = currentUser.Id, ToUserId = x.Id, Comment = y.Comment
                    }).ToList(),
                }).Where(x => x.Email != User.Identity.Name).ToListAsync();

            foreach(var c in users)
            {
                if(c.CommentsFrom.Count() == 0)
                    c.CommentsFrom.Add(new ApplicationUserComment{FromUserId = currentUser.Id, ToUserId = c.Id, ToUser = c});
            }
            return View(users);
        }

        [Authorize(Roles="admin")]
        public async Task<IActionResult> IndexAdmin()
        {
            var users = await _context.Users
                .Include(x => x.CommentsTo)
                .Select(x => x).ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Dogovor()
        {
            var model = new ContractModel {
                Users = await _context.Users
                    .Include(x => x.CommentsTo)
                        .ThenInclude(x => x.FromUser)
                    .Include(x => x.CommentsTo)
                        .ThenInclude(x => x.ToUser)
                    .Select(x => x).ToListAsync(),
                User = await _context.Users
                    .Include(x => x.CommentsTo)
                        .ThenInclude(x => x.FromUser)
                    .Include(x => x.CommentsTo)
                        .ThenInclude(x => x.ToUser)
                    .Where(x => x.Email == User.Identity.Name)
                    .SingleAsync(),
                Date = new DateTime(2019, 10, 12).ToString("dd.MM.yyyy"),
                Address = @"Град Сапарева Баня, ресторант ""Рилски Езера""",
                NameBG = "БЗДРНЦ",
                NameEN = "Bezdarnici",
                BzdrncCity = "Дупница",
                BzdrncAddress = "улица Венелин 69"
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeComment([Bind("ToId,FromId,Comment,IsInOkRole")]ApplicationUserCommentModel appComment)
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> ChangeCommentAdmin([Bind("FromUserId,ToUserId,Comment")]ApplicationUserComment appComment)
        {
            return await ChangeComment(new ApplicationUserCommentModel {
                FromId = appComment.FromUserId, ToId = appComment.ToUserId, Comment = appComment.Comment, IsInOkRole = true
            });
        }
    }
}

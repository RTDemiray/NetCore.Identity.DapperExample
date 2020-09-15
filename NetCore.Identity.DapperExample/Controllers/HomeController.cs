using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.DapperExample.Models;
using NLog;

namespace NetCore.Identity.DapperExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IDbConnection _dbConnection;

        public HomeController(SignInManager<AppUser> signInManager, IDbConnection dbConnection)
        {
            _signInManager = signInManager;
            _dbConnection = dbConnection;
        }

        [Route("/")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var query = @"SELECT u.Id, u.Username, u.Email, r.Name FROM IdentityUser u
                          INNER JOIN IdentityUserRole ur on ur.UserId = u.Id
                          INNER JOIN IdentityRole r on ur.RoleId = r.Id";

                var lookup = new Dictionary<int, UsersWithRoles>();

                await _dbConnection.QueryAsync<UsersWithRoles, IdentityRoles, UsersWithRoles>(query, (u, r) =>
                {
                    var usersWithRoles = new UsersWithRoles();
                    if (!lookup.TryGetValue(u.Id, out usersWithRoles))
                        lookup.Add(u.Id, usersWithRoles = u);

                    if (usersWithRoles.UserRoles == null)
                        usersWithRoles.UserRoles = new List<IdentityRoles>();

                    usersWithRoles.UserRoles.Add(r);
                    return usersWithRoles;
                }, splitOn: "Name");

                ViewBag.Roles = await _dbConnection.GetAllAsync<IdentityRoles>();
                return View(lookup.Values.ToList());
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return View();
            }
            
        }

        [Route("cikis-yap")]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return RedirectToAction(nameof(Index));
            }
            
        }

        [Route("yazar")]
        [Authorize(Roles = "ADMIN,YAZAR")]
        public IActionResult Yazar()
        {
            return View();
        }

        [Route("uye")]
        [Authorize(Roles = "ADMIN,ÜYE")]
        public IActionResult Uye()
        {
            return View();
        }
        
        [Route("yetkisiz-sayfa")]
        [Authorize]
        public IActionResult UnAuthorized()
        {
            return View();
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.DapperExample.Models;
using NLog;

namespace NetCore.Identity.DapperExample.Controllers
{
    [Route("/giris-yap/{returnUrl?}")]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userControl = await _userManager.FindByNameAsync(model.UserName);
                    if (userControl == null)
                        return Json(new { Message = "Böyle bir kullanıcı bulunamadı!", IsSuccess = false });

                    if (!userControl.IsActive)
                        return Json(new { Message = "Kullanıcı aktif değil!", IsSuccess = false });

                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                        return Json(new { Message = "İşlem başarılı yönlendiriliyorsunuz...", IsSuccess = true });

                    return Json(new { Message = "Kullanıcı adı veya şifre hatalı!", IsSuccess = false });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return View();
            }
        }
    }
}

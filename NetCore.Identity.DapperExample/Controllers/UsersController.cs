using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Identity.Dapper.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.DapperExample.Models;
using NLog;

namespace NetCore.Identity.DapperExample.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IDbConnection _dbConnection;

        public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IPasswordHasher<AppUser> passwordHasher, IDbConnection dbConnection)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _dbConnection = dbConnection;
        }

        [Route("kullanici/ekle")]
        public IActionResult UsersCreate()
        {
            var model = new UsersViewModel();
            model.IdentityRoles = _dbConnection.GetAll<IdentityRoles>().ToList();
            return View(model);
        }

        [Route("kullanici/ekle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UsersCreate(UsersViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userControl = _userManager.FindByEmailAsync(model.Email);
                    if (userControl.Result != null)
                        return Json(new { Message = "Sistemde aynı email'e kayıtlı kullanıcı bulunmaktadır!", IsSuccess = false });

                    var user = new AppUser { UserName = model.UserName, Email = model.Email, IsActive = model.IsActive };
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        for (int i = 0; i < model.RoleName.Count(); i++)
                        {
                            var resultRole = await _userManager.AddToRoleAsync(user, model.RoleName[i]);
                            if (resultRole.Errors.Count() > 0)
                            {
                                return Json(new { Message = "Kullanıcı rolü oluşturulamadı!", IsSuccess = false });
                            }
                        }

                        return Json(new { Message = "Kullanıcı kayıdı oluşturuldu!", IsSuccess = true });

                    }
                    return Json(new { Message = "Kullanıcı kayıdı oluşturulamadı!", IsSuccess = false });
                }

                return Json(new { Message = "Hata oluştu!", IsSuccess = false });
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return Json(new { Message = $"Hata: {ex.Message}", IsSuccess = false });
            }
            
        }

        [Route("kullanici/duzenle/{id}")]
        public async Task<IActionResult> UsersEdit(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();

                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                    return NotFound();

                var model = new UsersViewModel();
                model.Id = user.Id;
                model.IdentityRoles = _dbConnection.GetAllAsync<IdentityRoles>().Result.ToList();

                model.UserRole = new List<DapperIdentityRole>();

                foreach (var item in user.Roles.ToList())
                    model.UserRole.Add(await _roleManager.FindByIdAsync(item.RoleId.ToString()));

                foreach (var item in model.UserRole)
                {
                    var otherRoles = model.IdentityRoles.First(x => x.Name == item.Name);
                    model.IdentityRoles.Remove(otherRoles);
                }

                model.UserName = user.UserName;
                model.IsActive = user.IsActive;
                model.Email = user.Email;
                return View(model);
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return View();
            }
            
        }

        [Route("kullanici/duzenle/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UsersEdit(UsersViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(model.Id.ToString());
                    if (user != null)
                    {
                        user.UserName = model.UserName;
                        user.Email = model.Email;
                        user.IsActive = model.IsActive;
                        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            var oldRoleName = await _userManager.GetRolesAsync(user);
                            if (model.RoleName != null)
                            {
                                if (oldRoleName.Count() > 0)
                                {
                                    for (int i = 0; i < model.RoleName.Count(); i++)
                                        await _userManager.AddToRoleAsync(user, model.RoleName[i]);

                                    foreach (var item in model.RoleName)
                                        oldRoleName.Remove(item);

                                    await _userManager.RemoveFromRolesAsync(user, oldRoleName);
                                }
                                await _userManager.AddToRolesAsync(user, model.RoleName);
                            }
                            await _userManager.RemoveFromRolesAsync(user, oldRoleName);

                            return Json(new { Message = "Kullanıcı kayıdı düzenlendi!", IsSuccess = true });
                        }
                        return Json(new { Message = "Kullanıcı kayıdı düzenlenemedi!", IsSuccess = false });
                    }
                    return NotFound();
                }
                return Json(new { Message = "Hata oluştu!", IsSuccess = false });
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return Json(new { Message = $"Hata: {ex.Message}", IsSuccess = false });
            }
            
        }

        [Route("kullanici/sil/{id}")]
        public async Task<IActionResult> UsersDelete(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    return NotFound();

                await _userManager.DeleteAsync(user);
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("rol/ekle")]
        public IActionResult RolesCreate()
        {
            return View();
        }

        [Route("rol/ekle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RolesCreate(RolesViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var role = new AppRole { Name = model.Name, IsActive = model.IsActive };
                    var result = await _roleManager.CreateAsync(role);

                    if (result.Succeeded)
                        return Json(new { Message = "Rol kayıdı oluşturulmuştur!", IsSuccess = true });

                    return Json(new { Message = "Rol kayıdı oluşturulamadı!", IsSuccess = false });
                }
                return Json(new { Message = "Hata oluştu!", IsSuccess = false });
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return Json(new { Message = $"Hata: {ex.Message}", IsSuccess = false });
            }
        }

        [Route("rol/duzenle/{id}")]
        public async Task<IActionResult> RolesEdit(int id)
        {
            try
            {
                if (id == 0)
                    return BadRequest();

                var role = await _roleManager.FindByIdAsync(id.ToString());
                var model = new RolesViewModel();
                model.Id = role.Id;
                model.Name = role.Name;
                model.IsActive = role.IsActive;
                if (role == null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return View();
            }
            
        }

        [Route("rol/duzenle/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolesEdit(RolesViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var role = await _roleManager.FindByIdAsync(model.Id.ToString());
                    if (role != null)
                    {
                        role.Name = model.Name;
                        role.IsActive = model.IsActive;
                        await _roleManager.UpdateAsync(role);
                        return Json(new { Message = "Rol kayıdı düzenlendi!", IsSuccess = true });
                    }
                    return NotFound();
                }

                return Json(new { Message = "Hata oluştu!", IsSuccess = false });
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return Json(new { Message = $"Hata: {ex.Message}", IsSuccess = false });
            }
            
        }

        [Route("rol/sil/{id}")]
        public async Task<IActionResult> RolesDelete(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                    return NotFound();

                await _roleManager.DeleteAsync(role);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetLogger("FileLogger");
                logger.Log(NLog.LogLevel.Error, $"\nHata Mesajı: {ex.Message}\nStackTrace:{ex.StackTrace}");
                return RedirectToAction("Index", "Home");
            }
            
        }
    }
}

using AliaaCommon.Models;
using EasyMongoNet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TciDataLinks.Models;
using TciDataLinks.ViewModels;

namespace TciDataLinks.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(IDbContext db) : base(db) { }

        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AuthUserX user = db.CheckAuthentication(model.Username, model.Password);
                if (user != null)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.GivenName, user.DisplayName)
                    };

                    StringBuilder permsStr = new StringBuilder();
                    if (user.IsAdmin)
                    {
                        foreach (string p in Enum.GetNames(typeof(Permission)))
                            permsStr.Append(p).Append(",");
                        claims.Add(new Claim("IsAdmin", "true"));
                    }
                    else
                    {
                        foreach (Permission p in user.Permissions)
                            permsStr.Append(p).Append(",");
                    }
                    claims.Add(new Claim(nameof(Permission), permsStr.ToString()));

                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();
                    return RedirectToLocal(model.ReturnUrl);
                }
            }
            ModelState.AddModelError("", "نام کاربری یا رمز عبور صحیح نیست!");
            return View(nameof(Login));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(GraphController.Index), "Graph");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties { IsPersistent = false }).Wait();
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var user = GetUser();
            if (user != null)
            {
                if (AliaaCommon.Models.AuthUserDBExtention.GetHash(model.CurrentPassword) == user.HashedPassword)
                {
                    if (model.NewPassword == model.RepeatNewPassword)
                    {
                        user.Password = model.NewPassword;
                        db.Save(user);
                        ViewBag.Success = true;
                    }
                    else
                        ModelState.AddModelError("RepeatNewPassword", "رمز جدید و تکرار آن باهم برابر نیستند.");
                }
                else
                    ModelState.AddModelError("CurrentPassword", "رمز فعلی اشتباه میباشد.");
            }
            return View(model);
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public IActionResult Index()
        {
            var users = db.All<AuthUserX>();
            return View(users);
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public IActionResult Add()
        {
            return View(new AddUserViewModel());
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public IActionResult Add(AddUserViewModel model)
        {
            var user = Mapper.Map<AuthUserX>(model);
            db.Save(user);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UsernameIsValid(string username, string id)
        {
            var exists = db.Any<AuthUserX>(u => u.Username == username && u.Id != id);
            return Json(!exists);
        }

        [Authorize(nameof(Permission.ManageUsers))]
        public IActionResult Edit(string id)
        {
            var user = db.FindById<AuthUserX>(id);
            var model = Mapper.Map<EditUserViewModel>(user);
            return View(model);
        }

        [Authorize(nameof(Permission.ManageUsers))]
        [HttpPost]
        public IActionResult Edit(EditUserViewModel model)
        {
            var user = db.FindById<AuthUserX>(model.Id);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Permissions = model.Permissions;
            user.Disabled = model.Disabled;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = model.Password;
            db.Save(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
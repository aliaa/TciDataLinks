using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EasyMongoNet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TciDataLinks.Models;

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
    }
}
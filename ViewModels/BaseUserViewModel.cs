using AliaaCommon;
using EasyMongoNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TciDataLinks.Controllers;
using TciDataLinks.Models;

namespace TciDataLinks.ViewModels
{
    public class BaseUserViewModel
    {
        [Required]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Remote(nameof(AccountController.UsernameIsValid), "Account", AdditionalFields = nameof(MongoEntity.Id), ErrorMessage = "نام کاربری قبلا موجود میباشد!")]
        [Required]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Display(Name = "مجوزها")]
        public List<Permission> Permissions
        {
            get
            {
                return PermissionsSelect.Where(p => p.Selected)
              .Select(p => (Permission)Enum.Parse(typeof(Permission), p.Value)).ToList();
            }
            set
            {
                foreach (var perm in value)
                    PermissionsSelect.First(i => i.Value == perm.ToString()).Selected = true;
            }
        }

        [Display(Name = "مجوزها")]
        public List<SelectListItem> PermissionsSelect { get; set; } =
            Enum.GetNames(typeof(Permission))
            .Select(p => new SelectListItem(Utils.DisplayName(typeof(Permission), p), p))
            .ToList();
    }
}

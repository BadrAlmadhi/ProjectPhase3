// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectPhase3.Data;

namespace ProjectPhase3.Areas.Identity.Pages
{
    public class RegisterModel(
        UserManager<IdentityUser> userManager,
        IUserStore<IdentityUser> userStore,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger)
        : PageModel
    {
        //Use this if you're sending registration emails, which we're not
        //private readonly IUserEmailStore<IdentityUser> _emailStore;
        //private readonly IEmailSender _emailSender;

        /*IEmailSender emailSender*/
        //_emailStore = GetEmailStore();
        //_emailSender = emailSender;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            public List<SelectListItem> Roles { get; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Student", Text = "Student" },
                new SelectListItem { Value = "Professor", Text = "Professor" },
                new SelectListItem { Value = "Administrator", Text = "Administrator"  }
            };

            public string Department { get; set; }

            public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>
            {
                new SelectListItem{Value = "None", Text = "NONE"}
            };

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public System.DateTime DOB { get; set; }

            [Required]
            //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

          

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var uid = CreateNewUser(Input.FirstName, Input.LastName, Input.DOB, Input.Department, Input.Role);
                var user = new IdentityUser { Email = uid  + "@utah.edu"};

                await userStore.SetUserNameAsync(user, uid, CancellationToken.None);
                var result = await userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");
                    await userManager.AddToRoleAsync(user, Input.Role);

                    var userId = await userManager.GetUserIdAsync(user);

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a new user account and insert into your database (the one you designed in phase1/2 not the
        /// Authorization DB that dotnet creates for you.  Other code does that for you)
        /// </summary>
        /// <returns>
        /// A new unique uID that is not currently in use by any student, professor, or administrator
        /// </returns>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="DOB"></param>
        /// <param name="departmentAbbrev"></param>
        /// <param name="role"></param>
        string CreateNewUser(string firstName, string lastName, DateTime DOB, string departmentAbbrev, string role)
        {
            var presetInfo = new DbContextOptionsBuilder<LmsContext>();
            presetInfo.UseNpgsql("Host=atr.eng.utah.edu;Username=u1150859;Database=LMS1");
            using (LmsContext lmsCon = new LmsContext(presetInfo.Options))
            {
                var newUserData = new InputModel
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DOB = DOB,
                    Department = departmentAbbrev,
                    Role = role
                };

                lmsCon.Add(newUserData);
                lmsCon.SaveChanges();

                var newUser = lmsCon.Entry(newUserData);

                return newUser.Property("userid").CurrentValue.ToString();
            }
        }

        /*******End code to modify********/
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace Bookify.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            public string Username { get; set; }
        }

        public void OnGet(string Username)
        {
            Input = new InputModel
            {
                Username = Username,
            };

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            var Username = Input.Username.Trim().ToUpper();
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == Username || u.NormalizedUserName == Username);


            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            if (user.IsDeleted)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support for assistance.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);


            var placeholder = new Dictionary<string, string>()
                {
                    { "imageUrl" , "https://res.cloudinary.com/dkbsaseyc/image/upload/fl_preserve_transparency/v1729535424/icon-positive-vote-1_rdexez_ii8um2.jpg?_s=public-apps"} ,
                    { "header" , $"Hey {user.FullName}"} ,
                    { "body" , "Please Confirm your account"} ,
                    { "url" , callbackUrl} ,
                    { "linkTitle" , "Active Account"} ,
                };

            var body = await _emailBodyBuilder.GetEmailBodyAsync(
                            EmailTemplates.Email,
                            placeholder
                        );



            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);


            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}

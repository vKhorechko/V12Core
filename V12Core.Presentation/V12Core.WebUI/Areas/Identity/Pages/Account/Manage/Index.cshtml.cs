using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using V12Core.Application.Interfaces;

namespace V12Core.WebUI.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel<TUser> : PageModel where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        [BindProperty]
        public InputModel Input { get; set; }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public bool IsPhoneNumberConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string Code { get; set; }

        [BindProperty]
        public bool isPhoneVerifyCode { get; set; }

        public IndexModel(
            UserManager<TUser> userManager,
            SignInManager<TUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            IsPhoneNumberConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }

                // In our UI email and user name are one and the same, so when we update the email
                // we need to update the user name.
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Email);
                if (!setUserNameResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting name for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmail()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationPhone()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            try
            {
                await _smsSender.SendSmsAsync(
                    phoneNumber,
                    $"Confirm your phone number. #{Environment.NewLine} Verification code: #{code}");
            }
            catch (Exception ex)
            {
                // logger.Push(ex.Message)
                throw;
            }
            StatusMessage = "Verification code sent. Please check your sms.";
            isPhoneVerifyCode = true;

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyPhoneNumber(string code)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePhoneNumberAsync(user, Input.PhoneNumber, code);
            if (result.Succeeded)
            {
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }
                StatusMessage = "Phone number verified successfully.";
                return RedirectToPage();
            }
            StatusMessage = "Failed to verify phone";

            return RedirectToPage();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
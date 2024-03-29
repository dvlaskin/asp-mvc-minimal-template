using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Domain.Extensions;
using WebApp.Domain.Interfaces;
using WebApp.Domain.Models;
using WebApp.Models;
using WebApp.Models.Dto.AccountManager;

namespace WebApp.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class AccountManagerController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly UrlEncoder _urlEncoder;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

    public AccountManagerController(
      UserManager<AppUser> userManager,
      SignInManager<AppUser> signInManager,
      IEmailSender emailSender,
      ILogger<AccountManagerController> logger,
      UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
        _urlEncoder = urlEncoder;
    }

    [TempData]
    public string StatusMessage { get; set; }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();

        var model = new UserInfoManagerDto
        {
            Username = user.UserName,
            Email = user.Email,
            IsEmailConfirmed = user.EmailConfirmed,
            StatusMessage = StatusMessage
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserInfoManagerDto model)
    {
        model.Username = model.Email;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        var email = user.Email;
        if (model.Email != email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (setEmailResult.Succeeded)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            }
            else
            {
                model.StatusMessage = "Error occurred setting email";

                _logger.LogError($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                foreach (var error in setEmailResult.Errors)
                {
                    _logger.LogError(error.Description);
                }

                return View(model);

                // throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
            }
        }

        StatusMessage = "Your profile has been updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVerificationEmail(UserInfoManagerDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = EmailConfirmationLink(Url, user.Id, code, Request.Scheme);
        var email = user.Email;
        await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await GetCurrentUserAsync();
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToAction(nameof(SetPassword));
        }

        var model = new ChangePasswordDto { StatusMessage = StatusMessage };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            AddErrors(changePasswordResult);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        return RedirectToAction(nameof(ChangePassword));
    }

    [HttpGet]
    public async Task<IActionResult> SetPassword()
    {
        var user = await GetCurrentUserAsync();
        var hasPassword = await _userManager.HasPasswordAsync(user);

        if (hasPassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        var model = new SetPasswordDto { StatusMessage = StatusMessage };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            AddErrors(addPasswordResult);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        StatusMessage = "Your password has been set.";

        return RedirectToAction(nameof(SetPassword));
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLogins()
    {
        var user = await GetCurrentUserAsync();
        var model = new ExternalLoginsDto { CurrentLogins = await _userManager.GetLoginsAsync(user) };
        model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
        model.StatusMessage = StatusMessage;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkLogin(string provider)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = Url.Action(nameof(LinkLoginCallback));
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
        return new ChallengeResult(provider, properties);
    }

    [HttpGet]
    public async Task<IActionResult> LinkLoginCallback()
    {
        var user = await GetCurrentUserAsync();
        var info = await _signInManager.GetExternalLoginInfoAsync(user.Id)
            ?? throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");

        var result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
        }

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        StatusMessage = "The external login was added.";
        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(RemoveLoginDto model)
    {
        var user = await GetCurrentUserAsync();
        var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
        if (!result.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        StatusMessage = "The external login was removed.";
        return RedirectToAction(nameof(ExternalLogins));
    }

    [HttpGet]
    public async Task<IActionResult> TwoFactorAuthentication()
    {
        var user = await GetCurrentUserAsync();
        var model = new TwoFactorAuthenticationDto
        {
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            Is2faEnabled = user.TwoFactorEnabled,
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Disable2faWarning()
    {
        var user = await GetCurrentUserAsync();
        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
        }

        return View(nameof(Disable2fa));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2fa()
    {
        var user = await GetCurrentUserAsync();
        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
        }

        _logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
        return RedirectToAction(nameof(TwoFactorAuthentication));
    }

    [HttpGet]
    public async Task<IActionResult> EnableAuthenticator()
    {
        var user = await GetCurrentUserAsync();
        var model = new EnableAuthenticatorDto();
        await LoadSharedKeyAndQrCodeUriAsync(user, model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorDto model)
    {
        var user = await GetCurrentUserAsync();
        if (!ModelState.IsValid)
        {
            await LoadSharedKeyAndQrCodeUriAsync(user, model);
            return View(model);
        }

        // Strip spaces and hypens
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Code", "Verification code is invalid.");
            await LoadSharedKeyAndQrCodeUriAsync(user, model);
            return View(model);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        _logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        TempData[RecoveryCodesKey] = recoveryCodes.ToArray();

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [HttpGet]
    public IActionResult ShowRecoveryCodes()
    {
        if (TempData[RecoveryCodesKey] is not string[] recoveryCodes)
        {
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        var model = new ShowRecoveryCodesDto { RecoveryCodes = recoveryCodes };
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetAuthenticatorWarning()
    {
        return View(nameof(ResetAuthenticator));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetAuthenticator()
    {
        var user = await GetCurrentUserAsync();

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        _logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

        return RedirectToAction(nameof(EnableAuthenticator));
    }

    [HttpGet]
    public async Task<IActionResult> GenerateRecoveryCodesWarning()
    {
        var user = await GetCurrentUserAsync();
        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' because they do not have 2FA enabled.");
        }

        return View(nameof(GenerateRecoveryCodes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await GetCurrentUserAsync();
        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        _logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

        var model = new ShowRecoveryCodesDto { RecoveryCodes = recoveryCodes.ToArray() };

        return View(nameof(ShowRecoveryCodes), model);
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private void AddErrors(string errorDescription)
    {
        ModelState.AddModelError(string.Empty, errorDescription);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;

        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("webapp"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(AppUser user, EnableAuthenticatorDto model)
    {
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        model.SharedKey = FormatKey(unformattedKey);
        model.AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);
    }

    private string EmailConfirmationLink(IUrlHelper urlHelper, string userId, string code, string scheme)
    {
        return urlHelper.GenerateLinkForAction(
            new ActionLinkDescription()
            {
                Action = nameof(AccountController.ConfirmEmail),
                Controller = nameof(AccountController).GetControllerName(),
                UserId = userId,
                Code = code,
                Scheme = scheme
            }
        );
    }

    private async Task<AppUser> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User)
            ?? throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
    }

    #endregion

}

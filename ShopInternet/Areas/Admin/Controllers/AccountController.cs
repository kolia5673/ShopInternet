using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopInternet.Areas.Admin.Models.VM;

namespace ShopInternet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RegisterUserAccess")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //Goggle Auth
        //Обробка кнопки "Зайти з Google"
        [HttpPost]
        [AllowAnonymous]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { area = "Admin" });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        //Google перенаправить після підтвердження
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                return RedirectToAction("LoginFailure", "Account", new { area = "Admin" });
            }
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor:true);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile", "Account", new { area = "Admin" });
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction("LoginFailure", "Account", new { area = "Admin" });
            }
            else
            {
                var email = info.Principal.FindFirstValue((ClaimTypes.Email));
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToAction("LoginFailure", "Account", new { area = "Admin" });
                }
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return RedirectToAction("LoginFailure", "Account", new { area = "Admin" });
                    }
                }
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, WC.CustomerRole);
                    await _signInManager.SignInAsync(user, isPersistent:false);
                    return RedirectToAction("Profile", "Account", new { area = "Admin" });
                }
                return RedirectToAction("LoginFailure", "Account", new { area = "Admin" });
            }
        }

        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginFailure()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if(User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            return View(User.Claims);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return Challenge();
            var apiKeyClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "ApiKey");
            ViewBag.ApiKey = apiKeyClaim?.Value;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateApiKey()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return Challenge();
            var claims = await _userManager.GetClaimsAsync(user);
            var apiKey = claims.FirstOrDefault(c => c.Type == "ApiKey");
            if (apiKey != null)
            {
                await _userManager.RemoveClaimAsync(user, apiKey);
            }

            var newKey = Guid.NewGuid().ToString();
            await _userManager.AddClaimAsync(user, new Claim("ApiKey", newKey));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            RegisterViewModel regModel = new RegisterViewModel();
            return View(regModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, WC.CustomerRole);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home", new { area =""});
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            LoginViewModel lModel = new LoginViewModel();
            lModel.RememberMe = true;
            return View(lModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                ModelState.AddModelError(string.Empty, "Невірний логін чи пароль");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArsamBackend.Controllers
{
    // Todo : adjust some Status Codes
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IdentityUser>> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.EmailAddress,
                    Email = model.EmailAddress
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var Token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var ConfirmationLink = Url.Action("ConfirmEmail", "Account", new { id = user.Id, token = Token }, Request.Scheme);
                    // TODO : Send email 
                    _logger.Log(LogLevel.Warning, ConfirmationLink);
                    return CreatedAtAction(nameof(Register), new { email = user.Email, token = Token, id = user.Id });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);
        }

        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (id == null)
            {
                return NotFound("User id is invalid!");
            }
            if (token == null)
            {
                return BadRequest("Email confirmation Token is invalid!");
            }
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return StatusCode(307);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt!");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }

    }
}

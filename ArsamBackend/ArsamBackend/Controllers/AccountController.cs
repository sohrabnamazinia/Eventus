using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Security;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<AppUser>> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
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
        [AllowAnonymous]
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
                return Ok(user.Email);
            }
            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound("User not found!");
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("Error", "Email has not been confirmed yet!");
                    return Unauthorized(ModelState);
                }

                var Token = JWTokenHandler.GenerateToken(user);

                if (result.Succeeded)
                {
                    return Ok(new { Token });
                }
                
                ModelState.AddModelError("Error", "Invalid login attempt!");
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

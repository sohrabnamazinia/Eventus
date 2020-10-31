﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.Security;
using ArsamBackend.Utilities;
using ArsamBackend.ViewModels;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> _logger;
        public readonly JwtSecurityTokenHandler handler;
        private readonly IDataProtector protector;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStrings.UserIdQueryString);
        }

        [HttpPost]
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
                    var EncryptedId = protector.Protect(user.Id);
                    var ConfirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { id = EncryptedId, token = Token }, Request.Scheme);
                    // TODO : Send email 
                    _logger.Log(LogLevel.Warning, ConfirmationLink);
                    return CreatedAtAction(nameof(Register), new { email = user.Email, token = Token });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Error", error.Description);
                }
            }
            return BadRequest(ModelState);
        }

        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (id == null)
            {
                return NotFound(Constants.NotFoundError);
            }
            if (token == null)
            {
                return BadRequest("Email confirmation Token is invalid!");
            }
            id = protector.Unprotect(id);
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(Constants.NotFoundError);
            }
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok(user.Email);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(Constants.NotFoundError);
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("Error", Constants.EmailConfirmationError);
                    return Unauthorized(ModelState);
                }

                if (result.IsLockedOut)
                {
                    return StatusCode(423, "Too many Failed attempts! please try later.");
                }


                var Token = JWTokenHandler.GenerateToken(user);

                if (result.Succeeded)
                {
                    return Ok(new { Token });
                }
                
                ModelState.AddModelError("Error", Constants.InvalidLoginError);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<ActionResult> GoogleLogin(string TokenId)
        {
            GoogleJsonWebSignature.Payload Payload;
            try
            {
                Payload = await GoogleJsonWebSignature.ValidateAsync(TokenId);
                
                var Email = Payload.Email;
                var user = await userManager.FindByEmailAsync(Email);

                if (user == null)
                {
                    var NewUser = new AppUser()
                    {
                        Email = Email,
                        UserName = Email
                    };
                    var result = await userManager.CreateAsync(NewUser);

                    if (result.Succeeded)
                    {
                        var Token = await userManager.GenerateEmailConfirmationTokenAsync(NewUser);
                        var EncryptedId = protector.Protect(NewUser.Id);
                        var ConfirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { id = EncryptedId, token = Token }, Request.Scheme);
                        // TODO : Send email 
                        _logger.Log(LogLevel.Warning, ConfirmationLink);
                        return CreatedAtAction(nameof(Register), new { email = Email, token = Token });
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Error", error.Description);
                    }
                    return BadRequest(ModelState);
                }   
                else
                {
                    if (user.EmailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        var Token = JWTokenHandler.GenerateToken(user);
                        return Ok(new { Token });
                    }

                    else
                    {
                        return Unauthorized(new { Error = Constants.EmailConfirmationError });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }


    }
    
}

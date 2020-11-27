using System;
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
using ArsamBackend.Services;
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
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> _logger;
        public readonly JwtSecurityTokenHandler handler;
        private readonly IDataProtector protector;
        private readonly IJWTService _jWTHandler;
        

        public AccountController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IJWTService jWTHandler)
        {
            this._context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStrings.UserIdQueryString);
            this._jWTHandler = jWTHandler;
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


                var Token = _jWTHandler.GenerateToken(user);

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
                        var EmailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(NewUser);
                        var confirmEmailResult = await userManager.ConfirmEmailAsync(NewUser, EmailConfirmationToken);
                        if (confirmEmailResult.Succeeded)
                        {
                            _logger.Log(LogLevel.Warning, NewUser.Email + " : Logged in via google account, thus no email confirmation is required.");
                            await signInManager.SignInAsync(NewUser, isPersistent: false);
                            var jwt = _jWTHandler.GenerateToken(NewUser);
                            return Ok(new { jwt });
                        }
                        foreach (var error in confirmEmailResult.Errors)
                        {
                            ModelState.AddModelError("Error", error.Description);
                        }

                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Error", error.Description);
                    }
                    return BadRequest(ModelState);
                }

                await signInManager.SignInAsync(user, isPersistent: false);
                var Token = _jWTHandler.GenerateToken(user);
                return Ok(new { Token });
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Fields = CategoryService.BitWiseOr(model.Fields);
            user.Description = model.Description;
            _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return Ok(new OutputAppUserViewModel(user));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateImage()
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var files = Request.Form.Files;
            if (files.Count != 1) return BadRequest(Constants.OneImageRequiredError);
            var ImageFile = files[0];
            
            string path = Constants.UserImagesPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!ImageFile.ContentType.ToLower().Contains("image")) return StatusCode(415);

                var ImageFileName = Guid.NewGuid() + "_" + Path.GetFileName(ImageFile.FileName);
                await using (var fileStream = new FileStream(Path.Combine(path, ImageFileName), FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                var UserImage = new UserImage()
                {
                    UserId = user.Id,
                    FileName = ImageFileName,
                    ContentType = ImageFile.ContentType
                };

                var OldImagePath = Constants.UserImagesPath + user.Image.FileName;
                if (System.IO.File.Exists(OldImagePath))
                {
                    System.IO.File.Delete(OldImagePath);
                }
                if (user.Image != null) _context.UsersImage.Remove(user.Image);
                user.Image = UserImage;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new OutputAppUserViewModel(user));
            }

                
            return BadRequest(Constants.ImageNotFound);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(new OutputAppUserViewModel(user));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (user == null) return NotFound("User not found");
            var result = await userManager.ChangePasswordAsync(user, model.OldPass, model.NewPass);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return Conflict(ModelState);
            }
            return Ok(new OutputAppUserViewModel(user));
        }

    }
    
}

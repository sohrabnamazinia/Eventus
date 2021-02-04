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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using StackExchange.Redis;

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
        private readonly ConnectionMultiplexer muxer;
        private readonly IJWTService _jWTHandler;
        private readonly IMinIOService minIOService;
        private readonly IEmailService emailService;
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly IDatabase Redis;

        public AccountController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IJWTService jWTHandler, IMinIOService minIO, IEmailService emailService, IWebHostEnvironment env, IConfiguration config)
        {
            this._context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStrings.UserIdQueryString);
            this._jWTHandler = jWTHandler;
            this.minIOService = minIO;
            this.emailService = emailService;
            this.env = env;
            this.config = config;
            protector = dataProtectionProvider.CreateProtector(DataProtectionPurposeStrings.UserEmailQueryString);
            this.muxer = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
            Redis = this.muxer.GetDatabase();

        }

        [HttpPost]
        public async Task<ActionResult<AppUser>> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.EmailAddress,
                    Email = model.EmailAddress,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var Token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var EncryptedId = protector.Protect(user.Id);
                    var ConfirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { id = EncryptedId, token = Token }, Request.Scheme);
                    emailService.SendEmailConfirmation(new SendEmailConfirmationViewModel 
                    {
                        ConfirmationLink = ConfirmationLink,
                        Email = user.Email,
                        Username = user.UserName,
                        Password = model.Password,
                        FirstName = model.FirstName
                    });
                    _logger.Log(LogLevel.Warning, ConfirmationLink);
                    return CreatedAtAction(nameof(Register), new { email = user.Email, token = Token, confirmationLink = ConfirmationLink });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Error", error.Description);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<ActionResult<string>> ConfirmEmail(string id, string token)
        {
            if (id == null)
            {
                //return NotFound(Constants.NotFoundError);
                return Constants.EmailFailedToConfirmedMessage;
            }
            if (token == null)
            {
                //return BadRequest("Email confirmation Token is invalid!");
                return Constants.EmailFailedToConfirmedMessage;
            }
            id = protector.Unprotect(id);
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Constants.EmailFailedToConfirmedMessage;
                //return NotFound(Constants.NotFoundError);
            }
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Constants.EmailSuccessfullyConfirmedMessage;
                //return Ok(user.Email);
            }
            return Constants.EmailFailedToConfirmedMessage;
            //return BadRequest();
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
                    _jWTHandler.RemoveExpiredTokens(user.Email);
                    return Ok(new { Token , user.Id});
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
            string BearerToken = Request.Headers[HeaderNames.Authorization];
            string token = _jWTHandler.GetRawJTW(BearerToken);
            AppUser user = await _jWTHandler.FindUserByTokenAsync(BearerToken, _context);
            await signInManager.SignOutAsync();
            _jWTHandler.BlockToken(user.Email, token);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);

            user.FirstName = model.FirstName != null ? model.FirstName.Trim().Equals(string.Empty) ? user.FirstName : model.FirstName : user.FirstName;
            user.LastName = model.LastName != null ? model.LastName.Trim().Equals(string.Empty) ? user.LastName : model.LastName : user.LastName;
            user.Fields = model.Fields != null ? CategoryService.BitWiseOr(model.Fields) : user.Fields;
            user.Description = model.Description != null ? model.Description.Trim().Equals(string.Empty) ? null : model.Description : user.Description;
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

            if (ImageFile == null || !(ImageFile.Length > 0)) return BadRequest(Constants.ImageNotFound);

            using (var ms = new MemoryStream())
            {
                ImageFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                if (!Constants.FileFormatChecker(fileBytes) || !Constants.CheckFileNameExtension(Path.GetExtension(ImageFile.FileName))) return StatusCode(415, "File content is not a valid format!");
            }

            var ImageSize = ImageFile.Length;
            if (ImageSize > Constants.MaxImageSizeByte) return BadRequest("File exceeds Maximum size!");

            await minIOService.UpdateUserImage(ImageFile, user);
            return Ok(new OutputAppUserViewModel(user));
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveImage()
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (user.ImageName == null) return Conflict("User already does not have image!");
            await minIOService.RemoveUserImage(user);
            return Ok(new OutputAppUserViewModel(user));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile(string id)
        {
            AppUser requestedUser = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var user = await _context.Users.Include(x => x.InEvents).SingleOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.ImageName != null)
            {
                user.ImageLink = minIOService.GenerateUrl(user.Id, user.ImageName).Result;
                _context.SaveChanges();
            }



            var result = new GetProfileViewModel(user);
            result.IsMe = requestedUser == user;
            //result.EncryptedEmail = protector.Protect(user.Email);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (model.OldPass == model.NewPass) return BadRequest("Old pass word is equal to new password");
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

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChargeAccount(long amount)
        {
            if (amount <= 0) return BadRequest("charge amount must be positive!");
            if (amount > 1000000000) return BadRequest("charge amount must be smaller than 1 billion dollars!");
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            user.Balance += amount;
            _context.SaveChanges();
            return Ok(new OutputAppUserViewModel(user));
        }

        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpgradeToPremium(int package)
        {
            AppUser user = await _jWTHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            if (user == null)
                return StatusCode(401, "token is invalid, user not found");

            (int month, int price) = PremiumService.GetPackage(package);
            if (month == -1 && price == -1)
                return NotFound("package not found");

            if (user.Balance < price)
                return BadRequest("not enough credit, please charge your account");


            user.Balance -= price;

            bool isPremium = !(user.ExpireDateOfPremium == null || user.ExpireDateOfPremium < DateTime.Now);

            if (isPremium)
                user.ExpireDateOfPremium = user.ExpireDateOfPremium.Value.AddMonths(month);
            else
            {
                user.ExpireDateOfPremium = DateTime.Now.AddMonths(month);
                foreach (var ev in user.CreatedEvents)
                    ev.IsBlocked = false;
            }

            await _context.SaveChangesAsync();
            return Ok(new OutputAppUserViewModel(user));
        }

    }

}

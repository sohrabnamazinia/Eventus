using ArsamBackend.Models;
using ArsamBackend.Services;
using ArsamBackend.Utilities;
using ArsamBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AccountController> logger;
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly DataProtectionPurposeStrings dataProtectionPurposeStrings;
        private readonly IJWTService jWTHandler;
        private readonly IMinIOService minIOService;
        private readonly IJWTService jwtHandler;

        public RatingController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings, IJWTService jWTHandler, IMinIOService minIO, IJWTService jwtHandler)
        {
            this._context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.dataProtectionProvider = dataProtectionProvider;
            this.dataProtectionPurposeStrings = dataProtectionPurposeStrings;
            this.jWTHandler = jWTHandler;
            this.minIOService = minIO;
            this.jwtHandler = jwtHandler;
        }

        [HttpPost]
        public async Task<ActionResult<AppUser>> RateEvent(RateEventViewModel model)
        {
            AppUser user = await jwtHandler.FindUserByTokenAsync(Request.Headers[HeaderNames.Authorization], _context);
            var ev = _context.Events.Find(model.Id);
            if (ev == null) return NotFound("Event not found");
            if ((int) model.Stars > 5 || (int) model.Stars < 1) return BadRequest("Stars count must be from 1 to 5");
            Rating rating = new Rating()
            {
                Event = ev,
                Stars = model.Stars,
                User = user
            };
            var canRate = CanRate(ev, user);
            switch (canRate)
            {
                case -3:
                    return BadRequest("project events can not be rated!");
                case -2:
                    return BadRequest("event has not still finished!");
                case -1:
                    return BadRequest("only event members can rate the event!");
            };

            var temp = ev.Ratings.Where(x => x.User == user).FirstOrDefault();
            if (temp != null)
            {
                if (ev.Ratings.Count == 1) ev.AveragedRating = 0;
                else ev.AveragedRating = (((ev.AveragedRating * ev.Ratings.Count) - (int) temp.Stars)) / (ev.Ratings.Count - 1); 
                ev.Ratings.Remove(temp);
            }
            _context.Ratings.Add(rating);
            ev.AveragedRating = (((ev.AveragedRating) * (ev.Ratings.Count - 1) + (double)rating.Stars)) / (ev.Ratings.Count);
            _context.SaveChanges();

            if (ev.Images.Count > 0)
            {
                foreach (var img in ev.Images)
                    img.ImageLink = minIOService.GenerateEventsUrl(img.FileName).Result;
                await _context.SaveChangesAsync();
            }

            return Ok(new OutputEventViewModel(ev));
        }

        [NonAction]
        public int CanRate(Event ev, AppUser user)
        {
            if (ev.IsProject) return -3;
            if (DateTime.Now < ev.EndDate) return -2;
            if (!ev.EventMembers.Contains(user)) return -1;
            return 0;
        }

        [NonAction]
        public double ComputeAverageRating(Event ev)
        {
            double sum = 0;
            foreach (Rating rating in ev.Ratings)
            {
                sum += (double) rating.Stars;
            }
            sum /= ev.Ratings.Count;
            return sum;
        }

    }
}

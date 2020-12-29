using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFilters.ActionFilters
{
    public class NotBlocked : IActionFilter 
    {
        private readonly AppDbContext _context;

        public NotBlocked(AppDbContext context)
        {
            _context = context;
        }
 
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var id = context.ActionArguments.First();
            var ev = _context.Events.Find(id.Value);
            if (ev.IsBlocked)
            {
                context.Result = new BadRequestObjectResult("event is blocked");
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
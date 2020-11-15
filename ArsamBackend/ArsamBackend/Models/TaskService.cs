using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Services;
using ArsamBackend.ViewModels;
using Microsoft.Extensions.Logging;

namespace ArsamBackend.Models
{
    public class TaskService : ITaskService 
    {
        private readonly AppDbContext _context;
        public readonly ILogger<TaskService> _logger;
        public TaskService(AppDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Task> CreateTask(InputTaskViewModel incomeTask, Event taskEvent)
        {
            Task newTask = new Task()
            {
                Name = incomeTask.Name,
                Status = Status.Todo,
                Order = incomeTask.Order,
                Event = taskEvent,
                IsDeleted = false,
                AssignedMembers = new List<AppUser>()
            };
            await _context.Tasks.AddAsync(newTask);
            await _context.SaveChangesAsync();


            return newTask;
        }

    }
}

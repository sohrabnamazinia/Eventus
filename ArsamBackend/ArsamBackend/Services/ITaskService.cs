using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArsamBackend.Models;
using ArsamBackend.ViewModels;
using Task = ArsamBackend;

namespace ArsamBackend.Services
{
    public interface ITaskService
    {
        Task<Models.Task> CreateTask(InputTaskViewModel incomeTask, Event taskEvent);

    }
}

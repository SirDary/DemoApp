using Demo_Auth.Data;
using Demo_Auth.Data.Models;
using Demo_Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Demo.Pages.TaskManager
{
    [Authorize]
    public class IndexModel(TaskService taskService, UserManager<AppUser> userManager, TaskManagerDbContext dbContext) : PageModel
    {
        public IEnumerable<Tasks> AllTasks { get; set; } = [];
        public IEnumerable<Tasks> MeTasks { get; set; } = [];
        public IEnumerable<Tasks> MyTasks { get; set; } = [];
        public IEnumerable<Tasks> OverdueTasks { get; set; } = [];
        public async Task<IActionResult> OnGet()
        {
            var user = dbContext.Users
                .Include(u => u.Employee)
                .Where(u => u.Id == userManager.GetUserId(User))
                .FirstOrDefault();
            
            if(user is not null)
            {
                AllTasks = await taskService.GetAllTasksWithDetailsAsync();
                OverdueTasks = AllTasks.Where(t => t.Deadline < DateTime.Now).ToList();
                MyTasks = AllTasks.Where(t => t.TaskAuthorNavigation.EmployeeId == user.Employee.EmployeeId).ToList();
                MeTasks = AllTasks.Where(t => t.Executors.Contains(user.Employee)).ToList();
            }
            
            return Page();
        }

        public IActionResult OnGetMyClick(int i)
        {
            if(i == 1)
                return RedirectToPage("/TaskManager/Create");
            return Page();
        }

    }
}

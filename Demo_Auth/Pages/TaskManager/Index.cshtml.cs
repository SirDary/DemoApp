using Demo_Auth.Data;
using Demo_Auth.Data.Models;
using Demo_Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Pages.TaskManager
{
    public class IndexModel(TaskService taskService) : PageModel
    {
        public IEnumerable<Tasks> Tasks { get; set; } = [];
        public IEnumerable<Tasks> OverdueTasks { get; set; } = [];
        public async Task<IActionResult> OnGet()
        {
            Tasks = await taskService.GetAllTasksWithDetailsAsync();
            OverdueTasks = Tasks.Where(t => t.Deadline < DateTime.Now).ToList();

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

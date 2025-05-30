using Demo_Auth.Data;
using Demo_Auth.Data.Models;
using Demo_Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Demo_Auth.Pages.TaskManager
{
    
    public class CreateModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TaskService _taskSercice;
        private readonly TaskManagerDbContext _dbContext;

        public CreateModel(UserManager<AppUser> userManager, TaskService taskService, TaskManagerDbContext dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _taskSercice = taskService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public ICollection<SelectListItem> ExecuterList { get; set; } = [];

        public void OnGet()
        {
            ExecuterList = FillSelectLict();
        }

        public async Task<IActionResult> OnPost()
        {
            ExecuterList = FillSelectLict();

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            var newTask = new Tasks
            {
                TaskTitle = Input.TaskTitle,
                Description = Input.Description,
                Deadline = Input.Deadline,
                TaskAuthorNavigation = user.Employee,
                TasksStatusId = 0
            };

            await _taskSercice.CreateTaskAsync(newTask, Input.Executors);

            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Заполняет коллекцию элементов выпадающего списка (SelectListItem) данными пользователей.
        /// </summary>
        /// <returns>
        /// ExecuterList для отображения списка сотрудников в селекте
        /// </returns>
        private ICollection<SelectListItem> FillSelectLict()
        {
            ExecuterList.Clear();
            return _dbContext.Users
                .Include(u => u.Employee).ToList()
                .Select(u => u.Employee.FullName != null
                    ? new SelectListItem(
                        text: u.Employee.FullName,
                        value: u.Employee.EmployeeId.ToString())
                    : new SelectListItem(
                        text: u.Email,
                        value: u.Employee.EmployeeId.ToString())
                ).ToList();
        }

        public class InputModel
        {
            private static readonly DateTime _today = new(
                day: DateTime.Now.Day,
                month: DateTime.Now.Month,
                year: DateTime.Now.Year,
                hour: DateTime.Now.Hour,
                minute: DateTime.Now.Minute,
                second: 0);

            [Required(ErrorMessage = "Название обязательно!")]
            [Display(Name = "Название")]
            public string TaskTitle { get; set; } = null!;

            [Display(Name = "Описание")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Крайний срок обязателен!")]
            [Display(Name = "Крайний срок")]
            public DateTime Deadline { get; set; } = _today;

            [Required(ErrorMessage = "Выберите кому назначить задачу")]
            [Display(Name = "Кому")]
            public ICollection<int> Executors { get; set; } = null!;
        }
    }
}

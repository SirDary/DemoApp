using Demo_Auth.Data;
using Demo_Auth.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo_Auth.Services
{
    public class TaskService(TaskManagerDbContext dbContext, ILogger<TaskService> logger)
    {
        private readonly TaskManagerDbContext _dbContext = dbContext;
        private readonly ILogger<TaskService> _logger = logger;

        /// <summary>
        /// Создаёт новую задачу в системе
        /// </summary>
        /// <param name="task">Объект задачи для создания</param>
        /// <param name="executorIds">Список ID исполнителей</param>
        /// <returns>Созданная задача</returns>
        public async Task<Tasks> CreateTaskAsync(Tasks task, IEnumerable<int> executorIds)
        {
            try
            {
                _logger.LogInformation("Creating new task with title: {TaskTitle}", task.TaskTitle);

                task.DateOfCreate = DateTime.Now;
                _dbContext.Tasks.Add(task);
                await _dbContext.SaveChangesAsync();

                // Добавляем связь с исполнителями через таблицу task_executor
                if (executorIds != null && executorIds.Any())
                {
                    _logger.LogDebug("Adding {ExecutorCount} executors to task {TaskId}",
                        executorIds.Count(), task.TaskId);

                    foreach (var executorId in executorIds)
                    {
                        var employee = await _dbContext.Employees.FindAsync(executorId);
                        if (employee != null)
                        {
                            task.Executors.Add(employee);
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }

                _logger.LogInformation("Successfully created task ID: {TaskId}", task.TaskId);
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw new ApplicationException("Failed to create task", ex);
            }
        }

        /// <summary>
        /// Получает задачу со всей связанной информацией
        /// </summary>
        /// <param name="taskId">ID задачи</param>
        /// <returns>Задача с автором, статусом, комментариями и исполнителями</returns>
        public async Task<Tasks?> GetTaskWithDetailsAsync(int taskId)
        {
            try
            {
                _logger.LogDebug("Fetching task details for ID: {TaskId}", taskId);
                
                return await _dbContext.Tasks
                    .AsNoTracking()
                    .Where(t => t.TaskId == taskId)
                    .Select(t => new Tasks
                    {
                        TaskId = t.TaskId,
                        TaskTitle = t.TaskTitle,
                        Description = t.Description,
                        DateOfCreate = t.DateOfCreate,
                        Deadline = t.Deadline,
                        TaskAuthorNavigation = t.TaskAuthorNavigation != null ? new Employee
                        {
                            EmployeeId = t.TaskAuthorNavigation.EmployeeId,
                            FullName = t.TaskAuthorNavigation.FullName
                        } : null,
                        TasksStatus = t.TasksStatus != null ? new TasksStatus
                        {
                            TasksStatusId = t.TasksStatus.TasksStatusId,
                            TasksStatusName = t.TasksStatus.TasksStatusName
                        } : null,
                        TaskComments = t.TaskComments.Select(c => new TaskComment
                        {
                            CommentId = c.CommentId,
                            CommentContent = c.CommentContent,
                            Employee = c.Employee != null ? new Employee
                            {
                                EmployeeId = c.Employee.EmployeeId,
                                FullName = c.Employee.FullName
                            } : null
                        }).ToList(),
                        Executors = t.Executors.Select(e => new Employee
                        {
                            EmployeeId = e.EmployeeId,
                            FullName = e.FullName
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task with ID: {TaskId}", taskId);
                throw new ApplicationException($"Failed to get task with ID {taskId}", ex);
            }
        }

        /// <summary>
        /// Получает все задачи с базовой информацией
        /// </summary>
        /// <returns>Список всех задач с авторами, статусами и исполнителями</returns>
        public async Task<IEnumerable<Tasks>> GetAllTasksWithDetailsAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all tasks with details (optimized)");

                return await _dbContext.Tasks
                    .AsNoTracking()
                    .Select(t => new Tasks
                    {
                        TaskId = t.TaskId,
                        TaskTitle = t.TaskTitle,
                        Description = t.Description,
                        DateOfCreate = t.DateOfCreate,
                        Deadline = t.Deadline,
                        TaskAuthorNavigation = t.TaskAuthorNavigation != null ? new Employee
                        {
                            EmployeeId = t.TaskAuthorNavigation.EmployeeId,
                            FullName = t.TaskAuthorNavigation.FullName
                        } : null,
                        TasksStatus = t.TasksStatus != null ? new TasksStatus
                        {
                            TasksStatusId = t.TasksStatus.TasksStatusId,
                            TasksStatusName = t.TasksStatus.TasksStatusName
                        } : null,
                        Executors = t.Executors.Select(e => new Employee
                        {
                            EmployeeId = e.EmployeeId,
                            FullName = e.FullName
                        }).ToList()
                    })
                    .OrderByDescending(t => t.DateOfCreate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all tasks");
                throw new ApplicationException("Failed to get tasks", ex);
            }
        }

        /// <summary>
        /// Обновляет существующую задачу и её исполнителей
        /// </summary>
        /// <param name="task">Обновлённые данные задачи</param>
        /// <param name="executorIds">Новый список ID исполнителей (опционально)</param>
        /// <exception cref="KeyNotFoundException">Если задача не найдена</exception>
        public async Task UpdateTaskAsync(Tasks task, IEnumerable<int>? executorIds = null)
        {
            try
            {
                _logger.LogInformation("Updating task ID: {TaskId}", task.TaskId);

                // Получаем задачу из БД вместе с текущими исполнителями
                var existingTask = await _dbContext.Tasks
                    .Include(t => t.Executors)
                    .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

                if (existingTask == null)
                {
                    _logger.LogWarning("Task not found for update: {TaskId}", task.TaskId);
                    throw new KeyNotFoundException($"Task with ID {task.TaskId} not found");
                }

                // Копируем обновлённые значения (кроме исполнителей)
                _dbContext.Entry(existingTask).CurrentValues.SetValues(task);

                // Обновляем исполнителей, если передан новый список
                if (executorIds != null)
                {
                    _logger.LogDebug("Updating {ExecutorCount} executors for task {TaskId}",
                        executorIds.Count(), task.TaskId);

                    existingTask.Executors.Clear(); // Удаляем старых исполнителей

                    foreach (var executorId in executorIds)
                    {
                        var employee = await _dbContext.Employees.FindAsync(executorId);
                        if (employee != null)
                        {
                            existingTask.Executors.Add(employee);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully updated task ID: {TaskId}", task.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task ID: {TaskId}", task.TaskId);
                throw new ApplicationException($"Failed to update task with ID {task.TaskId}", ex);
            }
        }

        /// <summary>
        /// Удаляет задачу по ID
        /// </summary>
        /// <param name="taskId">ID задачи для удаления</param>
        /// <exception cref="KeyNotFoundException">Если задача не найдена</exception>
        public async Task DeleteTaskAsync(int taskId)
        {
            try
            {
                _logger.LogInformation("Deleting task ID: {TaskId}", taskId);

                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task not found for deletion: {TaskId}", taskId);
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                _dbContext.Tasks.Remove(task);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted task ID: {TaskId}", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task ID: {TaskId}", taskId);
                throw new ApplicationException($"Failed to delete task with ID {taskId}", ex);
            }
        }

        /// <summary>
        /// Добавляет комментарий к задаче
        /// </summary>
        /// <param name="taskId">ID задачи</param>
        /// <param name="employeeId">ID сотрудника (автора комментария)</param>
        /// <param name="commentContent">Текст комментария</param>
        /// <returns>Созданный комментарий</returns>
        /// <exception cref="ArgumentException">Если текст комментария пуст</exception>
        public async Task<TaskComment> AddCommentToTaskAsync(int taskId, int employeeId, string commentContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commentContent))
                {
                    _logger.LogWarning("Empty comment content for task ID: {TaskId}", taskId);
                    throw new ArgumentException("Comment content cannot be empty", nameof(commentContent));
                }

                _logger.LogDebug("Adding comment to task ID: {TaskId} by employee ID: {EmployeeId}",
                    taskId, employeeId);

                var comment = new TaskComment
                {
                    TaskId = taskId,
                    EmployeeId = employeeId,
                    CommentContent = commentContent.Trim()
                };

                _dbContext.TaskComments.Add(comment);
                await _dbContext.SaveChangesAsync();

                // Загружаем связанные данные для возврата полного объекта
                await _dbContext.Entry(comment)
                    .Reference(c => c.Employee)
                    .LoadAsync();

                _logger.LogInformation("Comment added with ID: {CommentId}", comment.CommentId);
                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to task ID: {TaskId}", taskId);
                throw new ApplicationException($"Failed to add comment to task with ID {taskId}", ex);
            }
        }

        /// <summary>
        /// Получает все комментарии задачи
        /// </summary>
        /// <param name="taskId">ID задачи</param>
        /// <returns>Список комментариев с информацией об авторах</returns>
        public async Task<IEnumerable<TaskComment>> GetTaskCommentsAsync(int taskId)
        {
            try
            {
                _logger.LogDebug("Fetching comments for task ID: {TaskId} (optimized)", taskId);

                return await _dbContext.TaskComments
                    .AsNoTracking()
                    .Where(c => c.TaskId == taskId)
                    .Select(c => new TaskComment
                    {
                        CommentId = c.CommentId,
                        CommentContent = c.CommentContent,
                        Employee = c.Employee != null ? new Employee
                        {
                            EmployeeId = c.Employee.EmployeeId,
                            FullName = c.Employee.FullName
                        } : null
                    })
                    .OrderBy(c => c.CommentId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comments for task ID: {TaskId}", taskId);
                throw new ApplicationException($"Failed to get comments for task with ID {taskId}", ex);
            }
        }

        /// <summary>
        /// Обновляет статус задачи
        /// </summary>
        /// <param name="taskId">ID задачи</param>
        /// <param name="statusId">ID нового статуса</param>
        /// <returns>Обновлённая задача</returns>
        /// <exception cref="KeyNotFoundException">Если задача или статус не найдены</exception>
        public async Task<Tasks> UpdateTaskStatusAsync(int taskId, int statusId)
        {
            try
            {
                _logger.LogInformation("Updating status for task ID: {TaskId} to status ID: {StatusId}",
                    taskId, statusId);

                var task = await _dbContext.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    _logger.LogWarning("Task not found for status update: {TaskId}", taskId);
                    throw new KeyNotFoundException($"Task with ID {taskId} not found");
                }

                var statusExists = await _dbContext.TasksStatuses.AnyAsync(s => s.TasksStatusId == statusId);
                if (!statusExists)
                {
                    _logger.LogWarning("Status not found: {StatusId}", statusId);
                    throw new KeyNotFoundException($"Status with ID {statusId} not found");
                }

                task.TasksStatusId = statusId;
                await _dbContext.SaveChangesAsync();

                // Возвращаем обновлённую задачу с загруженными связанными данными
                var updatedTask = await _dbContext.Tasks
                    .Include(t => t.TasksStatus)
                    .FirstAsync(t => t.TaskId == taskId);

                _logger.LogInformation("Successfully updated status for task ID: {TaskId}", taskId);
                return updatedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for task ID: {TaskId}", taskId);
                throw new ApplicationException($"Failed to update status for task with ID {taskId}", ex);
            }
        }

        /// <summary>
        /// Получает все возможные статусы задач
        /// </summary>
        public async Task<IEnumerable<TasksStatus>> GetAllStatusesAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all task statuses");
                return await _dbContext.TasksStatuses
                    .OrderBy(s => s.TasksStatusId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task statuses");
                throw new ApplicationException("Failed to get task statuses", ex);
            }
        }
    }
}

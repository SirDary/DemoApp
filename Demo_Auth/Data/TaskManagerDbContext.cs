using System;
using System.Collections.Generic;
using Demo_Auth.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo_Auth.Data;

public partial class TaskManagerDbContext : IdentityDbContext<AppUser>
{
    public TaskManagerDbContext()
    {
    }

    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Tasks> Tasks { get; set; }

    public virtual DbSet<TaskComment> TaskComments { get; set; }

    public virtual DbSet<TasksStatus> TasksStatuses { get; set; }

    public virtual DbSet<UserMessage> UserMessages { get; set; }

    public virtual DbSet<UserPost> UserPosts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Task_Manager_DB;Username=postgres;password=S2005@**");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("full_name");
            entity.Property(e => e.NumberPhone)
                .HasMaxLength(11)
                .HasColumnName("number_phone");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Post).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("employees_post_id_fkey");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.AppUserId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tasks>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("tasks_pkey");

            entity.ToTable("tasks");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.DateOfCreate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_of_create");
            entity.Property(e => e.Deadline)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.TaskAuthor).HasColumnName("task_author");
            entity.Property(e => e.TaskTitle)
                .HasMaxLength(50)
                .HasColumnName("task_title");
            entity.Property(e => e.TasksStatusId).HasColumnName("tasks_status_id");

            entity.HasOne(d => d.TaskAuthorNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TaskAuthor)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tasks_task_author_fkey");

            entity.HasOne(d => d.TasksStatus).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TasksStatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tasks_tasks_status_id_fkey");

            entity.HasMany(d => d.Executors).WithMany(p => p.TasksNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskExecutor",
                    r => r.HasOne<Employee>().WithMany()
                        .HasForeignKey("ExecutorId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("task_executor_executor_id_fkey"),
                    l => l.HasOne<Tasks>().WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("task_executor_task_id_fkey"),
                    j =>
                    {
                        j.HasKey("TaskId", "ExecutorId").HasName("task_executor_pkey");
                        j.ToTable("task_executor");
                        j.IndexerProperty<int>("TaskId").HasColumnName("task_id");
                        j.IndexerProperty<int>("ExecutorId").HasColumnName("executor_id");
                    });
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("task_comments_pkey");

            entity.ToTable("task_comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CommentContent).HasColumnName("comment_content");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Employee).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("task_comments_employee_id_fkey");

            entity.HasOne(d => d.Tasks).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("task_comments_task_id_fkey");
        });

        modelBuilder.Entity<TasksStatus>(entity =>
        {
            entity.HasKey(e => e.TasksStatusId).HasName("tasks_status_pkey");

            entity.ToTable("tasks_status");

            entity.Property(e => e.TasksStatusId).HasColumnName("tasks_status_id");
            entity.Property(e => e.TasksStatusName)
                .HasMaxLength(50)
                .HasColumnName("tasks_status_name");
        });

        modelBuilder.Entity<UserMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("user_message_pkey");

            entity.ToTable("user_message");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.DateOfCreate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_of_create");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Image)
                .HasMaxLength(50)
                .HasColumnName("image");
            entity.Property(e => e.MessageContext).HasColumnName("message_context");
            entity.Property(e => e.ReplyToMessage).HasColumnName("reply_to_message");
            entity.Property(e => e.Video)
                .HasMaxLength(50)
                .HasColumnName("video");

            entity.HasOne(d => d.Employee).WithMany(p => p.UserMessages)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_message_employee_id_fkey");

            entity.HasOne(d => d.ReplyToMessageNavigation).WithMany(p => p.InverseReplyToMessageNavigation)
                .HasForeignKey(d => d.ReplyToMessage)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_message_reply_to_message_fkey");
        });

        modelBuilder.Entity<UserPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("user_posts_pkey");

            entity.ToTable("user_posts");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.PostName)
                .HasMaxLength(50)
                .HasColumnName("post_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

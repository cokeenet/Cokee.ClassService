using Cokee.ClassService.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cokee.ClassService.Server;

public class GlobalDbContext : DbContext
{
    public GlobalDbContext(DbContextOptions<GlobalDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 配置 User 实体
        modelBuilder.Entity<User>()
            .ToTable("Users")
            .HasKey(u => u.UserID);

        modelBuilder.Entity<Student>()
            .ToTable("Students")
            .HasKey(s => s.ID);

        // 配置 Class 实体
        modelBuilder.Entity<Class>()
            .ToTable("Classes")
            .HasKey(c => c.ID);

        // 配置外键关系
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Class)   // 配置 Student 实体的 Class 导航属性
            .WithMany(s => s.Students)
            .HasForeignKey(s => s.EnrollClassID)
            .OnDelete(DeleteBehavior.Cascade); // 可选的级联删除操作

        // 配置 Class 和 User 之间的关联
        modelBuilder.Entity<Class>()
            .HasOne(c => c.Owner)
            .WithMany(u => u.OwnedClasses);
    }
}
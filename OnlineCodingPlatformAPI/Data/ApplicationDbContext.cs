using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Models;

namespace OnlineCodingAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Problem> Problems => Set<Problem>();
        public DbSet<TestCase> TestCases => Set<TestCase>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<Progress> ProgressRecords => Set<Progress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- User -----
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // ----- Problem -> TestCase (1-to-many, cascade delete) -----
            modelBuilder.Entity<TestCase>(entity =>
            {
                entity.HasOne(tc => tc.Problem)
                      .WithMany(p => p.TestCases)
                      .HasForeignKey(tc => tc.ProblemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- Submission -----
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasOne(s => s.User)
                      .WithMany(u => u.Submissions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Problem)
                      .WithMany(p => p.Submissions)
                      .HasForeignKey(s => s.ProblemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => new { s.UserId, s.ProblemId });
            });

            // ----- Progress -----
            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithMany(u => u.ProgressRecords)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Problem)
                      .WithMany(pr => pr.ProgressRecords)
                      .HasForeignKey(p => p.ProblemId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One progress record per user/problem pair
                entity.HasIndex(p => new { p.UserId, p.ProblemId }).IsUnique();
            });
        }
    }
}

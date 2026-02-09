using AIResumeAnalyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace AIResumeAnalyzer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Resume> Resumes { get; set; }
    }
}

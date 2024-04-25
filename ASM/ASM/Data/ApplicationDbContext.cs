using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ASM.Models; // Import JobListing model

namespace ASM.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<JobListing> JobListings { get; set; } // Define JobListings as a DbSet of JobListing

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ASM.Models.Employer> Employers { get; set; } = default!;

        public DbSet<ASM.Models.JobSeeker> JobSeekers { get; set; } = default!;

        public DbSet<ASM.Models.Application> Applications { get; set; } = default!;
    }
}

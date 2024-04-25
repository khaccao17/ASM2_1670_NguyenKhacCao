using Microsoft.AspNetCore.Identity;

namespace ASM.Models
{
    public class Employer
    {
        public int EmployerId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }

        public string UserId { get; set; }
        public IdentityUser? User { get; set; }

        public ICollection<JobListing>? JobListing { get; set; }

    }
}

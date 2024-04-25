// JobListing.cs
using System;
using System.Collections.Generic;

namespace ASM.Models
{
    public class JobListing
    {
        public int JobListingId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }
        public string? Status { get; set; }
        public DateTime Deadline { get; set; }

        public int EmployerId { get; set; }
        public Employer? Employer { get; set; }

        public ICollection<Application>? Application { get; set; }
    }
}

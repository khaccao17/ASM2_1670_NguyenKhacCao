namespace ASM.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public string? CoverLetter { get; set; }
        public string? Description { get; set; }

        public string? StatusApp { get; set; }
        public DateTime ApplicationDate { get; set; }

        public int JobListingId { get; set; }
        public JobListing? JobListing { get; set; }

        public int JobSeekerId { get; set; }
        public JobSeeker? JobSeeker { get; set; }
    }
}

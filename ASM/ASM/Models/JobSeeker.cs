using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM.Models
{
    public class JobSeeker
    {
        public int JobSeekerId { get; set; }
        public string Fullname { get; set; }
        //public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Experience { get; set; }
        public string Skill { get; set; }
        public string? UrlImage { get; set; }

        public string UserId { get; set; }
        public IdentityUser? User { get; set; }

        public ICollection<Application>? Application { get; set; }

        [NotMapped]
        public IFormFile? PictureImage { get; set; }
    }
}

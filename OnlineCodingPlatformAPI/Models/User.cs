using OnlineCodingAPI.Enums;
using OnlineCodingPlatformAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCodingAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Progress> ProgressRecords { get; set; } = new List<Progress>();
    }
}

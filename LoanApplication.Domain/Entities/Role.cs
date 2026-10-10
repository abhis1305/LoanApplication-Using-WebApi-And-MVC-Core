using System.ComponentModel.DataAnnotations;

namespace LoanApplication.Domain.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.PersistenceModel.Entities;

[Table("UserRole")]
internal class UserRolePM
{
    [Key]
    [Column("UserRoleId")]
    public Guid Id { get; set; }

    [Column("UserId")]
    [Required]
    public Guid UserId { get; set; }

    [Column("RoleId")]
    [Required]
    public string Role { get; set; } = string.Empty;

    [Column("IsActive")]
    [Required]
    public bool IsActive { get; set; } = true;

    [Column("CreationDate")]
    [Required]
    public DateTime CreationDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserPM User { get; set; } = null!;
}

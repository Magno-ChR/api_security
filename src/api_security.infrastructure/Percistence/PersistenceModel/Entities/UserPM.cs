using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.PersistenceModel.Entities;

[Table("User")]
internal class UserPM
{
    [Key]
    [Column("UserId")]
    public Guid Id { get; set; }

    [Column("Username")]
    [StringLength(100)]
    [Required]
    public string Username { get; set; } = string.Empty;

    [Column("FailedLoginAttempts")]
    [Required]
    public int failedLoginAttempts { get; set; }

    [Column("IsActive")]
    [Required]
    public bool IsActive { get; set; } = true;

    [Column("CreationDate")]
    [Required]
    public DateTime CreationDate { get; set; }

    [Column("UpdateDate")]
    public DateTime? UpdateDate { get; set; }

    [Column("LastLoginDate")]
    public DateTime? LastLoginDate { get; set; }

    [Column("BlockedUntilDate")]
    public DateTime? BlockedUntilDate { get; set; }

    [InverseProperty(nameof(CredentialPM.User))]
    public List<CredentialPM> Credentials { get; set; } = new();

    [InverseProperty(nameof(UserRolePM.User))]
    public List<UserRolePM> UserRoles { get; set; } = new();


}

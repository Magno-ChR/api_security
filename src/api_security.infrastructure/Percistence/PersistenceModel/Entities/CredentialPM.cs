using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Credential")]
internal class CredentialPM
{
    [Key]
    [Column("CredentialId")]
    public Guid Id { get; set; }

    [Column("UserId")]
    [Required]
    public Guid UserId { get; set; }

    [Column("PasswordHash")]
    [Required]
    public string Password { get; set; } = string.Empty;

    [Column("PasswordSalt")]
    [Required]
    public string PasswordSalt { get; set; } = string.Empty;

    [Column("IsActive")]
    [Required]
    public bool IsActive { get; set; }

    [Column("CreationDate")]
    [Required]
    public DateTime CreationDate { get; set; }

    [Column("ExpirationDate")]
    [Required]
    public DateTime ExpirationDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserPM User { get; set; } = null!;

}

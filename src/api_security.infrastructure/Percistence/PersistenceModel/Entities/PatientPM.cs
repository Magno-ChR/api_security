using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace api_security.infrastructure.Percistence.PersistenceModel.Entities;

[Table("Patient")]
internal class PatientPM
{
    [Key]
    [Column("PatientId")]
    [Required]
    public Guid Id { get; set; }

    [Column("FirstName")]
    [StringLength(100)]
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Column("MiddleName")]
    [StringLength(100)]
    [Required]
    public string MiddleName { get; set; } = string.Empty;

    [Column("LastName")]
    [StringLength(100)]
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Column("DocumentNumber")]
    [StringLength(50)]
    [Required]
    public string DocumentNumber { get; set; } = string.Empty;
}

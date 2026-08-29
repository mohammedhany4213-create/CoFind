using System.ComponentModel.DataAnnotations ;

namespace coFind.Domain.Entities;

public class User
{
    public int UserId {get; set;} 

    [Required]
    [MaxLength(50)]
    public string Name {get; set;} = string.Empty ;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email {get; set;} = string.Empty ;

    [Required]
    public string PasswordHash {get; set;} = string.Empty ;

    [Required]
    [Phone]
    [MaxLength(15)]
    public string WhatsappNumber {get; set;} = string.Empty ;

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow ;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow ;

    public ICollection<Offer> Offers { get; set; } = new List<Offer>();


}
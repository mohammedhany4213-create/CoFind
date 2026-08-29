using System.ComponentModel.DataAnnotations ;

namespace coFind.Domain.Entities;

    public class Offer
    {

        public int OfferId {get; set;} 
        public int UserId {get; set;} 

        public User Owner {get; set;} = null! ;
        
        [Required]
        [MaxLength(50)]
        public string Title {get; set;} = string.Empty ;

        [Required]
        [MaxLength(100)]
        public string Description {get; set;} = string.Empty ;

        [Required]
        [MaxLength(50)]
        public string Role {get; set;} = string.Empty ;

        [Required]
        public ICollection<string> Skills {get; set;} = new List<string>() ;

        [Required]
        [MaxLength(50)]
        public string Industry {get; set;} = string.Empty ;

        [Required]
        public bool IsAvilable {get; set;} 

        [MaxLength(100)]
        public string Location {get; set;} = string.Empty ;

        public bool IsActive {get; set;}

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow ;
        public DateTime UpdatedAt {get; set;} = DateTime.UtcNow ;
        

    }



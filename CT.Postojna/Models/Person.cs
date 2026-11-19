using System.ComponentModel.DataAnnotations;

namespace CT.Postojna.Models
{
    public class Person
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public bool Discontinued { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int Age { get; set; }
        public string Citizenship { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        [Required]
        public DateTime? Released { get; set; }
    }
}

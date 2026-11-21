using System.ComponentModel.DataAnnotations;

namespace CT.Postojna.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Duration { get; set; } = string.Empty;
        public string AheadOfTimeGOAT { get; set; } = string.Empty;
        public IEnumerable<Person> values { get; set; } = [];
    }
}

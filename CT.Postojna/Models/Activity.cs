namespace CT.Postojna.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public string AheadOfTimeGOAT { get; set; }
        public IEnumerable<Person> values { get; set; }


    }

}

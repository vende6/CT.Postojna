namespace CT.Postojna.Models
{
    public class Activity
    {
        public string Name { get; set; }

        public string Duration { get; set; }

        public string AheadOfTimeGOAT { get; set; }

        public IEnumerable<Person> values { get; set; }
    }

}

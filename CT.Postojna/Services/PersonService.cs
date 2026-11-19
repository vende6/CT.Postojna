using CT.Postojna.Models;

namespace CT.Postojna.Services
{
    public class PersonService
    {
        public static IEnumerable<Person> GeneratePersons() => new List<Person>
        {
            new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Jones",
                Bio = "A software developer from New York.",
                Discontinued = false,
                Nickname = "Johnny",
                Height = 180.5m,
                Weight = 75.0m,
                Age = 30,
                Citizenship = "Russia",
                Nationality = "German",
                Released = DateTime.Now.AddYears(-5)
            },
            new Person
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Bio = "An artist based in Paris.",
                Discontinued = false,
                Nickname = "Janie",
                Height = 165.0m,
                Weight = 60.0m,
                Age = 28,
                Citizenship = "Russia",
                Nationality = "French",
                Released = DateTime.Now.AddYears(-3)
            }
        };
    }
}

using CT.Postojna.Models;

namespace CT.Postojna.Services
{
    public class ActivityService
    {
        public static IEnumerable<Activity> GenerateActivities() => Enumerable.Range(1, 100).Select(i => new Activity
        {
            Id = i,
            CategoryId= i % 5 == 0 ? (int?)null : i % 5,
            CategoryName = i % 5 == 0 ? string.Empty : $"Category {i % 5}",
            Name = $"Activity {i}",
            Description = $"Description for activity {i}",
            Duration = Random.Shared.Next(30, 120).ToString() + " minutes",
        });
    }
}

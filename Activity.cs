namespace DigitalWellbeing
{
    public class Activity
    {
        public DateTime Date = DateTime.Now.Date;

        public string Name { get; set; }

        public TimeSpan Duration { get; set; }
    }
}

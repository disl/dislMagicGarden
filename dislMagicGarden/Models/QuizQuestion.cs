namespace dislMagicGarden.Models
{
    public class QuizQuestion
    {
        public string question { get; set; }
        public List<string> options { get; set; }
        public int correctIndex { get; set; }
    }
}

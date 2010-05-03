namespace SandRibbonInterop
{
    public class QuizDetails  
    {
        public int returnSlide { get; set; }
        public int targetSlide { get; set; }
        public int optionCount { get; set; }
        public string target { get; set; }
        public string author { get; set; }
        public string quizPath { get; set; }
    }
    public class QuizInfo
    {
        public int quizParent { get; set; }
        public string answerer { get; set; }
        public int targetQuiz { get; set; }
    }
    public class QuizAnswer : QuizInfo
    {
        public int targetQuiz { get; set; }
        public string answer { get; set; }
        public string answerURL { get; set; }
    }
    public class QuizStatusDetails : QuizInfo
    {
        public string status { get; set; }
    }
}

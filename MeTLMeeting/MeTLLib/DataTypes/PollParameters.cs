using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;

namespace MeTLLib.DataTypes
{
    public class Option 
    {
        public Option(String Name, String OptionText, bool IsCorrect, Color Color)
            : base()
        {
            name = Name;
            optionText = OptionText;
            correct = IsCorrect;
            color = Color;
        }
        public String optionText { get; set; }
        public String name { get; set; }
        public bool correct { get; set; }
        public Color color { get; set; }
    }
    public class QuizQuestion
    {
        public QuizQuestion(long Id, long created, string Title, string Author, string Question, List<Option> Options)
            : this(Id, Title, Author, Question, Options)
        {
            this.created = created;
        }
        public QuizQuestion(long Id, string Title, string Author, string Question, List<Option> Options)
        {
            id = Id;
            title = Title;
            author = Author;
            question = Question;
            options = Options;
        }
        public QuizQuestion(long Id, string Title, string Author, string Question, List<Option> Options, string Url)
            : this(Id, Title, Author, Question, Options)
        {
            url = Url;
        }
        public long created;
        public string title { get; set; }
        public string url { get; set; }
        public string question { get; set; }
        public string author { get; set; }
        public List<Option> options { get; set; }
        public long id { get; set; }
        /*public QuizQuestion(){
            options = new List<Option>();
        }*/
    }
    public class QuizAnswer
    {
        public QuizAnswer(long Id, string Respondent, string Response)
        {
            id = Id;
            answerer = Respondent;
            answer = Response;
        }
        public string answerer { get; set; }
        public string answer { get; set; }
        public long id { get; set; }
    }
}

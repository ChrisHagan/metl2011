using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SandRibbonInterop
{
   public class Option
    {
       public String name;
       public string optionText;
       public bool correct;
       public Color color;
    }
    public class QuizQuestion
    {
        public string title { get; set;}
        public string question { get; set;}
        public string author { get; set; }
        public List<Option> options = new List<Option>();
        public long id;
    }
    public class QuizAnswer
    {
        public string answerer { get; set; }
        public string answer { get; set; }
        public long id { get; set; }

    }
}

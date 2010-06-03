using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SandRibbonInterop
{
   public class Option
    {
       public String name{get;set;}
       public string optionText { get; set; }
       public bool correct { get; set; }
       public Color color { get; set; }
    }
    public class QuizQuestion
    {
        public string title { get; set;}
        public string question { get; set;}
        public string author { get; set; }
        public List<Option> options { get; set; }
        public long id { get; set; }
        public QuizQuestion(){
            options = new List<Option>();
        }
    }
    public class QuizAnswer
    {
        public string answerer { get; set; }
        public string answer { get; set; }
        public long id { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using MeTLLib.Providers;

namespace MeTLLib.DataTypes
{
    public class Option : INotifyPropertyChanged
    {
        // change to an injected field if needed
        private static EnglishAlphabetSequence alphabetSequence = new EnglishAlphabetSequence();

        public Option(String Name, String OptionText, bool IsCorrect, Color Color) : base()
        {
            name = Name;
            optionText = OptionText;
            correct = IsCorrect;
            color = Color;
        }

        private string _optionText;
        private string _name;
        private bool _correct;
        private Color _color;

        public Option DeepCopy()
        {
            return new Option((string)_name.Clone(), (string)_optionText.Clone(), _correct, _color);
        }

        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Properties
        public String optionText 
        {
            get
            {
                return _optionText;
            }
            set
            {
                if (value != _optionText)
                {
                    _optionText = value;
                    NotifyPropertyChanged("optionText");
                }
            }                
        }
        public String name 
        { 
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("name");
                }
            }
        }
        public bool correct
        {
            get
            {
                return _correct;
            }
            set
            {
                if (value != _correct)
                {
                    _correct = value;
                    NotifyPropertyChanged("correct");
                }
            }
        }
        public Color color 
        { 
            get
            {
                return _color;
            }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    NotifyPropertyChanged("color");
                }
            } 
        }
        #endregion

        #region Generators
        public static String GetOptionNameFromIndex(int index)
        {
            return alphabetSequence.GetEncoded((uint)index);
        }
        public static String GetNextOptionName(string currentOptionName)
        {
            return alphabetSequence.GetNext(currentOptionName);
        }
        #endregion
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
            url = url == null ? String.Empty : url;
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
        public QuizQuestion DeepCopy()
        {
            var quizQuestion = new QuizQuestion(id, title, author, question, new List<Option>());
            foreach (var option in options)
            {
                quizQuestion.options.Add(option.DeepCopy());
            }

            return quizQuestion;
        }
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

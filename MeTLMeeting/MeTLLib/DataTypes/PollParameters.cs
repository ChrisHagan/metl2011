using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using MeTLLib.Providers;
using System.Collections.ObjectModel;

namespace MeTLLib.DataTypes
{
    public class Option : IEditableObject, INotifyPropertyChanged
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

        #region IEditableObject members

        public Option _cachedCopy = null;

        public void BeginEdit()
        {
            _cachedCopy = DeepCopy();
            IsInEditMode = true;
        }

        public void CancelEdit()
        {
            if (_cachedCopy != null)
            {
                _optionText = _cachedCopy.optionText;
                _name = _cachedCopy.name;
                _correct = _cachedCopy.correct;
                _color = _cachedCopy.color;
            }
        }

        public void EndEdit()
        {
            _cachedCopy = null;
            IsInEditMode = false;
        }

        private bool _isInEditMode = false;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    NotifyPropertyChanged("IsInEditMode");
                }
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
    public class QuizInfo
    {
        public QuizQuestion Question { get; set; }
        public List<QuizAnswer> Answers { get; set; }

        public QuizInfo(QuizQuestion question, List<QuizAnswer> answers )
        {
            Question = question;
            Answers = answers;
        }
        
        public void AddAnswer(QuizAnswer quizAnswer)
        {
            Answers.Add(quizAnswer);
        }
    }

    public class QuizQuestion : IEditableObject, INotifyPropertyChanged
    {
        public QuizQuestion(long id, long created, string title, string author, string question, List<Option> options)
            : this(id, title, author, question, options)
        {
            this.Created = created;
        }
        public QuizQuestion(long id, string title, string author, string question, List<Option> options)
        {
            Id = id;
            Title = title;
            Author = author;
            Question = question;
            Options = new ObservableCollection<Option>(options);
            Options.CollectionChanged += (sender, e) => { RaisePropertyChanged("Options"); };
            Url = Url == null ? String.Empty : Url;
        }
        public QuizQuestion(long id, string title, string author, string question, List<Option> options, string url)
            : this(id, title, author, question, options)
        {
            Url = url;
        }

        #region Properties

        private long _created = 0;
        public long Created
        {
            get { return _created; }
            set
            {
                _created = value;
                RaisePropertyChanged("Created");
            }
        }
        private string _title = string.Empty;
        public string Title 
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        private string _url = string.Empty;
        public string Url 
        {
            get { return _url; }
            set
            {
                _url = value;
                RaisePropertyChanged("Url");
            }
        }
        private string _question = string.Empty;
        public string Question 
        {
            get { return _question; }
            set
            {
                _question = value;
                RaisePropertyChanged("Question");
            }
        }
        private string _author = string.Empty;
        public string Author 
        {
            get { return _author; }
            set
            {
                _author = value;
                RaisePropertyChanged("Author");
            }
        }
        private ObservableCollection<Option> _options = null;
        public ObservableCollection<Option> Options 
        {
            get { return _options; }
            set
            {
                _options = value;
                RaisePropertyChanged("Options");
            }
        }
        private long _id = 0;
        public long Id 
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        private bool _deleted = false;
        public bool IsDeleted 
        { 
            get { return _deleted; } 
            set
            {
                _deleted = value;
                RaisePropertyChanged("IsDeleted");
            }
        }

        #endregion

        public QuizQuestion DeepCopy()
        {
            var quizQuestion = new QuizQuestion(Id, Title, Author, Question, new List<Option>());
            quizQuestion.IsDeleted = IsDeleted;
            quizQuestion.Url = Url;
            quizQuestion.Created = Created;
            foreach (var option in Options)
            {
                quizQuestion.Options.Add(option.DeepCopy());
            }

            return quizQuestion;
        }

        public void RemoveEmptyOptions()
        {
            Options = new ObservableCollection<Option>(Options.Where((op) => !String.IsNullOrEmpty(op.optionText)));
        }

        public void Delete()
        {
            if (IsInEditMode)
            {
                CancelEdit();
                EndEdit();
            }
            IsDeleted = true;
        }

        public bool Validate(out bool questionError, out bool optionError)
        {
            questionError = Question == null || String.IsNullOrEmpty(Question.Trim());
            optionError = Options.Where(o => o.optionText != null && !String.IsNullOrEmpty(o.optionText.Trim())).Count() < 2;

            return !(optionError || questionError);
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IEditableObject members

        public QuizQuestion _cachedCopy = null;

        public void BeginEdit()
        {
            _cachedCopy = DeepCopy();

            IsInEditMode = true;

            foreach (var option in Options)
            {
                option.BeginEdit();
            }
        }

        public void CancelEdit()
        {
            if (_cachedCopy != null)
            {
                Id = _cachedCopy.Id;
                Title = _cachedCopy.Title;
                Author = _cachedCopy.Author;
                Question = _cachedCopy.Question;
                IsDeleted = _cachedCopy.IsDeleted;
                Url = _cachedCopy.Url;
                Created = _cachedCopy.Created;
                Options.Clear();
                foreach (var option in _cachedCopy.Options)
                {
                    Options.Add(option.DeepCopy());
                }
            }

            foreach (var option in Options)
            {
                option.CancelEdit();
                option.EndEdit();
            }
            EndEdit();
        }

        public void EndEdit()
        {
            foreach (var option in Options)
            {
                option.EndEdit();
            }

            _cachedCopy = null;
            IsInEditMode = false;
        }

        private bool _isInEditMode = false;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    RaisePropertyChanged("IsInEditMode");
                }
            }
        }
        #endregion
    }

    public class QuizAnswer
    {
        public QuizAnswer(long Id, string Respondent, string Response, long answerTime)
        {
            this.answerTime = answerTime;
            id = Id;
            answerer = Respondent;
            answer = Response;
        }
        public string answerer { get; set; }
        public string answer { get; set; }
        public long id { get; set; }
        public long answerTime { get; set; }
    }
}

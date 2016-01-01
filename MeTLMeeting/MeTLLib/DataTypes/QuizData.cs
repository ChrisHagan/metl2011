using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MeTLLib.DataTypes
{
    public class QuizData
    {
        public ObservableCollection<QuizQuestion> activeQuizzes = new ObservableCollection<QuizQuestion>();
        public Dictionary<long, ObservableCollection<QuizAnswer>> answers = new Dictionary<long, ObservableCollection<QuizAnswer>>();
    }
}

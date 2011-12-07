using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeTLLib.Providers
{
    interface IAlphabetProvider
    {
        IEnumerable<char> GetAlphabet();
    }

    public class EnglishAlphabetProvider : IAlphabetProvider
    {
        public IEnumerable<char> GetAlphabet()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                yield return c;
            }
        }
    }

    class AlphabetSequence
    {
        private IAlphabetProvider _alphabet;

        public AlphabetSequence(IAlphabetProvider alphabet) 
        {
            _alphabet = alphabet;
        }

        public string GetNext(string current)
        {
            return Sequence().SkipWhile(x => x != current).Skip(1).First();
        }

        public IEnumerable<string> Sequence()
        {
            uint counter = 0;
            while (true)
            {
                yield return Encode(counter++);
            }
        }

        public string Encode(uint value)
        {
            string returnValue = null;
            var alphabet = _alphabet.GetAlphabet().ToArray();
            var alphabetLength = (uint)alphabet.Count();

            do
            {
                returnValue = alphabet[value % alphabetLength] + returnValue;
                value = value / alphabetLength; 
            } while (value > 0);
                
            return returnValue;
        }
    }

    public class EnglishAlphabetSequence
    {
        private AlphabetSequence _sequence;

        public EnglishAlphabetSequence()
        {
            _sequence = new AlphabetSequence(new EnglishAlphabetProvider());
        }

        public string GetNext(string current)
        {
            return _sequence.GetNext(current);
        }

        public string GetEncoded(uint value)
        {
            return _sequence.Encode(value);
        }
    }
}

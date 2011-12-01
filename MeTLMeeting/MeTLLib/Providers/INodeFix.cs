using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeTLLib.Providers
{
    public class INodeFix
    {
        private static readonly int stemLength = 2;
        private static readonly int startingStemPosition = 5;

        public static string Stem(string source) {
            var padded = source.Split('/')[0].PadLeft(5, '0');
            return new String(padded.Reverse().Take(startingStemPosition).Reverse().Take(stemLength).ToArray());//Take the 10,000 and 1,000 columns to branch on
        }
        public static string StemBeneath(string prefix, string source){
            var nofix = source.StartsWith(prefix) ? source.Substring(prefix.Length) : source;
            var deStemmed = DeStem(nofix);
            return string.Format("{0}{1}/{2}", prefix, Stem(deStemmed.Split('/')[0]), deStemmed);
        }
        public static string StripServer(string source){
            try
            {
                var uri = new Uri(source);
                return uri.PathAndQuery;
            }
            catch (UriFormatException) {
                return source;
            }
        }
        public static string DeStem(string source) {
            var sourceParts = source.Split('/').ToList();
            var outputString = source;
            var potentialStems = sourceParts.Where(s => s.Length == stemLength).ToList();
            foreach (var stem in potentialStems)
            {
                if (stem == Stem(sourceParts[sourceParts.IndexOf(stem) + 1]))
                    outputString = sourceParts.Where(s => s != stem).Aggregate("", (acc, item) => acc + "/" + item).ToString();
                var len = outputString.TakeWhile(c => c == '/').Count();
                outputString = outputString.Substring(len);
                if (source.StartsWith("/"))
                {
                    outputString = outputString.Insert(0, "/");
                }
            }
            return outputString;
        }
    }
}

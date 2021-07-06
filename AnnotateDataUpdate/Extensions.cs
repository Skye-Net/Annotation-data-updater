using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnnotateDataUpdate
{
    public static class Extensions
    {
        public static string ReplaceAtIndex(int i, char value, string word)
        {
            var letters = word.ToCharArray();
            letters[i] = value;
            return string.Join("", letters);
        }
        
        public static IEnumerable<string> FilterFiles(string path, params string[] exts) {
            return 
                exts.Select(x => "*." + x) // turn into globs
                    .SelectMany(x => 
                        Directory.GetFiles(path, x)
                    );
        }
    }
}
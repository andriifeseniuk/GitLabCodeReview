using GitLabCodeReview.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitLabCodeReview.Helpers
{
    public static class HunkHelper
    {
        //private const string rawLinePattern = @".*?((?<!\\)\\n|$)";
        private const string hunkHeaderPattern = @"^@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@";
        private const string hunkPattern = @"@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@\\n[\s*ï»¿]*(?<line>.*^[\\](\\n|$))+";
        // @@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@\\n(?<line>.*?(\\n|$))+

        //@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@.*?\\n

        //.*?(?<!\\)\\n  negaive lookbehind
        //.*?((?<!\\)\\n|$) - raw line
        // .*?[^\\](\\n|$) - using not but wont work for empty lines 

        //@@\s*\-\d+,\s*\d+\s*\+\d+,\s*\d+\s*@@

        //(?<=(\\n)|^)@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@   - positive lookbehind

        //(?<=(\\n)|^)@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@.*?((?<!\\)\\n|$)

        //(?<=(\\n)|^)@@\s*\-(?<start1>\d+),\s*(?<length1>\d+)\s*\+(?<start2>\d+),\s*(?<length2>\d+)\s*@@.*?((?<!\\)\\n|$)

        public static IEnumerable<Hunk> ParseHunks(string diff)
        {
            //var rawLineMatches = Regex.Matches(diff, rawLinePattern);
            //var rawLines = rawLineMatches.Cast<Match>().Select(m => m.Value).ToArray();
            var rawLines = diff.Split('\n').ToArray();
            Hunk currentHunk = null;
            var numberInSourceFile = 0;
            var numberInTargetFile = 0;
            var hunks = new List<Hunk>();
            foreach(var rawLine in rawLines)
            {
                var hunkMatch = Regex.Match(rawLine, hunkHeaderPattern);
                if (hunkMatch.Success)
                {
                    currentHunk = new Hunk();

                    var start1 = hunkMatch.Groups["start1"].Value;
                    var lenght1 = hunkMatch.Groups["length1"].Value;
                    var start2 = hunkMatch.Groups["start2"].Value;
                    var lenght2 = hunkMatch.Groups["length2"].Value;

                    currentHunk.StartInTargetFile = int.Parse(start1);
                    currentHunk.LenghtInTargetFile = int.Parse(lenght1);
                    currentHunk.StartInSourceFile = int.Parse(start2);
                    currentHunk.LengthInSourceFile = int.Parse(lenght2);

                    hunks.Add(currentHunk);
                    numberInSourceFile = currentHunk.StartInSourceFile;
                    numberInTargetFile = currentHunk.StartInTargetFile;
                    continue;
                }

                if (currentHunk == null)
                {
                    continue;
                }

                var line = new HunkLine();
                line.Text = rawLine;
                if (rawLine.StartsWith("-"))
                {
                    line.NumberInTargetFile = numberInTargetFile;
                    numberInTargetFile++;
                }
                else if (rawLine.StartsWith("+"))
                {
                    line.NumberInSourceFile = numberInSourceFile;
                    numberInSourceFile++;
                }
                else
                {
                    line.NumberInSourceFile = numberInSourceFile;
                    line.NumberInTargetFile = numberInTargetFile;
                    numberInTargetFile++;
                    numberInSourceFile++;
                }

                currentHunk.Lines.Add(line);
            }

            return hunks;
        } 
    }
}

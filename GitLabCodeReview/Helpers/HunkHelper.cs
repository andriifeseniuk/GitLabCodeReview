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

        public static Hunk[] ParseHunks(string diff, out Gap[] gaps)
        {
            //var rawLineMatches = Regex.Matches(diff, rawLinePattern);
            //var rawLines = rawLineMatches.Cast<Match>().Select(m => m.Value).ToArray();
            var rawLines = diff.Split('\n').ToArray();
            Hunk currentHunk = null;
            Hunk previousHunk = null;
            var numberInChanges = 0;
            var numberInSourceFile = 0;
            var numberInTargetFile = 0;
            var hunks = new List<Hunk>();
            var gapsList = new List<Gap>();
            foreach(var rawLine in rawLines)
            {
                var hunkMatch = Regex.Match(rawLine, hunkHeaderPattern);
                if (hunkMatch.Success)
                {
                    previousHunk = currentHunk;
                    currentHunk = new Hunk();

                    var start1 = hunkMatch.Groups["start1"].Value;
                    var lenght1 = hunkMatch.Groups["length1"].Value;
                    var start2 = hunkMatch.Groups["start2"].Value;
                    var lenght2 = hunkMatch.Groups["length2"].Value;

                    currentHunk.StartInTargetFile = int.Parse(start1);
                    currentHunk.LengthInTargetFile = int.Parse(lenght1);
                    currentHunk.StartInSourceFile = int.Parse(start2);
                    currentHunk.LengthInSourceFile = int.Parse(lenght2);

                    if (previousHunk == null)
                    {
                        currentHunk.StartInChanges = currentHunk.StartInSourceFile;
                        if (currentHunk.StartInSourceFile > 1)
                        {
                            var gap = new Gap();
                            gap.StartInSourceFile = 1;
                            gap.StartInTargetFile = 1;
                            gap.StartInChanges = 1;
                            gap.Length = currentHunk.StartInSourceFile - 1;
                            gapsList.Add(gap);
                        }
                    }
                    else
                    {
                        var gapLength = currentHunk.StartInSourceFile - previousHunk.EndInSourceFile - 1;
                        currentHunk.StartInChanges = previousHunk.EndInChanges + gapLength + 1;
                        var gap = new Gap();
                        gap.Length = gapLength;
                        gap.StartInSourceFile = previousHunk.EndInSourceFile + 1;
                        gap.StartInTargetFile = previousHunk.EndInTargetFile + 1;
                        gap.StartInChanges = previousHunk.EndInChanges + 1;
                        gapsList.Add(gap);
                    }

                    hunks.Add(currentHunk);
                    numberInChanges = currentHunk.StartInChanges;
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
                line.NumberInChanges = numberInChanges;
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

                numberInChanges++;
                currentHunk.Lines.Add(line);
            }

            gaps = gapsList.ToArray();
            return hunks.ToArray();
        }

        public static Hunk TryGetHunk(IEnumerable<Hunk> hunks, int? numberInSourceFile, int? numberInTargetFile)
        {
            if (numberInSourceFile != null)
            {
                return hunks.FirstOrDefault(hunk =>
                    hunk.StartInSourceFile <= numberInSourceFile
                    && numberInSourceFile < hunk.StartInSourceFile + hunk.LengthInSourceFile);
            }

            if (numberInTargetFile != null)
            {
                return hunks.FirstOrDefault(hunk =>
                    hunk.StartInTargetFile <= numberInTargetFile
                    && numberInTargetFile < hunk.StartInTargetFile + hunk.LengthInTargetFile);
            }

            return null;
        }

        public static bool IsInSourceGap(this Gap gap, int number)
        {
            if (gap == null)
            {
                return false;
            }

            return gap.StartInSourceFile <= number && number <= gap.EndInSourceFile;
        }

        public static bool IsInTargetGap(this Gap gap, int number)
        {
            if (gap == null)
            {
                return false;
            }

            return gap.StartInTargetFile <= number && number <= gap.EndInTargetFile;
        }
    }
}

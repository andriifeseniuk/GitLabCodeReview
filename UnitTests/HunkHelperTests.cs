using GitLabCodeReview.Helpers;
using GitLabCodeReview.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class HunkHelperTests
    {
        [TestMethod]
        public void ParseHunksTest()
        {
            var diff = "@@ -1,14 +1,10 @@\n ﻿using System;\n-using System.Collections.Generic;\n-using System.Linq;\n-using System.Text;\n-using System.Threading.Tasks;\n \n namespace HelloGitLab\n {\n     class Program\n     {\n-        static void Main(string[] args)\n+        static void Main()\n         {\n             var a = 1;\n             a++;\n@@ -99,6 +95,8 @@ namespace HelloGitLab\n             a++;\n             Console.WriteLine(a);\n             Console.WriteLine(\"Hello GitLab\");\n+            Console.WriteLine(\"\\n\");\n+            Console.WriteLine(\"@@ -1,14 +1,10 @@\");\n             Console.ReadKey();\n         }\n     }\n";
            Gap[] gaps;
            var hunks = HunkHelper.ParseHunks(diff, out gaps).ToList();
            Assert.AreEqual(2, hunks.Count);
            Assert.AreEqual(5, hunks[0].Lines.Count(line => line.IsLineRemoved));
            Assert.AreEqual(1, hunks[0].Lines.Count(line => line.IsLineAdded));
            Assert.AreEqual(0, hunks[1].Lines.Count(line => line.IsLineRemoved));
            Assert.AreEqual(2, hunks[1].Lines.Count(line => line.IsLineAdded));
        }
    }
}

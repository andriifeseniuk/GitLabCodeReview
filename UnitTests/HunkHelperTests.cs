using GitLabCodeReview.Helpers;
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
            var hunks = HunkHelper.ParseHunks(diff);
            Assert.AreEqual(2, hunks.Count());
        }
    }
}

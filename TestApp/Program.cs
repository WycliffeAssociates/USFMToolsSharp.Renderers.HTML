using System;
using System.Collections.Generic;
using System.IO;
using USFMToolsSharp;
using USFMToolsSharp.Renderers.HTML;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("44-JHN.usfm");
            var parser = new USFMParser(new List<String> { "s5" });
            var usfmDoc = parser.ParseFromString(text);
            var renderer = new HtmlRenderer();
            var htmlDoc = renderer.Render(usfmDoc);
            File.WriteAllText("44-JHN.html", htmlDoc);
        }
    }
}

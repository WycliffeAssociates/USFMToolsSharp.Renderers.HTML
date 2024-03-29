﻿using System;
using System.Collections.Generic;
using System.Text;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.HTML
{
    public class HTMLConfig
    {
        public List<string> divClasses;
        public bool separateChapters;
        public bool separateVerses;
        public bool blankColumn;
        public bool partialHTML;
        public bool renderTableOfContents;
        public string ChapterIdPattern;
        public string VerseIdPattern;

        public HTMLConfig()
        {
            divClasses = new List<string>();
            separateChapters = false;
            separateVerses = false;
            blankColumn = false;
            partialHTML = false;
            renderTableOfContents = false;
        }
        public HTMLConfig(List<string> divClasses, bool separateChapters = false, bool partialHTML = false, bool separateVerses=false, bool blankColumn = false)
        {
            this.divClasses = divClasses;
            this.separateChapters = separateChapters;
            this.separateVerses = separateVerses;
            this.blankColumn = blankColumn;
            this.partialHTML = partialHTML;
        }
        
        
    }
}

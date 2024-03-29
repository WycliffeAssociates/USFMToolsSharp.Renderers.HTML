﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USFMToolsSharp.Models.Markers;



namespace USFMToolsSharp.Renderers.HTML
{
    public class HtmlRenderer
    {
        public List<string> UnrenderableTags;
        public List<string> FootnoteTextTags;
        public List<string> CrossReferenceTags;
        public HTMLConfig ConfigurationHTML;
        public string currentChapterLabel;
        private IList<string> TOCEntries;
        private CMarker CurrentChapter;
        private VMarker CurrentVerse;
        private int NextFootnoteUniqueID = 1;
	private USFMDocument document;

        public string FrontMatterHTML { get; set; }
        public string InsertedFooter { get; set;}
        public string InsertedHead { get; set; }

        public HtmlRenderer()
        {
            UnrenderableTags = new List<string>();
            FootnoteTextTags = new List<string>();
            CrossReferenceTags = new List<string>();
            ConfigurationHTML = new HTMLConfig();
            TOCEntries = new List<string>();
        }
        public HtmlRenderer(HTMLConfig config):this()
        {
            ConfigurationHTML = config;
        }
        public string Render(USFMDocument input)
        {
            document = input;
            UnrenderableTags = new List<string>();
            var encoding = GetEncoding(input);
            StringBuilder output = new StringBuilder();
            NextFootnoteUniqueID = 1;

            if (!ConfigurationHTML.partialHTML)
            {
                output.AppendLine("<html>");

                if (InsertedHead == null)
                {
                    output.AppendLine("<head>");
                    if (!string.IsNullOrEmpty(encoding))
                    {
                        output.Append($"<meta charset=\"{encoding}\">");
                    }
                    output.AppendLine("<link rel=\"stylesheet\" href=\"style.css\">");

                    output.AppendLine("</head>");
                }
                else
                {
                    output.AppendLine(InsertedHead);
                }

                output.AppendLine("<body>");

                output.AppendLine(FrontMatterHTML);
            }

            var bodyContent = new StringBuilder();

            // HTML tags can only have one class, when render to docx
            foreach (string class_name in ConfigurationHTML.divClasses)
            {
                bodyContent.AppendLine($"<div class=\"{class_name}\">");
            }

            // Will add a table with two columns where the second column will be blank
            if(ConfigurationHTML.blankColumn)
            {
                bodyContent.AppendLine("<table class=\"blank_col\"><tr><td>");
            }

            foreach (Marker marker in input.Contents)
            {
                bodyContent.Append(RenderMarker(marker));
            }

            // render Table of Contents before body content
            if (ConfigurationHTML.renderTableOfContents && TOCEntries.Count > 0)
            {
                output.AppendLine(RenderTOC());
                output.AppendLine("<br/>");
            }

            if (ConfigurationHTML.blankColumn)
            {
                bodyContent.AppendLine("</td><td></td></tr></table>");
            }

            foreach (string class_name in ConfigurationHTML.divClasses)
            {
                bodyContent.AppendLine($"</div>");
            }

            if (!ConfigurationHTML.partialHTML)
            {
                bodyContent.AppendLine(InsertedFooter);
                bodyContent.AppendLine("</body>");
                bodyContent.AppendLine("</html>");
            }

            output.Append(bodyContent);

            return output.ToString();
        }
        private string GetEncoding(USFMDocument input)
        {
            var encodingSearch = input.GetChildMarkers<IDEMarker>();
            if (encodingSearch.Count > 0)
            {
                return encodingSearch[0].Encoding;
            }
            return null;
        }
        private string RenderMarker(Marker input)
        {
            StringBuilder output = new StringBuilder();
            switch (input)
            {
                case PMarker _:
                    output.AppendLine("<p>");
                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</p>");
                    break;
                case PIMarker piMarker:
                    output.AppendLine($"<p class=\"para-indent-{piMarker.Depth}\">");
                    foreach (var marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</p>");
                    break;
                case CMarker cMarker:
                    CurrentChapter = cMarker;
                    if (ConfigurationHTML.ChapterIdPattern == null)
                    {
                        output.AppendLine("<div class=\"chapter\">");
                    }
                    else
                    {
                        output.AppendLine($"<div id=\"{string.Format(ConfigurationHTML.ChapterIdPattern, cMarker.Number)}\" class=\"chapter\">");
                    }

                    if (cMarker.GetChildMarkers<CLMarker>().Count > 0)
                    {
                        output.AppendLine($"<div class=\"chapterlabel\">{cMarker.CustomChapterLabel}</div>");
                        output.AppendLine("<br/>");
                    }
                    else if (currentChapterLabel != null)
                    {
                        output.AppendLine($"<div class=\"chapterlabel\">{currentChapterLabel} {cMarker.PublishedChapterMarker}</div>");
                        output.AppendLine("<br/>");
                    }
                    else
                    {
                        output.AppendLine($"<span class=\"chaptermarker\">{cMarker.PublishedChapterMarker}</span>");
                    }

                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    output.AppendLine(RenderFootnotes());
                    output.AppendLine(RenderCrossReferences());

                    // Page breaks after each chapter
                    if (ConfigurationHTML.separateChapters)
                    {
                        output.AppendLine("<br class=\"pagebreak\"></br>");
                        output.AppendLine("<div class=\"pagebreak\"></div>");
                    }

                    break;
                case CAMarker cAMarker:
                    output.AppendLine($"<span class=\"chaptermarker-alt\">({cAMarker.AltChapterNumber})</span>");
                    break;
                case CLMarker cLMarker:
                    currentChapterLabel = cLMarker.Label;
                    break;
                case VMarker vMarker:
                    CurrentVerse = vMarker;
                    output.AppendLine($"<span class=\"verse\">");
                    output.AppendLine($"<sup class=\"versemarker\">{vMarker.VerseCharacter}</sup>");
                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine($"</span>");

                    // New Line after each Verse
                    if (ConfigurationHTML.separateVerses)
                    {
                        output.AppendLine("<br/>");
                    }
                    break;
                case VAMarker vAMarker:
                    output.AppendLine($"<sup class=\"versemarker-alt\">({vAMarker.AltVerseNumber})</sup>");
                    break;
                case QMarker qMarker:
                    QMarker parentQ = document
                        .GetHierarchyToMarker(qMarker)
                        .LastOrDefault(marker => marker is QMarker && marker != input) 
                        as QMarker;

                    if (parentQ == null)
                    {
                        output.AppendLine($"<div class=\"poetry-{qMarker.Depth}\">");
                    }
                    else // nested q, add css class based on diff calculation
                    {
                        int depthOffset = qMarker.Depth - parentQ.Depth;
                        if (depthOffset > 0)
                        {
                            output.AppendLine($"<div class=\"poetry-{depthOffset}\">");
                        }
                        else if (depthOffset < 0)
                        {
                            output.AppendLine($"<div class=\"poetry-outdent-{-depthOffset}\">");
                        }
                        else
                        {
                            output.AppendLine($"<div>");
                        }
                    }

                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case MMarker mMarker:
                    output.AppendLine("<div class=\"resetmargin\">");
                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case TextBlock textBlock:
                    output.AppendLine(textBlock.Text);
                    break;
                case BDMarker bdMarker:
                    output.AppendLine("<b>");
                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</b>");
                    break;
                case ITMarker iTMarker:
                    output.AppendLine("<i>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</i>");
                    break;
                case BDITMarker bditMarker:
                    output.AppendLine("<span class=\"bold-italic\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case EMMarker emMarker:
                    output.AppendLine("<span class=\"emphasis\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case NOMarker noMarker:
                    output.AppendLine("<span class=\"normal-text\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case NDMarker ndMarker:
                    output.AppendLine("<span class=\"deity-name\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case SUPMarker supMarker:
                    output.AppendLine("<span class=\"superscript-text\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case HMarker hMarker:
                    output.AppendLine("<div class=\"header\">");
                    output.Append(hMarker.HeaderText);
                    output.AppendLine("</div>");
                    break;
                case MTMarker mTMarker:
                    output.AppendLine($"<div class=\"majortitle-{mTMarker.Weight}\">");
                    output.AppendLine(mTMarker.Title);
                    output.AppendLine("</div>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                case MSMarker mSMarker:
                    output.AppendLine($"<div class=\"sectionhead-{mSMarker.Weight}\">");
                    output.AppendLine(mSMarker.Heading);
                    output.AppendLine("</div>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                case MRMarker mRMarker:
                    output.AppendLine($"<div class=\"major-section-reference\">");
                    output.Append(mRMarker.SectionReference);
                    output.Append("</div>");
                    break;
                case FMarker fMarker:
                    StringBuilder footnote = new StringBuilder();
                    string footnoteId;
                    switch (fMarker.FootNoteCaller)
                    {
                        case "-":
                            footnoteId = "";
                            break;
                        case "+":
                            footnoteId = $"{FootnoteTextTags.Count+1}";
                            break;
                        default:
                            footnoteId = fMarker.FootNoteCaller;
                            break;
                    }
                    string footnoteCallerHTML = $"<sup id=\"footnote-caller-{NextFootnoteUniqueID}\" class=\"caller\"><a href=\"#footnote-target-{NextFootnoteUniqueID}\">{footnoteId}</a></sup>";
                    string footnoteTargetHTML = $"<sup id=\"footnote-target-{NextFootnoteUniqueID}\" class=\"caller\"><a href=\"#footnote-caller-{NextFootnoteUniqueID}\">{footnoteId}</a></sup>";
                    NextFootnoteUniqueID++;
                    output.AppendLine(footnoteCallerHTML);
                    footnote.Append(footnoteTargetHTML);
                    foreach (Marker marker in input.Contents)
                    {
                        footnote.Append(RenderMarker(marker));
                    }
                    FootnoteTextTags.Add(footnote.ToString());
                    break;
                case FPMarker fPMarker:
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                case FTMarker fTMarker:
                    foreach(Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                case FRMarker fRMarker:
                    output.Append($"<b> {fRMarker.VerseReference} </b>");
                    break;
                case FKMarker fKMarker:
                    output.Append($" {fKMarker.FootNoteKeyword.ToUpper()}: ");
                    break;
                case FQAMarker _:
                    output.Append("<span class=\"footnote-alternate-translation\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.Append("</span>");
                    break;
                case BMarker bMarker:
                    output.Append("<br/><br/>");
                    break;
                case SMarker sMarker:
                    output.AppendLine($"<div class=\"sectionhead-{sMarker.Weight}\">");
                    output.AppendLine(sMarker.Text);
                    output.AppendLine("</div>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
                case BKMarker bkMarker:
                    output.AppendLine($"<span class=\"quoted-book\">");
                    output.AppendLine(bkMarker.BookTitle);
                    output.AppendLine("</span>");
                    break;
                case LIMarker liMarker:
                    output.AppendLine($"<div class=\"list-{liMarker.Depth}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case ADDMarker addMarker:
                    output.AppendLine($"<span class=\"additions\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case TLMarker tlMarker:
                    output.AppendLine($"<span class=\"transliterated\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case SCMarker scMarker:
                    output.AppendLine($"<span class=\"small-caps\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case WMarker wMarker:
                    output.AppendLine($"<span class=\"word-entry\"> {wMarker.Term} </span>");
                    break;
                case XMarker xMarker:
                    StringBuilder crossRef = new StringBuilder();
                    string crossId;
                    switch (xMarker.CrossRefCaller)
                    {
                        case "-":
                            crossId = "";
                            break;
                        case "+":
                            crossId = $"{CrossReferenceTags.Count + 1}";
                            break;
                        default:
                            crossId = xMarker.CrossRefCaller;
                            break;
                    }
                    string crossCallerHTML = $"<sup class=\"caller\">{crossId}</sup>";
                    output.AppendLine(crossCallerHTML);
                    crossRef.AppendLine(crossCallerHTML);
                    foreach (Marker marker in input.Contents)
                    {
                        crossRef.AppendLine(RenderMarker(marker));
                    }
                    CrossReferenceTags.Add(crossRef.ToString());
                    break;
                case XOMarker xOMarker:
                    output.AppendLine($"<b> {xOMarker.OriginRef} </b>");
                    break;
                case XTMarker xTMarker:
                    foreach (Marker marker in input.Contents)
                    {
                        output.AppendLine(RenderMarker(marker));
                    }
                    break;
                case XQMarker xQMarker:
                    output.AppendLine("<span class=\"cross-ref-quote\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.AppendLine(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case FQMarker fqMarker:
                    output.Append("<span class=\"footnote-alternate-translation\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.Append("</span>");
                    break;
                case FVMarker fVMarker:
                    output.AppendLine($"<sup class=\"versemarker\">{fVMarker.VerseCharacter}</sup>");
                    break;
                case TableBlock table:
                    output.AppendLine("<div>");
                    output.AppendLine("<table class=\"table-block\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</table>");
                    output.AppendLine("</div>");
                    break;
                case TRMarker tRMarker:
                    output.AppendLine("<tr>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</tr>");
                    break;
                case THMarker tHMarker:
                    output.AppendLine($"<td class=\"table-head\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</td>");
                    break;
                case THRMarker tHRMarker:
                    output.AppendLine($"<td class=\"table-head-right\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</td>");
                    break;
                case TCMarker tCMarker:
                    output.AppendLine("<td class=\"table-cell\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</td>");
                    break;
                case TCRMarker tCRMarker:
                    output.AppendLine("<td class=\"table-cell-right\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</td>");
                    break;
                case PCMarker pCMarker:
                    output.Append("<div class=\"center-paragraph\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.Append("</div>");
                    break;
                case CLSMarker cLSMarker:
                    output.Append("<div class=\"closing\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case RMarker rMarker:
                    output.AppendLine($"<div class=\"section-reference\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.Append("</div>");
                    break;
                case RQMarker rQMarker:
                    output.AppendLine($"<div class=\"reference\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.Append("</div>");
                    break;
                case QSMarker qSMarker:
                    output.Append("<div class=\"selah-text\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case QRMarker qRMarker:
                    output.Append("<div class=\"poetry-right\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case QCMarker qCMarker:
                    output.Append("<div class=\"poetry-center\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case QDMarker qDMarker:
                    output.Append("<div class=\"hebrew-note\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case QACMarker qACMarker:
                    output.AppendLine($"<span class=\"acrostic-letter\">{qACMarker.AcrosticLetter}</span>");
                    break;
                case QAMarker qAMarker:
                    output.AppendLine($"<span class=\"acrostic-heading\">{qAMarker.Heading}</span>");
                    break;
                case QMMarker qMMarker:
                    output.Append($"<div class=\"embedded-poetry-{qMMarker.Depth}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case DMarker dMarker:
                    output.Append("<div class=\"descriptive-text\">");
                    output.AppendLine(dMarker.Description);
                    output.AppendLine("</div>");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    break;
            /* Introduction */
                case IMTMarker iMTMarker:
                    output.Append($"<div class=\"intro-title-{iMTMarker.Weight}\">");
                    output.AppendLine(iMTMarker.IntroTitle);
                    output.AppendLine("</div>");
                    break;
                case ISMarker iSMarker:
                    output.Append($"<div class=\"intro-head-{iSMarker.Weight}\">");
                    output.AppendLine(iSMarker.Heading);
                    output.AppendLine("</div>");
                    break;
                case IBMarker iBMarker:
                    output.Append("<br/><br/>");
                    break;
                case IQMarker iqMarker:
                    output.AppendLine($"<div class=\"poetry-{iqMarker.Depth}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IPMarker iPMarker:
                    output.Append($"<div class=\"intro-para\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IPIMarker iPIMarker:
                    output.Append($"<div class=\"intro-para-indent\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IMMarker iMMarker:
                    output.Append($"<div class=\"intro-para-flush\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case ILIMarker iliMarker:
                    output.AppendLine($"<div class=\"list-{iliMarker.Depth}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IMIMarker iMIMarker:
                    output.Append($"<div class=\"intro-para-flush-indent\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IOTMarker iotMarker:
                    output.AppendLine($"<div class=\"outline-title\">{iotMarker.Title}</div>");
                    break;
                case IOMarker ioMarker:
                    output.AppendLine($"<div class=\"outline-entry-{ioMarker.Depth}\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IPQMarker iPQMarker:
                    output.Append($"<div class=\"intro-quote-indent\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IMQMarker iMQMarker:
                    output.Append($"<div class=\"intro-quote-flush\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IORMarker iorMarker:
                    output.AppendLine($"<span class=\"outline-ref\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</span>");
                    break;
                case IPRMarker iPRMarker:
                    output.Append($"<div class=\"intro-right-align\">");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker));
                    }
                    output.AppendLine("</div>");
                    break;
                case IDMarker idMarker:
                    currentChapterLabel = null;
                    break;
                case TOC2Marker tocMarker:
                    if (ConfigurationHTML.renderTableOfContents)
                    {
                        string name = tocMarker.ShortTableOfContentsText;
                        output.AppendLine($"<div class=\"toc2-ref\" id=\"{name}\"></div>");
                        TOCEntries.Add(name);
                    }
                    break;
                case IOREndMarker _:
                case SUPEndMarker _:
                case NDEndMarker _:
                case NOEndMarker _:
                case BDITEndMarker _:
                case EMEndMarker _:
                case QACEndMarker _:
                case QSEndMarker _:
                case XEndMarker _:
                case WEndMarker _:
                case RQEndMarker _:
                case FVEndMarker _:
                case TLEndMarker _:
                case SCEndMarker _:
                case ADDEndMarker _:
                case BKEndMarker _:
                case FEndMarker _:
                case IDEMarker _:
                case VPMarker _:
                case VPEndMarker _:
                case USFMMarker _:
                    break;
                default:
                    UnrenderableTags.Add(input.Identifier);
                    break;
            }

            return output.ToString();
        }
        private string RenderFootnotes()
        {
            if (FootnoteTextTags.Count > 0)
            {
                
                StringBuilder footnoteHTML = new StringBuilder();
                footnoteHTML.AppendLine("<hr/>");
                foreach (string footnote in FootnoteTextTags)
                {
                    footnoteHTML.AppendLine("<div class=\"footnotes\">");
                    footnoteHTML.Append(footnote);
                    footnoteHTML.AppendLine("</div>");
                }
                footnoteHTML.AppendLine("<hr/>");
                FootnoteTextTags.Clear();
                return footnoteHTML.ToString();
            }
            return string.Empty;
        }
        private string RenderCrossReferences()
        {
            if (CrossReferenceTags.Count > 0)
            {
                StringBuilder crossRefHTML = new StringBuilder();
                crossRefHTML.AppendLine("<hr/>");
                foreach (string crossRef in CrossReferenceTags)
                {
                    crossRefHTML.AppendLine("<span class=\"cross-ref\">");
                    crossRefHTML.Append(crossRef);
                    crossRefHTML.AppendLine(" </span>");
                }
                CrossReferenceTags.Clear();
                return crossRefHTML.ToString();
            }
            return string.Empty;
        }

        private string RenderTOC()
        {
            var toc = new StringBuilder();
            toc.AppendLine("<div class=\"toc\"><h2>Table of Contents</h2>");
            toc.AppendLine("<ul>");
            foreach (var entry in TOCEntries)
            {
                toc.AppendLine(string.Format("<li><h3><a href=\"#{0}\">{0}</a></h3></li>", entry));
            }
            toc.AppendLine("</ul>");
            toc.AppendLine("</div>");
            return toc.ToString();
        }
    }
}

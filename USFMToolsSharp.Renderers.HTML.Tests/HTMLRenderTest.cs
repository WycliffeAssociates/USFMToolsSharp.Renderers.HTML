using System;
using System.Collections.Generic;
using NUnit.Framework;
using USFMToolsSharp;
using USFMToolsSharp.Renderers.HTML;

namespace USFMToolsSharpTest
{

    public class HTMLRenderTest
    {
        private USFMParser parser;
        private HtmlRenderer render;
        private HTMLConfig configHTML;

        [SetUp]
        public void SetUpTestCase()
        {
            configHTML = new HTMLConfig(new List<string>(), partialHTML: true);

            parser = new USFMParser();
            render = new HtmlRenderer(configHTML);

        }
        [Test]
        public void TestSectionRender()
        {
            // Section Headings
            Assert.That(WrapTest("\\s Silsilah Yesus Kristus \\r (Luk. 3:23 - 38)"), Is.EqualTo("<div class=\"sectionhead-1\">Silsilah Yesus Kristus </div><div class=\"section-reference\">(Luk. 3:23 - 38)</div>"));
            Assert.That(WrapTest("\\s3 Silsilah Yesus Kristus \\r (Luk. 3:23 - 38)"), Is.EqualTo("<div class=\"sectionhead-3\">Silsilah Yesus Kristus </div><div class=\"section-reference\">(Luk. 3:23 - 38)</div>"));

            // Major Section
            Assert.That(WrapTest("\\ms2 jilid 1 \\mr (Mazmur 1 - 41)"), Is.EqualTo("<div class=\"sectionhead-2\">jilid 1 </div><div class=\"major-section-reference\">(Mazmur 1 - 41)</div>"));
            Assert.That(WrapTest("\\ms3 jilid 1 \\mr (Mazmur 1 - 41)"), Is.EqualTo("<div class=\"sectionhead-3\">jilid 1 </div><div class=\"major-section-reference\">(Mazmur 1 - 41)</div>"));
            Assert.That(WrapTest("\\ms jilid 1 \\mr (Mazmur 1 - 41)"), Is.EqualTo("<div class=\"sectionhead-1\">jilid 1 </div><div class=\"major-section-reference\">(Mazmur 1 - 41)</div>"));

            // References
            Assert.That(WrapTest("\\ms2 jilid 1 \\mr (Mazmur)"), Is.EqualTo("<div class=\"sectionhead-2\">jilid 1 </div><div class=\"major-section-reference\">(Mazmur)</div>"));

        }
        [Test]
        public void TestMajorTitleRender()
        {
            Assert.That(WrapTest("\\mt1 Keluaran"), Is.EqualTo("<div class=\"majortitle-1\">Keluaran</div>"));
            Assert.That(WrapTest("\\mt3 Keluaran"), Is.EqualTo("<div class=\"majortitle-3\">Keluaran</div>"));
            Assert.That(WrapTest("\\mt Keluaran"), Is.EqualTo("<div class=\"majortitle-1\">Keluaran</div>"));
            Assert.That(WrapTest("\\mt2 Keluaran"), Is.EqualTo("<div class=\"majortitle-2\">Keluaran</div>"));
        }
        [Test]
        public void TestHeaderRender()
        {
            Assert.That(WrapTest("\\h Genesis"), Is.EqualTo("<div class=\"header\">Genesis</div>"));
            Assert.That(WrapTest("\\h 1 John"), Is.EqualTo("<div class=\"header\">1 John</div>"));
            Assert.That(WrapTest("\\h"), Is.EqualTo("<div class=\"header\"></div>"));
            Assert.That(WrapTest("\\h      "), Is.EqualTo("<div class=\"header\"></div>"));

        }
        [Test]
        public void TestChapterRender()
        {

            // Chapter Numbers
            Assert.That(WrapTest("\\c 1"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span></div>"));
            Assert.That(WrapTest("\\c 1000"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1000</span></div>"));
            Assert.That(WrapTest("\\c 0"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">0</span></div>"));

            // Chapter Labels
            Assert.That(WrapTest("\\c 1 \\cl Chapter One"), Is.EqualTo("<div class=\"chapter\"><div class=\"chapterlabel\">Chapter One</div><br/></div>"));
            Assert.That(WrapTest("\\c 1 \\cl Chapter One \\c 2 \\cl Chapter Two"), Is.EqualTo("<div class=\"chapter\"><div class=\"chapterlabel\">Chapter One</div><br/></div><div class=\"chapter\"><div class=\"chapterlabel\">Chapter Two</div><br/></div>"));
            Assert.That(WrapTest("\\cl Chapter \\c 1 \\c 2"), Is.EqualTo("<div class=\"chapter\"><div class=\"chapterlabel\">Chapter 1</div><br/></div><div class=\"chapter\"><div class=\"chapterlabel\">Chapter 2</div><br/></div>"));
            Assert.That(WrapTest("\\id Genesis \\cl Chapter \\c 1 \\c 2 \\id Psalms \\cl Psalm \\c 1 \\c 2"), Is.EqualTo("<div class=\"chapter\"><div class=\"chapterlabel\">Chapter 1</div><br/></div><div class=\"chapter\"><div class=\"chapterlabel\">Chapter 2</div><br/></div><div class=\"chapter\"><div class=\"chapterlabel\">Psalm 1</div><br/></div><div class=\"chapter\"><div class=\"chapterlabel\">Psalm 2</div><br/></div>"));


        }

        [Test]
        public void TestVerseRender()
        {

            Assert.That(WrapTest("\\v 200 Genesis"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">200</sup>Genesis</span>"));
            Assert.That(WrapTest("\\v 0 fff"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">0</sup>fff</span>"));
            Assert.That(WrapTest("\\c 1  \\v 1 asdfasdf"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup>asdfasdf</span></div>"));

            // References - Quoted book title - Parallel passage reference
            Assert.That(WrapTest("\\v 14 Itulah sebabnya kata-kata ini ditulis dalam \\bk Kitab Peperangan TUHAN,\\bk*"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">14</sup>Itulah sebabnya kata-kata ini ditulis dalam <span class=\"quoted-book\">Kitab Peperangan TUHAN,</span></span>"));
            Assert.That(WrapTest("\\v 5 For God never said to any of his angels,\\q1 \"You are my Son;\\q2 today I have become your Father.\"\\rq Psa 2.7\\rq* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">5</sup>For God never said to any of his angels,<div class=\"poetry-1\">\"You are my Son;</div><div class=\"poetry-2\">today I have become your Father.\"</div><div class=\"reference\">Psa 2.7</div></span>"));

            // Closing - Selah
            Assert.That(WrapTest("\\cls [[ayt.co/Mat]]"), Is.EqualTo("<div class=\"closing\">[[ayt.co/Mat]]</div>"));
            Assert.That(WrapTest("\\v 3 Allah datang dari negeri Teman \\q2 dan Yang Mahakudus datang dari Gunung Paran. \\qs Sela \\qs* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">3</sup>Allah datang dari negeri Teman <div class=\"poetry-2\">dan Yang Mahakudus datang dari Gunung Paran. <div class=\"selah-text\">Sela</div></div></span>"));
            Assert.That(WrapTest("\\q2 dan sampai batu yang penghabisan. \\qs Sela \\qs*"), Is.EqualTo("<div class=\"poetry-2\">dan sampai batu yang penghabisan. <div class=\"selah-text\">Sela</div></div>"));

            // Transliterated
            Assert.That(WrapTest("\\c 1 \\v 1 \\f + \\fr 10:15 \\fk dunia orang mati \\ft Dalam bahasa Yunani adalah \\tl Hades\\tl* \\ft , tempat orang setelah meninggal.\\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">1</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">1</a></sup><b> 10:15 </b> DUNIA ORANG MATI: Dalam bahasa Yunani adalah <span class=\"transliterated\">Hades</span> , tempat orang setelah meninggal.</div><hr/>"));
            Assert.That(WrapTest("\\v 27 \\tl TEKEL\\tl* :"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">27</sup><span class=\"transliterated\">TEKEL</span> :</span>"));
        }

        [Test]
        public void TestTableRender()
        {
            // Table Rows - Cells
            Assert.That(WrapTest("\\tr \\tc1 dari suku Ruben \\tcr2 12.000"), Is.EqualTo("<div><table class=\"table-block\"><tr><td class=\"table-cell\">dari suku Ruben</td><td class=\"table-cell-right\">12.000</td></tr></table></div>"));

            // Embedded Verse
            Assert.That(WrapTest("\\tc1 \\v 6 dari suku Asyer"), Is.EqualTo("<td class=\"table-cell\"></td><span class=\"verse\"><sup class=\"versemarker\">6</sup>dari suku Asyer</span>"));

            // Table Headers
            Assert.That(WrapTest("\\tr \\th1 dari suku Ruben \\thr2 12.000"), Is.EqualTo("<div><table class=\"table-block\"><tr><td class=\"table-head\">dari suku Ruben</td><td class=\"table-head-right\">12.000</td></tr></table></div>"));


        }
        [Test]
        public void TestListRender()
        {
            // List Items
            Assert.That(WrapTest("\\li Peres ayah Hezron."), Is.EqualTo("<div class=\"list-1\">Peres ayah Hezron.</div>"));
            // Verse within List
            Assert.That(WrapTest("\\li Peres ayah Hezron. \\li \\v 19 Hezron ayah Ram."), Is.EqualTo("<div class=\"list-1\">Peres ayah Hezron.</div><div class=\"list-1\"><span class=\"verse\"><sup class=\"versemarker\">19</sup>Hezron ayah Ram.</span></div>"));
        }
        [Test]
        public void TestMIRender()
        {
            // Indented Flush Paragraph
            Assert.AreEqual("<div class=\"para-flush-indent\"> Some indented flush text.</div>", WrapTest("\\mi Some indented flush text."));
            string result = WrapTest("\\v 12 dia berkata dengan suara nyaring:\n\\mi Lorem ipsum dolor sit amet, consectetur adipiscing elit!\n\\m\n\\s5\n");
            Assert.IsTrue(
                System.Text.RegularExpressions.Regex.IsMatch(
                    result,
                    """<div class="para-flush-indent"> ?Lorem ipsum dolor sit amet, consectetur adipiscing elit!</div>"""
                ),
                result
            );
            Assert.IsFalse(result.Contains("para-flush-indent\"> ”"));
            Assert.IsTrue(result.Contains("<div class=\"resetmargin\">"));
            Assert.IsTrue(result.Contains("<div class=\"sectionhead-5\">"));
        }
        [Test]
        public void TestFootnoteRender()
        {
            // Footnote Caller - Text - Alternate Translation
            Assert.That(WrapTest("\\v 26 This is a footnote \\f + \\f*"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">26</sup>This is a footnote <sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">1</a></sup></span>"));
            Assert.That(WrapTest("\\v 26 God said, \"Let us make man in our image, after our likeness. Let them have dominion over the fish of the sea, over the birds of the sky, over the livestock, over all the earth, and over every creeping thing that creeps on the earth.\" \\f + \\ft Some ancient copies have: \\fqa ... Over the livestock, over all the animals of the earth, and over every creeping thing that creeps on the earth \\fqa*  . \\f*"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">26</sup>God said, \"Let us make man in our image, after our likeness. Let them have dominion over the fish of the sea, over the birds of the sky, over the livestock, over all the earth, and over every creeping thing that creeps on the earth.\" <sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">2</a></sup></span>"));
            Assert.That(WrapTest("\\v 1 Sam Paul! \\f + \\ft Sample Simple Footnote. \\f*"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup>Sam Paul! <sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">3</a></sup></span>"));

            //Footnote Keyword - Reference - Verse Marker
            // Footnote Caller
            Assert.That(WrapTest("\\c 1 \\v 1 \\f + \\ft Sample Simple Footnote. \\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">4</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">1</a></sup></div><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">2</a></sup>Some ancient copies have: <span class=\"footnote-alternate-translation\">... Over the livestock, over all the animals of the earth, and over every creeping thing that creeps on the earth </span>  . </div><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">3</a></sup>Sample Simple Footnote. </div><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">4</a></sup>Sample Simple Footnote. </div><hr/>"));


            // Footnote Keyword
            SetUpTestCase(); // Reset content
            Assert.That(WrapTest("\\c 1 \\v 1 \\f - \\ft Sample Simple Footnote. \\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\"></a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\"></a></sup>Sample Simple Footnote. </div><hr/>"));
            SetUpTestCase(); // Reset content
            Assert.That(WrapTest("\\c 1 \\v 1 \\f abc \\ft Sample Simple Footnote. \\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">abc</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">abc</a></sup>Sample Simple Footnote. </div><hr/>"));

            // Footnote Keyword - Reference
            SetUpTestCase(); // Reset content
            Assert.That(WrapTest("\\c 1 \\v 1 \\f + \\fr 1.3 \\fk Tamar \\ft Menantu Yehuda yang akhirnya menjadi istrinya (bc. Kej. 38:1-30).\\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">1</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">1</a></sup><b> 1.3 </b> TAMAR: Menantu Yehuda yang akhirnya menjadi istrinya (bc. Kej. 38:1-30).</div><hr/>"));

            // Footnote Verse Marker - Paragraph
            SetUpTestCase(); // Reset content
            Assert.That(WrapTest("\\c 1 \\v 1 \\f + \\fr 9:55 \\ft Beberapa  \\fv 56 \\ft untuk menyelamatkan mereka.\\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">1</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">1</a></sup><b> 9:55 </b>Beberapa  <sup class=\"versemarker\">56</sup>untuk menyelamatkan mereka.</div><hr/>"));

        }
        [Test]
        public void TestCrossReferenceRender()
        {
            // Cross Reference Caller
            Assert.That(WrapTest("\\x - \\xo 11.21 \\xq Tebes \\xt \\x*"), Is.EqualTo("<sup class=\"caller\"></sup>"));

            // Cross Reference Origin
            Assert.That(WrapTest("\\c 1 \\v 1 \\x - \\xo 11.21 \\xq Tebes \\xt \\x*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup class=\"caller\"></sup></span></div><hr/><span class=\"cross-ref\"><sup class=\"caller\"></sup><b> 11.21 </b><span class=\"cross-ref-quote\">Tebes</span> </span><span class=\"cross-ref\"><sup class=\"caller\"></sup><b> 11.21 </b><span class=\"cross-ref-quote\">Tebes</span> </span>"));

            // Cross Reference Target
            Assert.That(WrapTest("\\c 1 \\v 1 \\x - \\xo 11.21 \\xq Tebes \\xt Mrk 1.24; Luk 2.39; Jhn 1.45.\\x*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup class=\"caller\"></sup></span></div><hr/><span class=\"cross-ref\"><sup class=\"caller\"></sup><b> 11.21 </b><span class=\"cross-ref-quote\">Tebes</span>Mrk 1.24; Luk 2.39; Jhn 1.45. </span>"));

            // Cross Reference Quotation
            Assert.That(WrapTest("\\c 1 \\v 1 \\x - \\xo 11.21 \\xq Tebes \\xt \\x*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup class=\"caller\"></sup></span></div><hr/><span class=\"cross-ref\"><sup class=\"caller\"></sup><b> 11.21 </b><span class=\"cross-ref-quote\">Tebes</span> </span>"));
        }
        [Test]
        public void TestVPRender()
        {
            Assert.That(WrapTest("\\v 1 \\vp 1a \\vp* This is not Scripture"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1a</sup> This is not Scripture</span>"));
            Assert.That(WrapTest("\\v 2 \\vp 2b \\vp* This is not Scripture"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">2b</sup> This is not Scripture</span>"));
        }
        [Test]
        public void TestWordEntryRender()
        {

            // Within Footnotes
            Assert.That(WrapTest("\\c 1 \\v 1\\f + \\fr 3:5 \\fk berhala \\ft Lih. \\w Berhala \\w* di Daftar Istilah.\\f*"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup><sup id=\"footnote-caller-1\" class=\"caller\"><a href=\"#footnote-target-1\">1</a></sup></span></div><hr/><div class=\"footnotes\"><sup id=\"footnote-target-1\" class=\"caller\"><a href=\"#footnote-caller-1\">1</a></sup><b> 3:5 </b> BERHALA: Lih. <span class=\"word-entry\"> Berhala </span> di Daftar Istilah.</div><hr/>"));

            // Word Entry Attributes
            Assert.That(WrapTest("\\v 1 \\w Berhala|Berhala \\w* di Daftar Istilah"), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup><span class=\"word-entry\"> Berhala </span> di Daftar Istilah</span>"));
            Assert.That(WrapTest("\\v 1 \\w gracious|lemma=\"grace\" \\w* di Daftar Istilah."), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup><span class=\"word-entry\"> gracious </span> di Daftar Istilah.</span>"));
            Assert.That(WrapTest("\\v 1 \\w gracious|lemma=\"grace\" strong=\"G5485\" \\w* di Daftar Istilah."), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup><span class=\"word-entry\"> gracious </span> di Daftar Istilah.</span>"));
            Assert.That(WrapTest("\\v 1 \\w gracious|strong=\"H1234,G5485\" \\w* di Daftar Istilah."), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup><span class=\"word-entry\"> gracious </span> di Daftar Istilah.</span>"));
            Assert.That(WrapTest("\\v 1 \\w gracious|lemma=\"grace\" srcloc=\"gnt5:51.1.2.1\" \\w* di Daftar Istilah."), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">1</sup><span class=\"word-entry\"> gracious </span> di Daftar Istilah.</span>"));

        }
        [Test]
        public void TestCharacterStylingRender()
        {
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\em Emphasis \\em* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"emphasis\">Emphasis</span></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\bd Boldness \\bd* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <b>Boldness</b></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\bdit Boldness and Italics \\bdit* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"bold-italic\">Boldness and Italics</span></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\it Italics \\it* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <i>Italics</i></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\sup Superscript \\sup* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"superscript-text\">Superscript</span></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\nd Name of Diety \\nd* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"deity-name\">Name of Diety</span></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\sc Small Caps \\sc* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"small-caps\">Small Caps</span></span>"));
            Assert.That(WrapTest("\\v 21 Penduduk kota yang satu akan pergi \\no Normal \\no* "), Is.EqualTo("<span class=\"verse\"><sup class=\"versemarker\">21</sup>Penduduk kota yang satu akan pergi <span class=\"normal-text\">Normal</span></span>"));


        }
        [Test]
        public void TestStyleRender()
        {
            render.ConfigurationHTML.divClasses.Add("two-columns");
            Assert.That(WrapTest("\\v 200 Genesis"), Is.EqualTo("<div class=\"two-columns\"><span class=\"verse\"><sup class=\"versemarker\">200</sup>Genesis</span></div>"));
            render.ConfigurationHTML.divClasses.Add("justified");
            Assert.That(WrapTest("\\v 200 Genesis"), Is.EqualTo("<div class=\"two-columns\"><div class=\"justified\"><span class=\"verse\"><sup class=\"versemarker\">200</sup>Genesis</span></div></div>"));

        }
        [Test]
        public void TestChapterBreak()
        {
            render.ConfigurationHTML.separateChapters = true;
            Assert.That(WrapTest("\\mt Genesis \\c 1 \\c 2 \\c 3"), Is.EqualTo("<div class=\"majortitle-1\">Genesis</div><div class=\"chapter\"><span class=\"chaptermarker\">1</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"chapter\"><span class=\"chaptermarker\">2</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"chapter\"><span class=\"chaptermarker\">3</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div>"));
            Assert.That(WrapTest("\\mt Genesis \\c 1 \\c 2 \\mt Exodus \\c 1"), Is.EqualTo("<div class=\"majortitle-1\">Genesis</div><div class=\"chapter\"><span class=\"chaptermarker\">1</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"chapter\"><span class=\"chaptermarker\">2</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"majortitle-1\">Exodus</div><div class=\"chapter\"><span class=\"chaptermarker\">1</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div>"));
            Assert.That(WrapTest("\\c 1 \\v 1 First \\c 2 \\v 1 Second \\c 3 \\v 1 Third"), Is.EqualTo("<div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup>First </span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"chapter\"><span class=\"chaptermarker\">2</span><span class=\"verse\"><sup class=\"versemarker\">1</sup>Second </span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div><div class=\"chapter\"><span class=\"chaptermarker\">3</span><span class=\"verse\"><sup class=\"versemarker\">1</sup>Third</span></div><br class=\"pagebreak\"></br><div class=\"pagebreak\"></div>"));

        }
        [Test]
        public void TestBlankColumn()
        {
            render.ConfigurationHTML.blankColumn = true;

            Assert.That(WrapTest("\\c 1 \\v 1 First \\v 2 Second \\v 3 Third"), Is.EqualTo("<table class=\"blank_col\"><tr><td><div class=\"chapter\"><span class=\"chaptermarker\">1</span><span class=\"verse\"><sup class=\"versemarker\">1</sup>First </span><span class=\"verse\"><sup class=\"versemarker\">2</sup>Second </span><span class=\"verse\"><sup class=\"versemarker\">3</sup>Third</span></div></td><td></td></tr></table>"));
        }
        [Test]
        public void TestUnknownMarkerRender()
        {

            Assert.That(WrapTest("\\yy"), Is.EqualTo(""));
            Assert.That(WrapTest("\\h 1 John \\test"), Is.EqualTo("<div class=\"header\">1 John</div>"));
            Assert.That(WrapTest("\\123 sdfgsgdfg"), Is.EqualTo(""));

        }
        [Test]
        public void TestAcrosticHeadingRender()
        {
            Assert.That(WrapTest("\\qa BETH"), Is.EqualTo("<span class=\"acrostic-heading\">BETH</span>"));
        }
        [Test]
        public void TestNestedPoetryRender()
        {
            // q1 is a poetry block
            var ascendingNested = 
                "\\q1 \\v 12 Verse twelve text lv1\n" +
                "\\q2 verse twelve text lv2" +
                "\\q3 verse twelve text lv3.";
            
            // q2 is a poetry block
            var descendingNested =
                "\\q2 \\v 12 Verse twelve text lv2\n" +
                "\\q1 verse twelve text lv1.\n";

            var nonNestedQMarkers= 
                "\\q1 \\v 12 Verse twelve text.\n" +
                "\\q2 \\v 13 Verse thirteen text.\n";

            var expectedFromAcendingNested = "<div class=\"poetry-1\"><span class=\"verse\"><sup class=\"versemarker\">12</sup>Verse twelve text lv1<div class=\"poetry-1\">verse twelve text lv2</div><div class=\"poetry-2\">verse twelve text lv3.</div></span></div>";
            var expectedFromDescendingNested = "<div class=\"poetry-2\"><span class=\"verse\"><sup class=\"versemarker\">12</sup>Verse twelve text lv2<div class=\"poetry-outdent-1\">verse twelve text lv1.</div></span></div>";
            var expectedFromNonNested = "<div class=\"poetry-1\"><span class=\"verse\"><sup class=\"versemarker\">12</sup>Verse twelve text.</span></div><div class=\"poetry-2\"><span class=\"verse\"><sup class=\"versemarker\">13</sup>Verse thirteen text.</span></div>";
            
            Assert.That(WrapTest(ascendingNested), Is.EqualTo(expectedFromAcendingNested));
            Assert.That(WrapTest(descendingNested), Is.EqualTo(expectedFromDescendingNested));
            Assert.That(WrapTest(nonNestedQMarkers), Is.EqualTo(expectedFromNonNested));
        }

        [Test]
        public void TestPIMarkers()
        {
            var usfm = "\\pi This is a indented paragraph";
            var expected = $"<p class=\"para-indent-1\">This is a indented paragraph</p>";
            Assert.That(WrapTest(usfm), Is.EqualTo(expected));
        }

        public string stripWhiteSpace(string input)
        {
            return input.Replace("\r", "").Replace("\n", "");
        }
        public string WrapTest(string usfm)
        {
            return stripWhiteSpace(render.Render(parser.ParseFromString(usfm)));
        }

    }
}

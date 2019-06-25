using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SCTVObjects
{
    class HTMLCodes
    {
        public readonly Dictionary<string, string> Code = new Dictionary<string, string>();
        public readonly Dictionary<string, string> NumericCode = new Dictionary<string, string>();
        //const string HTML_TAG_PATTERN = "<.*?>";
        const string HTML_TAG_PATTERN = "<(.|\n)*?>";
        
        public HTMLCodes()
        {
            Code.Add("&nbsp;", " ");
            Code.Add("&ndash;", "-");
            Code.Add("&lsquo;", "`");
            Code.Add("&rsquo;", "'");
            Code.Add("&sbquo;", ",");
            Code.Add("&quot;", "\"");
            Code.Add("&amp;", "&");
            Code.Add("&lt;", "<");
            Code.Add("&gt;", ">");
            Code.Add("&circ;", "^");
            Code.Add("&tilde;", "~");
            Code.Add("&Aacute;", "�");
            Code.Add("&aacute;", "�");
            Code.Add("&Acirc;", "�");
            Code.Add("&acirc;", "�");
            Code.Add("&Auml;", "�");
            Code.Add("&auml;", "�");
            Code.Add("&Agrave;", "�");
            Code.Add("&agrave;", "�");
            Code.Add("&Aring;", "�");
            Code.Add("&aring;", "�");
            Code.Add("&Atilde;", "�");
            Code.Add("&atilde;", "�");
            Code.Add("&Ccedil;", "�");
            Code.Add("&ccedil;", "�");
            Code.Add("&Eacute;", "�");
            Code.Add("&eacute;", "�");
            Code.Add("&Ecirc;", "�");
            Code.Add("&ecirc;", "�");
            Code.Add("&Euml;", "�");
            Code.Add("&euml;", "�");
            Code.Add("&Egrave;", "�");
            Code.Add("&egrave;", "�");
            Code.Add("&Iacute;", "�");
            Code.Add("&iacute;", "�");
            Code.Add("&Icirc;", "�");
            Code.Add("&icirc;", "�");
            Code.Add("&Iuml;", "�");
            Code.Add("&iuml;", "�");
            Code.Add("&Igrave;", "�");
            Code.Add("&igrave;", "�");
            Code.Add("&Ntilde;", "�");
            Code.Add("&ntilde;", "�");
            Code.Add("&Oacute;", "�");
            Code.Add("&oacute;", "�");
            Code.Add("&Ocirc;", "�");
            Code.Add("&ocirc;", "�");
            Code.Add("&Ouml;", "�");
            Code.Add("&ouml;", "�");
            Code.Add("&Ograve;", "�");
            Code.Add("&ograve;", "�");
            Code.Add("&Otilde;", "�");
            Code.Add("&otilde;", "�");
            Code.Add("&Uacute;", "�");
            Code.Add("&uacute;", "�");
            Code.Add("&Ucirc;", "�");
            Code.Add("&ucirc;", "�");
            Code.Add("&Uuml;", "�");
            Code.Add("&uuml;", "�");
            Code.Add("&Ugrave;", "�");
            Code.Add("&ugrave;", "�");
            Code.Add("&Yuml;", "�");
            Code.Add("&yuml;", "�");
            Code.Add("&larr;", "\\u27");

            NumericCode.Add("&#34;", "\"");
            NumericCode.Add("&#39;", "`");
            NumericCode.Add("&amp;#39;", "`");//this combination was getting missed
            NumericCode.Add("&#38;", "&");
            NumericCode.Add("&#60;", "less");
            NumericCode.Add("&#62;", "greater");
            NumericCode.Add("&#160;", " ");
            NumericCode.Add("&#161;", "!");
            NumericCode.Add("&#164;", "currency");
            NumericCode.Add("&#162;", "cent");
            NumericCode.Add("&#163;", "pound");
            NumericCode.Add("&#165;", "yen");
            NumericCode.Add("&#166;", "|");
            NumericCode.Add("&#167;", "section");
            NumericCode.Add("&#168;", "..");
            NumericCode.Add("&#169;", "(C)");
            NumericCode.Add("&#170;", "a");
            NumericCode.Add("&#171;", "``");
            NumericCode.Add("&#172;", "not");
            NumericCode.Add("&#173;", "-");
            NumericCode.Add("&#174;", "(R)");
            NumericCode.Add("&#8482;", "TM");
            NumericCode.Add("&#175;", "-");
            NumericCode.Add("&#176;", "o");
            NumericCode.Add("&#177;", "+/-");
            NumericCode.Add("&#178;", "^2");
            NumericCode.Add("&#179;", "^3");
            NumericCode.Add("&#180;", "`");
            NumericCode.Add("&#181;", "u");
            NumericCode.Add("&#182;", "P");
            NumericCode.Add("&#183;", ".");
            NumericCode.Add("&#184;", ",");
            NumericCode.Add("&#185;", "^1");
            NumericCode.Add("&#186;", "o");
            NumericCode.Add("&#187;", "``");
            NumericCode.Add("&#188;", "1/4");
            NumericCode.Add("&#189;", "1/2");
            NumericCode.Add("&#190;", "3/4");
            NumericCode.Add("&#191;", "?");
            NumericCode.Add("&#215;", "x");
            NumericCode.Add("&#247;", "/");
            NumericCode.Add("&#192;", "A");
            NumericCode.Add("&#193;", "A");
            NumericCode.Add("&#194;", "A");
            NumericCode.Add("&#195;", "A");
            NumericCode.Add("&#196;", "A");
            NumericCode.Add("&#197;", "A");
            NumericCode.Add("&#198;", "AE");
            NumericCode.Add("&#199;", "C");
            NumericCode.Add("&#200;", "E");
            NumericCode.Add("&#201;", "E");
            NumericCode.Add("&#202;", "E");
            NumericCode.Add("&#203;", "E");
            NumericCode.Add("&#204;", "I");
            NumericCode.Add("&#205;", "I");
            NumericCode.Add("&#206;", "I");
            NumericCode.Add("&#207;", "I");
            NumericCode.Add("&#208;", "D");
            NumericCode.Add("&#209;", "N");
            NumericCode.Add("&#210;", "O");
            NumericCode.Add("&#211;", "O");
            NumericCode.Add("&#212;", "O");
            NumericCode.Add("&#213;", "O");
            NumericCode.Add("&#214;", "O");
            NumericCode.Add("&#216;", "O");
            NumericCode.Add("&#217;", "U");
            NumericCode.Add("&#218;", "U");
            NumericCode.Add("&#219;", "U");
            NumericCode.Add("&#221;", "Y");
            NumericCode.Add("&#222;", "P");
            NumericCode.Add("&#223;", "ss");
            NumericCode.Add("&#224;", "a");
            NumericCode.Add("&#225;", "a");
            NumericCode.Add("&#226;", "a");
            NumericCode.Add("&#227;", "a");
            NumericCode.Add("&#228;", "a");
            NumericCode.Add("&#229;", "a");
            NumericCode.Add("&#230;", "ae");
            NumericCode.Add("&#231;", "c");
            NumericCode.Add("&#232;", "e");
            NumericCode.Add("&#233;", "e");
            NumericCode.Add("&#234;", "e");
            NumericCode.Add("&#235;", "e");
            NumericCode.Add("&#236;", "i");
            NumericCode.Add("&#237;", "i");
            NumericCode.Add("&#238;", "i");
            NumericCode.Add("&#239;", "i");
            NumericCode.Add("&#240;", "eth");
            NumericCode.Add("&#241;", "n");
            NumericCode.Add("&#242;", "o");
            NumericCode.Add("&#243;", "o");
            NumericCode.Add("&#244;", "o");
            NumericCode.Add("&#245;", "o");
            NumericCode.Add("&#246;", "o");
            NumericCode.Add("&#248;", "o");
            NumericCode.Add("&#249;", "u");
            NumericCode.Add("&#250;", "u");
            NumericCode.Add("&#251;", "u");
            NumericCode.Add("&#252;", "u");
            NumericCode.Add("&#253;", "y");
            NumericCode.Add("&#254;", "p");
            NumericCode.Add("&#255;", "y");
            NumericCode.Add("&#338;", "OE");
            NumericCode.Add("&#339;", "oe");
            NumericCode.Add("&#352;", "S");
            NumericCode.Add("&#353;", "s");
            NumericCode.Add("&#376;", "Y");
            NumericCode.Add("&#710;", "^");
            NumericCode.Add("&#732;", "~");
            NumericCode.Add("&#8194;", " ");
            NumericCode.Add("&#8195;", " ");
            NumericCode.Add("&#8201;", " ");
            NumericCode.Add("&#8204;", "|");
            NumericCode.Add("&#8205;", "|");
            NumericCode.Add("&#8206;", "|");
            NumericCode.Add("&#8207;", "|");
            NumericCode.Add("&#8211;", "-");
            NumericCode.Add("&#8212;", "-");
            NumericCode.Add("&#xB7;", "-");
            NumericCode.Add("&#8216;", "`");
            NumericCode.Add("&#8217;", "`");
            NumericCode.Add("&#8218;", ",");
            NumericCode.Add("&#8220;", "``");
            NumericCode.Add("&#8221;", "``");
            NumericCode.Add("&#8222;", "``");
            NumericCode.Add("&#8224;", "+");
            NumericCode.Add("&#8225;", "++");
            NumericCode.Add("&#8230;", "...");
            NumericCode.Add("&#8240;", "0/00");
            NumericCode.Add("&#8249;", "(");
            NumericCode.Add("&#8250;", ")");
            NumericCode.Add("&#8264;", "euro");
            NumericCode.Add("&#x22;", "\"");
            NumericCode.Add("#x22;", "\"");
            NumericCode.Add("&#xE8;", "e`");
            NumericCode.Add("&#x26;", "&");
            NumericCode.Add("&#x27;", "'");
        }

        static string StripHTML(string inputString)
        {
            try
            {
                //find first open tag
                int openTag = inputString.IndexOf("<");
                int closeTag = inputString.IndexOf(">");

                if (closeTag < openTag)
                    inputString = inputString.Substring(closeTag + 1);

                //remove quotes
                inputString = inputString.Replace("\"", "");

                inputString = inputString.Replace("\n", "");

                while (inputString.Contains("  "))
                    inputString = inputString.Replace("  ", " ");

                return Regex.Replace
                  (inputString, HTML_TAG_PATTERN, string.Empty);
            }
            catch (Exception ex)
            {
                Tools.WriteToFile(ex);

                return inputString;
            }
        }

        public string ToHTMLCode(string Text)
        {
            foreach (string currentCode in Code.Values)
            {

            }

            return Text;
        }

        public string ToHTMLNumericCode(string Text)
        {

            return Text;
        }

        public string ToText(string HtmlText)
        {
            try
            {
                foreach (string code in Code.Keys)
                {
                    HtmlText = HtmlText.Replace(code, Code[code]);
                }

                foreach (string numericCode in NumericCode.Keys)
                {
                    HtmlText = HtmlText.Replace(numericCode, NumericCode[numericCode]);
                }
            }
            catch (Exception ex)
            {
                Tools.WriteToFile(ex);
            }

            return StripHTML(HtmlText).Trim();
        }
    }
}

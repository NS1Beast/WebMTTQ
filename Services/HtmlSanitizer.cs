using System;
using System.Collections.Generic;
using System.Text;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Server-side allow-list HTML sanitizer for admin-authored rich text that is
    /// emitted through @Html.Raw(...). It is NOT a regex blacklist and it is NOT a
    /// full HTML5 parser: it tokenizes the input and REBUILDS output from scratch,
    /// emitting only an allow-list of safe tags and per-tag safe attributes, with all
    /// plain text and attribute values HTML-encoded. Therefore even malformed /
    /// adversarial input can never re-introduce a dangerous construct; at worst it
    /// causes content loss (availability), never script execution.
    ///
    /// Policy (minimal, per Phase 4 review decision):
    ///  - SVG, event handlers, form/object/embed/script and dangerous URL schemes
    ///    are dropped entirely.
    ///  - The 'style' attribute is NOT emitted (color/alignment are dropped).
    ///  - iframe/src is allowed ONLY for the video-embed feature with an explicit
    ///    https host allow-list (YouTube / Vimeo / Facebook embed).
    ///  - URL attributes accept only http/https (and data:image/* on &lt;img&gt; for
    ///    legacy base64 images); attribute values are entity-decoded before the
    ///    scheme check so 'jav&amp;colon;script:' cannot bypass.
    /// </summary>
    public static class HtmlSanitizer
    {
        private static readonly HashSet<string> AllowedTags = new HashSet<string>();
        private static readonly HashSet<string> ForbiddenContentTags = new HashSet<string>();
        private static readonly HashSet<string> GlobalAttrs = new HashSet<string>();
        private static readonly HashSet<string> UrlAttrs = new HashSet<string>();
        private static readonly HashSet<string> IframeHosts = new HashSet<string>();
        private static readonly Dictionary<string, HashSet<string>> TagAttrs = new Dictionary<string, HashSet<string>>();

        static HtmlSanitizer()
        {
            AllowedTags.Add("p"); AllowedTags.Add("br"); AllowedTags.Add("hr");
            AllowedTags.Add("b"); AllowedTags.Add("strong"); AllowedTags.Add("i");
            AllowedTags.Add("em"); AllowedTags.Add("u"); AllowedTags.Add("s");
            AllowedTags.Add("strike"); AllowedTags.Add("span"); AllowedTags.Add("div");
            AllowedTags.Add("blockquote"); AllowedTags.Add("ul"); AllowedTags.Add("ol");
            AllowedTags.Add("li"); AllowedTags.Add("h1"); AllowedTags.Add("h2");
            AllowedTags.Add("h3"); AllowedTags.Add("h4"); AllowedTags.Add("h5");
            AllowedTags.Add("h6"); AllowedTags.Add("figure"); AllowedTags.Add("figcaption");
            AllowedTags.Add("a"); AllowedTags.Add("img"); AllowedTags.Add("table");
            AllowedTags.Add("thead"); AllowedTags.Add("tbody"); AllowedTags.Add("tfoot");
            AllowedTags.Add("tr"); AllowedTags.Add("th"); AllowedTags.Add("td");
            AllowedTags.Add("colgroup"); AllowedTags.Add("col"); AllowedTags.Add("video");
            AllowedTags.Add("source"); AllowedTags.Add("iframe"); AllowedTags.Add("pre");

            ForbiddenContentTags.Add("script"); ForbiddenContentTags.Add("style");
            ForbiddenContentTags.Add("meta"); ForbiddenContentTags.Add("link");
            ForbiddenContentTags.Add("base"); ForbiddenContentTags.Add("title");
            ForbiddenContentTags.Add("object"); ForbiddenContentTags.Add("embed");
            ForbiddenContentTags.Add("applet"); ForbiddenContentTags.Add("param");
            ForbiddenContentTags.Add("form"); ForbiddenContentTags.Add("input");
            ForbiddenContentTags.Add("textarea"); ForbiddenContentTags.Add("select");
            ForbiddenContentTags.Add("option"); ForbiddenContentTags.Add("button");
            ForbiddenContentTags.Add("label"); ForbiddenContentTags.Add("svg");
            ForbiddenContentTags.Add("math"); ForbiddenContentTags.Add("audio");
            ForbiddenContentTags.Add("template"); ForbiddenContentTags.Add("noscript");
            ForbiddenContentTags.Add("xmp"); ForbiddenContentTags.Add("plaintext");

            // Only 'class' is a harmless cosmetic attribute the editor actually emits.
            GlobalAttrs.Add("class");

            UrlAttrs.Add("href"); UrlAttrs.Add("src");
            UrlAttrs.Add("cite"); UrlAttrs.Add("poster");
            UrlAttrs.Add("action"); UrlAttrs.Add("longdesc");

            // Video-embed feature: only the hosts the editor's oEmbed builder produces.
            IframeHosts.Add("youtube.com");
            IframeHosts.Add("www.youtube.com");
            IframeHosts.Add("youtu.be");
            IframeHosts.Add("youtube-nocookie.com");
            IframeHosts.Add("vimeo.com");
            IframeHosts.Add("player.vimeo.com");
            IframeHosts.Add("facebook.com");
            IframeHosts.Add("www.facebook.com");
            IframeHosts.Add("facebook.net");
            IframeHosts.Add("www.facebook.net");
void AddAttrs(string tag, params string[] allowed)
            {
                if (!TagAttrs.ContainsKey(tag))
                    TagAttrs[tag] = new HashSet<string>();
                foreach (var a in allowed) TagAttrs[tag].Add(a);
            }

            AddAttrs("a", "href", "target", "rel", "title", "name", "hreflang", "download");
            AddAttrs("img", "src", "alt", "width", "height");
            AddAttrs("table", "border", "width", "align", "cellpadding", "cellspacing");
            AddAttrs("tr", "align", "valign");
            AddAttrs("th", "colspan", "rowspan", "width", "height", "align", "valign", "scope");
            AddAttrs("td", "colspan", "rowspan", "width", "height", "align", "valign");
            AddAttrs("col", "span", "width");
            AddAttrs("colgroup", "span", "width");
            AddAttrs("ul", "type", "start", "reversed", "compact");
            AddAttrs("ol", "type", "start", "reversed", "compact");
            AddAttrs("li", "value", "type");
            AddAttrs("blockquote", "cite");
            AddAttrs("video", "controls", "width", "height");
            AddAttrs("source", "src", "type", "media");
            AddAttrs("iframe", "src", "width", "height", "allow", "allowfullscreen", "frameborder", "scrolling", "title");
    }
/// <summary>
        /// Sanitize untrusted/admin-authored HTML to the safe allow-listed subset.
        /// Sanitizing already-sanitized value is idempotent (no double change).
        /// </summary>
        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return SanitizeInternal(html);
        }

        /// <summary>
        /// Escape a JSON string so it is safe to inline inside a
        /// &lt;script type="application/json"&gt; block while remaining valid JSON
        /// (these are legal JSON string escapes). Used for the map-data sink.
        /// </summary>
        public static string JsonScriptSafe(string json)
        {
            if (string.IsNullOrEmpty(json)) return "[]";
            var sb = new StringBuilder(json.Length + 8);
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                switch (c)
                {
                    case '<': sb.Append("\\u003c"); break;
                    case '>': sb.Append("\\u003e"); break;
                    case '&': sb.Append("\\u0026"); break;
                    case '\u2028': sb.Append("\\u2028"); break;
                    case '\u2029': sb.Append("\\u2029"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private static string SanitizeInternal(string input)
        {
            var sb = new StringBuilder(input.Length + 64);
            int n = input.Length;
            int i = 0;
            string? skipToClose = null;

            while (i < n)
            {
                // When skipping a forbidden element's subtree, ignore all content
                // (including text) until its close tag is found.
                if (skipToClose != null)
                {
                    int close = IndexOfIgnoreCase(input, "</" + skipToClose, i);
                    if (close < 0) break;
                    i = close + skipToClose.Length + 3; // position after "</name>"
                    skipToClose = null;
                    continue;
                }

                char c = input[i];

                if (c != '<')
                {
                    sb.Append(EscapeText(input[i]));
                    i++;
                    continue;
                }

                // HTML comment: drop entirely.
                if (i + 3 < n && input.Substring(i, 4) == "<!--")
                {
                    int end = input.IndexOf("-->", i + 4);
                    i = (end < 0) ? n : end + 3;
                    continue;
                }

                // Doctype / declaration / processing instruction: drop entirely.
                if (i + 1 < n && (input[i + 1] == '!' || input[i + 1] == '?'))
                {
                    int declEnd = FindTagEnd(input, i + 1);
                    i = (declEnd < 0) ? n : declEnd + 1;
                    continue;
                }

                int gt = FindTagEnd(input, i + 1);
                if (gt < 0)
                {
                    // Unclosed '<' -> literal text.
                    sb.Append("&lt;");
                    i++;
                    continue;
                }

                string tagText = input.Substring(i + 1, gt - (i + 1));
                i = gt + 1;

                bool isClosing = tagText.StartsWith("/");
                string body = isClosing ? tagText.Substring(1) : tagText;
                int nameEnd = 0;
                while (nameEnd < body.Length && !IsTagNameBreak(body[nameEnd])) nameEnd++;
                string tagName = body.Substring(0, nameEnd).ToLowerInvariant();

                if (tagName.Length == 0)
                {
                    sb.Append("&lt;");
                    continue;
                }

                if (isClosing)
                {
                    if (AllowedTags.Contains(tagName))
                        sb.Append("</").Append(tagName).Append('>');
                    continue;
                }

                if (ForbiddenContentTags.Contains(tagName))
                {
                    skipToClose = tagName;
                    continue;
                }

                if (!AllowedTags.Contains(tagName))
                {
                    // Drop tag itself; its text content continues to be encoded
                    // normally (no executable markup).
                    continue;
                }

                sb.Append('<').Append(tagName);
                EmitAllowedAttributes(sb, body, nameEnd, tagName);
                sb.Append('>');
            }

            return sb.ToString();
        }
/// <summary>
        /// Emits only per-tag allow-listed attributes (plus the globally allowed
        /// 'class'), performing URL-scheme validation. Event handlers and 'style'
        /// are never emitted.
        /// </summary>
        private static void EmitAllowedAttributes(StringBuilder sb, string body, int from, string tagName)
        {
            int len = body.Length;
            int p = from;
            HashSet<string>? allowed;
            TagAttrs.TryGetValue(tagName, out allowed);

            while (p < len)
            {
                while (p < len && IsSpace(body[p])) p++;
                if (p >= len) break;
                if (body[p] == '>') break;
                if (body[p] == '/' && p + 1 < len && body[p + 1] == '>') break;

                int aStart = p;
                while (p < len && !IsSpace(body[p]) && body[p] != '=' && body[p] != '>'
                       && !(body[p] == '/' && p + 1 < len && body[p + 1] == '>'))
                    p++;
                if (p == aStart) { p++; continue; }

                string attrName = body.Substring(aStart, p - aStart).ToLowerInvariant();
                while (p < len && IsSpace(body[p])) p++;

                string attrValue = "";
                if (p < len && body[p] == '=')
                {
                    p++;
                    while (p < len && IsSpace(body[p])) p++;
                    if (p < len && (body[p] == '"' || body[p] == '\''))
                    {
                        char q = body[p];
                        int start = p + 1;
                        int end = body.IndexOf(q, start);
                        if (end < 0) end = len;
                        attrValue = body.Substring(start, end - start);
                        p = end + 1;
                    }
                    else if (p < len && body[p] != '>')
                    {
                        int vs = p;
                        while (p < len && !IsSpace(body[p]) && body[p] != '>') p++;
                        attrValue = body.Substring(vs, p - vs);
                    }
                }

                // Never emit event handlers.
                if (attrName.StartsWith("on"))
                    continue;

                // 'style' is intentionally dropped (removes CSS attack surface).
                if (attrName == "style")
                    continue;

                bool allowedForAttr = GlobalAttrs.Contains(attrName)
                    || (allowed != null && allowed.Contains(attrName));
                if (!allowedForAttr) continue;

                string? finalValue = attrValue;

                if (UrlAttrs.Contains(attrName))
                {
                    if (tagName == "iframe" && attrName == "src")
                    {
                        if (!IsSafeIframeSrc(attrValue)) continue;
                    }
                    else
                    {
                        bool isImg = tagName == "img";
                        var safe = SafeUrl(attrValue, isImg);
                        if (safe == null) continue;
                        finalValue = safe;
                    }
                }

                if (string.IsNullOrEmpty(finalValue) && !UrlAttrs.Contains(attrName))
                {
                    // Boolean HTML attributes: emit the bare attribute name.
                    if (attrName == "controls" || attrName == "allowfullscreen"
                        || attrName == "reversed" || attrName == "download"
                        || attrName == "compact")
                        sb.Append(' ').Append(attrName);
                    continue;
                }

                if (attrName == "target" && EqualsIgnoreCase(attrValue, "_blank"))
                {
                    // New-window links always get noopener/noreferrer.
                    sb.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
                    continue;
                }

                sb.Append(' ').Append(attrName).Append("=\"");
                sb.Append(EncodeAttr(finalValue ?? ""));
                sb.Append('"');
            }
        }
/// <summary>
        /// Validates a URL attribute value. Attribute values are entity-decoded
        /// first so 'java&amp;colon;script:' cannot defeat the scheme allow-list.
        /// Returns the value to emit, or null to drop the attribute.
        /// </summary>
        private static string? SafeUrl(string? value, bool imageSrc)
        {
            if (value == null) return null;
            string s = value.Trim();

            if (s.Length == 0) return null;
            if (s.StartsWith("//")) return null; // block protocol-relative (no host allow-list)
            if (s[0] == '#') return s;
            if (s.StartsWith("/") || s.StartsWith("./") || s.StartsWith("../")) return s;

            string decoded = DecodeEntities(s).Trim();
            if (decoded.Length == 0) return null;

            int colon = decoded.IndexOf(':');
            if (colon < 0)
            {
                // No scheme: a bare relative path is acceptable.
                return decoded;
            }

            string scheme = decoded.Substring(0, colon).Trim().ToLowerInvariant();

            // Scheme must be a clean alphabetic token; reject control-char smuggles.
            if (scheme.Length < 2 || scheme.Length > 16) return null;
            for (int k = 0; k < scheme.Length; k++)
            {
                char cc = scheme[k];
                if (cc < 'a' || cc > 'z') return null;
            }

            if (scheme == "http" || scheme == "https") return decoded;

            // data: allowed only for raster image data on <img> (legacy base64);
            // data:text/html and data:image/svg+xml are always rejected.
            if (imageSrc && scheme == "data")
            {
                string rest = decoded.Substring(colon + 1).Trim();
                if (rest.Length > 4096) return null;
                string lr = rest.ToLowerInvariant();
                if (lr.StartsWith("image/png") || lr.StartsWith("image/jpeg")
                    || lr.StartsWith("image/gif") || lr.StartsWith("image/webp")
                    || lr.StartsWith("image/bmp") || lr.StartsWith("image/x-icon"))
                    return decoded;
                return null;
            }

            return null;
        }

        /// <summary>
        /// iframe src is allowed only over https to an explicit video-embed host.
        /// </summary>
        private static bool IsSafeIframeSrc(string? value)
        {
            if (value == null) return false;
            var trimmed = DecodeEntities(value).Trim().ToLowerInvariant();
            if (!trimmed.StartsWith("https://")) return false;
            string rest = trimmed.Substring("https://".Length);
            int slash = rest.IndexOf('/');
            string host = (slash < 0) ? rest : rest.Substring(0, slash);
            return IframeHosts.Contains(host);
        }
/// <summary>Finds the '>' terminating a tag, honoring quoted attribute values.</summary>
        private static int FindTagEnd(string s, int from)
        {
            bool inDq = false, inSq = false;
            for (int j = from; j < s.Length; j++)
            {
                char c = s[j];
                if (inDq) { if (c == '"') inDq = false; continue; }
                if (inSq) { if (c == '\'') inSq = false; continue; }
                if (c == '"') inDq = true;
                else if (c == '\'') inSq = true;
                else if (c == '>') return j;
            }
            return -1;
        }

        private static bool IsTagNameBreak(char c)
        {
            return IsSpace(c) || c == '>' || c == '/';
        }

        private static bool IsSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';
        }

        private static int IndexOfIgnoreCase(string haystack, string needle, int from)
        {
            return haystack.ToLowerInvariant().IndexOf(needle.ToLowerInvariant(), from);
        }

        private static bool EqualsIgnoreCase(string? a, string? b)
        {
            return a != null && b != null && a.ToLowerInvariant() == b.ToLowerInvariant();
        }

        private static string EscapeText(char c)
        {
            switch (c)
            {
                case '&': return "&amp;";
                case '<': return "&lt;";
                case '>': return "&gt;";
                default: return c.ToString();
            }
        }

        private static string EncodeAttr(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s.ToCharArray())
            {
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
/// <summary>
        /// Decodes a bounded set of common/numeric HTML entities so an attacker
        /// cannot smuggle a dangerous scheme past the URL check.
        /// </summary>
        private static string DecodeEntities(string? s)
        {
            if (s == null) return "";
            if (s.IndexOf('&') < 0) return s;
            var sb = new StringBuilder(s.Length);
            int i = 0;
            int n = s.Length;
            while (i < n)
            {
                char c = s[i];
                if (c != '&' || i + 1 >= n) { sb.Append(c); i++; continue; }
                int semi = s.IndexOf(';', i);
                if (semi < 0 || semi - i > 12) { sb.Append(c); i++; continue; }
                string token = s.Substring(i + 1, semi - (i + 1));
                string lower = token.ToLowerInvariant();
                string? decoded = null;

                if (token.Length > 1 && token[0] == '#')
                {
                    int code = -1;
                    try
                    {
                        if (token.Length > 1 && (token[1] == 'x' || token[1] == 'X'))
                            code = ParseInt(token.Substring(2), 16);
                        else
                            code = ParseInt(token.Substring(1), 10);
                    }
                    catch (Exception) { code = -1; }
                    if (code > 0 && (code < 0xD800 || code > 0xDFFF))
                        decoded = char.ConvertFromUtf32(code);
                }
                else if (lower == "amp") decoded = "&";
                else if (lower == "lt") decoded = "<";
                else if (lower == "gt") decoded = ">";
                else if (lower == "quot") decoded = "\"";
                else if (lower == "apos") decoded = "'";
                else if (lower == "nbsp") decoded = "\u00a0";
                else if (lower == "colon") decoded = ":";
                else if (lower == "sol") decoded = "/";
                else if (lower == "Tab") decoded = "\t";
                else if (lower == "NewLine") decoded = "\n";

                if (decoded != null) { sb.Append(decoded); i = semi + 1; }
                else { sb.Append(c); i++; }
            }
            return sb.ToString();
        }

        private static int ParseInt(string digits, int radix)
        {
            if (digits.Length == 0) return -1;
            int value = 0;
            foreach (char ch in digits.ToCharArray())
            {
                int d = HexValue(ch);
                if (d < 0 || d >= radix) return -1;
                value = value * radix + d;
            }
            return value;
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            if (c >= 'A' && c <= 'F') return 10 + (c - 'A');
            return -1;
        }
    }
        }
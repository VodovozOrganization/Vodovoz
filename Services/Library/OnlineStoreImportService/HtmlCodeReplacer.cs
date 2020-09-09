using System.Collections.Generic;
using System.Text;

namespace OnlineStoreImportService
{
    public class HtmlCodeReplacer
    {
        Dictionary<string, string> codesDictionary = new Dictionary<string, string>();

        public HtmlCodeReplacer()
        {
            codesDictionary.Add("quot;", "\\u0022");
            codesDictionary.Add("amp;", "\\u0026");
        }
        
        public void ReplaceCodes(StringBuilder source)
        {
            foreach (var pair in codesDictionary) {
                source.Replace($"&{pair.Key}", pair.Value);
                source.Replace(pair.Key, pair.Value);
            }
        }
    }
}
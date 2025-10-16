using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PurchaseTransaction.Utils
{
    public static class CountryCurrencyDescConverter
    {
        public static string Normalize(string countryCurrencyDesc)
        {
            if(string.IsNullOrWhiteSpace(countryCurrencyDesc)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(countryCurrencyDesc));
            }

            //Trim and normalize dash variants to "-"
            var normalized = countryCurrencyDesc.Trim();
            normalized = Regex.Replace(normalized, "[‐‑‒–—―]", "-");
            //Remove space around dash
            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = Regex.Replace(normalized, @"\s*-\s*", "-");

            var parts = normalized.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if(parts.Length != 2) {
                return ToSmartTitleCase(normalized);
            }

            var country = ToSmartTitleCase(parts[0]);
            var currency = ToSmartTitleCase(parts[1]);

            return $"{country}-{currency}";
        }

        //Title-case while preserving things liks "&", "U.S." etc.
        private static string ToSmartTitleCase(string normalized)
        {
            var textInfo = CultureInfo.InvariantCulture.TextInfo;

            var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < tokens.Length; i++) {
                var token = tokens[i];

                if (token == "&") { /*Keep*/}
                else if(IsDottedAcronym(token))
                {
                    token = ToUpperLettersKeepDots(token);
                }
                else
                {
                    token = ToTitleWithApostrophesPeriods(token, textInfo);
                }

                tokens[i] = token;
            }
            return string.Join(' ', tokens);
        }

        private static string ToTitleWithApostrophesPeriods(string token, TextInfo textInfo)
        {
            var apostropheParts = token.Split('\'');
            for(int j = 0; j < apostropheParts.Length; j++) {
                var part = apostropheParts[j];

                if(IsDottedAcronym(part)) part = ToUpperLettersKeepDots(part);
                else part = textInfo.ToTitleCase(part.ToLowerInvariant());
                apostropheParts[j] = part;
            }
            return string.Join('\'', apostropheParts);
        }

        private static string ToUpperLettersKeepDots(string token)
        {
            var sb = new StringBuilder(token.Length);
            foreach(var ch in token)
                sb.Append(char.IsLetter(ch) ? char.ToUpperInvariant(ch) : ch);
            return sb.ToString();
        }

        private static bool IsDottedAcronym(string token)
        {
            //e.g, "u.s.", "e."
            if(!token.Contains('.')) return false;  
            foreach(var ch in token) {
                if(!char.IsLetter(ch) && ch != '.') return false;
            }
            return true;
        }
    }
}

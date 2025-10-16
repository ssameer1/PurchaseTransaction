using PurchaseTransaction.Utils;

namespace PurchaseTransaction.Test
{
    public class CountryCurrencyDescConverterTest
    {
        [TestCase("canada-dollar", "Canada-Dollar")]
        [TestCase("south africa-rand", "South Africa-Rand")]
        [TestCase("cayman islands-dollar", "Cayman Islands-Dollar")]
        [TestCase("singapore-dollar", "Singapore-Dollar")]
        [TestCase("nepal-rupee", "Nepal-Rupee")]
        public void Normalize_BasicTitleCase_Works(string input, string expected)
        {
            var actual = CountryCurrencyDescConverter.Normalize(input);
            
            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.EqualTo(expected));
                AssertHasSingleHyphenNoSpaces(actual);
            });
        }

        [TestCase("antigua & barbuda-e. caribbean dollar", "Antigua & Barbuda-E. Caribbean Dollar")]
        [TestCase("central african republic-cfa franc", "Central African Republic-Cfa Franc")]
        [TestCase("marshall islands-u.s. dollar", "Marshall Islands-U.S. Dollar")]
        public void Normalize_PunctuationAndDottedAcronmys_Preserved(string input, string expected)
        {
            var actual = CountryCurrencyDescConverter.Normalize(input);
            
            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.EqualTo(expected));
                AssertHasSingleHyphenNoSpaces(actual);
            });
        }

        [TestCase("cote d'ivoire-cfa franc", "Cote D'Ivoire-Cfa Franc")]
        public void Normalize_Apostrophes_TitleCasedPreWord(string input, string expected)
        {
            var actual = CountryCurrencyDescConverter.Normalize(input);
            
            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.EqualTo(expected));
                AssertHasSingleHyphenNoSpaces(actual);
            });
        }

        private void AssertHasSingleHyphenNoSpaces(string actual)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual.Count(c => c == '-'), Is.EqualTo(1),"Should contain exactly one hyphen");
                Assert.That(actual, Does.Not.Contain("- "));
                Assert.That(actual, Does.Not.Contain(" -"));
                Assert.That(actual, Does.Not.Contain(" - "));
            });
        }
    }
}

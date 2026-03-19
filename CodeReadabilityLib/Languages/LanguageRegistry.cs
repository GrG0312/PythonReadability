using CodeReadabilityLib.Languages.LanguageContexts;

namespace CodeReadabilityLib.Languages
{
    public static class LanguageRegistry
    {
        public static IReadOnlyCollection<SupportedLanguage> SupportedLanguages { get; } = new List<SupportedLanguage>
        {
            new SupportedLanguage
            {
                Language = ProgLang.CSharp,
                LangNames = ["c#", "csharp"],
                FileExtensions = [".cs"],
                Context = new LanguageContext
                {
                    UsesSignificantIndent = false,
                    SingleLineCommentPrefixes = new HashSet<string> { "//" },
                    MultiLineCommentDelimiters = new HashSet<(string, string)> { ("/*", "*/") },
                    BlockOpeningTokens = new HashSet<char> { '{' },
                    BlockClosingTokens = new HashSet<char> { '}' },
                    CyclomaticKeywords = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "if", "for", "while", "case", "catch"
                    },
                    Operators = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "+","-","*","/","%","=","==","!=","<",">","<=",">=",
                        "&&","||","!","??","?.","=>","+=","-=","*=","/="
                    }
                }
            },
            new SupportedLanguage
            {
                Language = ProgLang.Python,
                LangNames = ["python"],
                FileExtensions = [".py"],
                Context = new LanguageContext
                {
                    UsesSignificantIndent = true,
                    SingleLineCommentPrefixes = new HashSet<string> { "#" },
                    MultiLineCommentDelimiters = new HashSet<(string, string)>
                    {
                        ("\"\"\"", "\"\"\""),
                        ("'''", "'''")
                    },
                    BlockOpeningTokens = new HashSet<char>(),
                    BlockClosingTokens = new HashSet<char>(),
                    CyclomaticKeywords = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "if", "elif", "for", "while", "except", "case"
                    },
                    Operators = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "+","-","*","/","%","=","==","!=","<",">","<=",">=",
                        "and","or","not","in","is","+=","-=","*=","/="
                    }
                }
            }
        };

        public static SupportedLanguage FromString(string langName)
        {
            SupportedLanguage? language = SupportedLanguages.FirstOrDefault(lang => lang.LangNames.Contains(langName, StringComparer.OrdinalIgnoreCase));

            if (language is null) throw new LanguageNotFoundException($"Argument {langName} is not a valid language name or is not supported!");

            return language;
        }
    }
}

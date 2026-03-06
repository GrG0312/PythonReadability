namespace CodeReadabilityLib.Metrics
{
    /// <summary>
    /// Central registry of all available readability metrics.
    /// Add new metrics here or in new language-specific nested classes.
    /// </summary>
    public static class MetricRegistry
    {
        public static readonly IReadOnlyCollection<MetricDefinition> Algorithmic = [
            #region Formatting & Structure Metrics
            // ======================================================== //
            //             FORMATTING & STRUCTURE METRICS               //
            // ======================================================== //
            new MetricDefinition{
                Id = "indentation_consistency",
                Name = "Indentation Consistency",
                Description = "Measures how consistent the indentation is across the codebase. A higher score indicates more consistent indentation, which can improve readability.",
                ApplicableLanguages = [SupportedLanguage.CSharp], // In Python indentation is part of the syntax, so this metric is not applicable.
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "average_line_length",
                Name = "Average Line Length",
                Description = "Calculates the average number of characters per line of code. Shorter lines are generally easier to read, so a lower score is better.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false
            },
            new MetricDefinition{
                Id = "whitespace_usage",
                Name = "Whitespace Usage",
                Description = "Evaluates the use of whitespace in the code, including spaces around operators and after commas. Proper use of whitespace can enhance readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "bracing_style_consistency",
                Name = "Bracing Style Consistency",
                Description = "Measures how consistently braces are used in the code. Consistent bracing style can improve readability.",
                ApplicableLanguages = [SupportedLanguage.CSharp], // In Python, code blocks are defined by indentation rather than braces, so this metric is not applicable.
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "nesting_depth",
                Name = "Nesting Depth",
                Description = "Measures the maximum depth of nested code blocks. Deeply nested code can be harder to read and understand.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false,
            },
            #endregion
            #region Complexity Metrics
            // ======================================================== //
            //                   COMPLEXITY METRICS                     //
            // ======================================================== //
            new MetricDefinition{
                Id = "cyclomatic_complexity",
                Name = "Cyclomatic Complexity",
                Description = "Measures the number of linearly independent paths through the code. Higher complexity can indicate code that is harder to understand and maintain.",
                ApplicableLanguages = [],
                MinValue = 1,
                HigherIsBetter = false
            },
            new MetricDefinition{
                Id = "halstead_volume",
                Name = "Halstead Volume",
                Description = "Calculates the Halstead Volume, which is based on the number of operators and operands in the code. Higher volume can indicate more complex code.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false
            },
            new MetricDefinition{
                Id = "lines_of_code",
                Name = "Lines of Code",
                Description = "Counts the total number of lines of code. While not a direct measure of readability, excessively long files can be harder to navigate and understand.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false
            },
            #endregion
            #region Documentation Metrics
            // ======================================================== //
            //                 DOCUMENTATION METRICS                    //
            // ======================================================== //
            new MetricDefinition{
                Id = "comment_density",
                Name = "Comment Density",
                Description = "Calculates the ratio of comment lines to total lines of code. A higher score suggests better documentation and can enhance readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "docstring_density",
                Name = "Docstring Density",
                Description = "Calculates the ratio of docstring lines to total lines of code. A higher score suggests better documentation and can enhance readability.",
                ApplicableLanguages = [SupportedLanguage.Python, SupportedLanguage.CSharp],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
            #region Naming Metrics
            // ======================================================== //
            //                     NAMING METRICS                       //
            // ======================================================== //
            new MetricDefinition{
                Id = "naming_convention_adherence",
                Name = "Naming Convention Adherence",
                Description = "Evaluates how well the code adheres to common naming conventions (e.g., camelCase for variables, PascalCase for classes). Consistent naming can improve readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "identifier_length",
                Name = "Identifier Length",
                Description = "Measures the average length of identifiers (variable names, function names, etc.). While not a direct measure of readability, excessively long or short identifiers can impact understandability.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false
            },
            new MetricDefinition{
                Id = "keyword_usage_as_names",
                Name = "Keyword Usage as Names",
                Description = "Counts the number of times language keywords are used as identifiers (e.g., variable names). Using keywords as names can lead to confusion and reduce readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                HigherIsBetter = false
            }
            #endregion
        ];

        public static readonly IReadOnlyCollection<MetricDefinition> LlmBased = [
            #region Naming Quality Metrics
            // ======================================================== //
            //                 NAMING QUALITY METRICS                   //
            // ======================================================== //
            new MetricDefinition{
                Id = "appropriateness_of_names",
                Name = "Appropriateness of Names",
                Description = "Evaluates how well the names of variables, functions, and classes reflect their purpose and usage. Appropriate naming can significantly enhance readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "consistency_of_naming",
                Name = "Consistency of Naming",
                Description = "Assesses how consistently naming conventions are applied throughout the codebase. Consistent naming can improve readability and reduce cognitive load.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "pronounceability_of_names",
                Name = "Pronounceability of Names",
                Description = "Evaluates how easily the names of variables, functions, and classes can be pronounced. Easily pronounceable names can improve readability and communication among developers.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
            #region Comment Quality Metrics
            new MetricDefinition{
                Id = "clarity_of_comments",
                Name = "Clarity of Comments",
                Description = "Assesses how clear and informative the comments in the code are. Clear comments can help explain complex logic and improve readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "usefulness_of_comments",
                Name = "Usefulness of Comments",
                Description = "Evaluates how useful the comments are in understanding the code. Useful comments can provide context and explanations that enhance readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "consistency_of_comments_and_code",
                Name = "Consistency of Comments and Code",
                Description = "Assesses how well the comments align with the actual code. Inconsistent comments can lead to confusion and reduce readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
            #region Complexity & Understandability Metrics
            new MetricDefinition{
                Id = "high_level_understandability",
                Name = "High-Level Understandability",
                Description = "Evaluates how easily a developer can understand the overall structure and purpose of the code. High-level understandability can improve readability and ease of navigation.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            new MetricDefinition{
                Id = "cognitive_load",
                Name = "Cognitive Load",
                Description = "Assesses the mental effort required to understand the code. Convoluted or tricky logic can increase cognitive load and reduce readability.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = false
            },
            new MetricDefinition{
                Id = "use_of_idioms",
                Name = "Use of Idioms",
                Description = "Evaluates how well the code uses common language idioms and patterns. Proper use of idioms can enhance readability by leveraging familiar constructs.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
            #region Error Handling & Validation
            new MetricDefinition{
                Id = "error_handling_clarity",
                Name = "Error Handling Clarity",
                Description = "Assesses how clearly the code handles errors and exceptions. Clear error handling can improve readability by making it easier to understand how the code responds to unexpected conditions.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
            #region Usage of Project-Specific Conventions
            new MetricDefinition{
                Id = "adherence_to_project_conventions",
                Name = "Adherence to Project Conventions",
                Description = "Evaluates how well the code follows any specific conventions or guidelines established for similar projects. Adherence to project conventions can enhance readability by providing a consistent style and structure.",
                ApplicableLanguages = [],
                MinValue = 0,
                MaxValue = 100,
                HigherIsBetter = true
            },
            #endregion
        ];

        #region Query Methods
        public static IEnumerable<MetricDefinition> GetAllMetrics()
        {
            return Algorithmic.Concat(LlmBased);
        }

        public static IEnumerable<MetricDefinition> GetMetricsForLanguage(SupportedLanguage language)
        {
            return GetAllMetrics().Where(metric => metric.AppliesTo(language));
        }

        public static IEnumerable<MetricDefinition> GetAlgorithmicForLanguage(SupportedLanguage language)
        {
            return Algorithmic.Where(metric => metric.AppliesTo(language));
        }

        public static IEnumerable<MetricDefinition> GetLlmBasedForLanguage(SupportedLanguage language)
        {
            return LlmBased.Where(metric => metric.AppliesTo(language));
        }

        public static MetricDefinition? GetMetricById(string id)
        {
            return GetAllMetrics().FirstOrDefault(metric => metric.Id == id);
        }
        #endregion
    }
}

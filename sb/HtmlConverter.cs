using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Collections.Generic;

class HtmlConverterProgram
{
    static void Main(string[] args)
    {
        string inputPath = "input.md"; // Path to the input Markdown file
        string outputPath = "output.html";
        
        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }
        
        string markdownContent = File.ReadAllText(inputPath);
        string htmlContent = ConvertMarkdownToHtml(markdownContent);
        
        File.WriteAllText(outputPath, htmlContent);
        Console.WriteLine($"HTML content has been saved to {outputPath}");
    }

    static string ConvertMarkdownToHtml(string markdown)
    {
        try
        {
            StringBuilder html = new StringBuilder();

            // Now Splitting the files when this character is encountered \n. Now lines are divided into array of strings
            string[] lines = markdown.Split(new[] { '\n' });

            bool notInCodingBlock = true;
            bool inUnorderedList = false;
            bool inOrderedList = false;

            StringBuilder concatenatedLine = new StringBuilder();

            foreach (string originalline in lines)
            {
                string line = originalline;
                // Now trimming extra white spaces and tabs.
                string trimmedLine = line.Trim(); // This will trim at the start and at the end too.

                // Handle line continuation with backslash
                if (trimmedLine.EndsWith("\\"))
                {
                    concatenatedLine.Append(trimmedLine.Substring(0, trimmedLine.Length - 1) + "<br>");
                    continue;
                }
                else if (concatenatedLine.Length > 0)
                {
                    concatenatedLine.Append(trimmedLine);
                    trimmedLine = concatenatedLine.ToString();
                    concatenatedLine.Clear();
                }

                // Heading
                Match headingMatch = Regex.Match(trimmedLine, @"^(#{1,6})\s+(.+)");
                if (headingMatch.Success)
                {
                    if (inUnorderedList)
                    {
                        html.Append("</ul>\n");
                        inUnorderedList = false;
                    }
                    if (inOrderedList)
                    {
                        html.Append("</ol>\n");
                        inOrderedList = false;
                    }
                    int level = headingMatch.Groups[1].Length; // Count of '#' determines heading level
                    string content = headingMatch.Groups[2].Value; // Extracted heading text
                    html.AppendFormat("<h{0}>{1}</h{0}>\n", level, content);
                    continue;
                }

                // Blockquotes(>) simple case, it won't work for nested ones.
                if (trimmedLine.StartsWith(">"))
                {
                    html.AppendFormat("<blockquote>{0}</blockquote>\n", trimmedLine.Substring(1).Trim());
                    continue;
                }

                // Code Block
                if (trimmedLine.StartsWith("```"))
                {
                    if (notInCodingBlock)
                    {
                        html.Append("<pre><code>\n");
                    }
                    else
                    {
                        html.Append("</code></pre>\n");
                    }
                    notInCodingBlock = !notInCodingBlock;
                    continue; // Go to next iteration
                }

                // While notInCodingBlock is false, add the code in HTML as it is.
                if (!notInCodingBlock)
                {
                    html.Append(line + '\n');
                    continue; // Go to next iteration
                }

                // Horizontal Lines
                // Handle Horizontal Rules (---, ***, ___)
                if (Regex.IsMatch(trimmedLine, @"^(-{3,}|_{3,}|\*{3,})$"))
                {
                    html.Append("<hr>\n");
                    continue;
                }

                // Handle unordered lists
                Match unorderedMatch = Regex.Match(trimmedLine, @"^[-*]\s+(.+)");
                if (unorderedMatch.Success)
                {
                    if (!inUnorderedList)
                    {
                        html.Append("<ul>\n");
                        inUnorderedList = true;
                    }
                    html.AppendFormat("<li>{0}</li>\n", unorderedMatch.Groups[1].Value);
                    continue;
                }

                // Handle ordered lists
                Match orderedMatch = Regex.Match(trimmedLine, @"^\d+\.\s+(.+)");
                if (orderedMatch.Success)
                {
                    if (!inOrderedList)
                    {
                        html.Append("<ol>\n");
                        inOrderedList = true;
                    }
                    html.AppendFormat("<li>{0}</li>\n", orderedMatch.Groups[1].Value);
                    continue;
                }

                string newLine = trimmedLine;

                // Line Breaks
                // newLine = Regex.Replace(newLine, @"\n\s*\n", "<br>");
                if (newLine.EndsWith("  "))
                {
                    newLine = newLine.Substring(0, newLine.Length - 2) + "<br>";
                }

                // Convert Bold & Italic
                newLine = Regex.Replace(newLine, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
                newLine = Regex.Replace(newLine, @"___(.+?)___", "<strong><em>$1</em></strong>");
                newLine = Regex.Replace(newLine, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
                newLine = Regex.Replace(newLine, @"__(.+?)__", "<strong>$1</strong>");
                newLine = Regex.Replace(newLine, @"\*(.+?)\*", "<em>$1</em>");
                newLine = Regex.Replace(newLine, @"_(.+?)_", "<em>$1</em>");

                // Inline Code
                // To denote a word or phrase as code, enclose it in backticks (`).
                newLine = Regex.Replace(newLine, @"`(.*?)`", "<code>$1</code>");

                // image
                newLine = Regex.Replace(newLine, @"!\[(.*?)\]\((.*?)\)", "<img src=\"$2\" alt=\"$1\">");

                // Links
                newLine = Regex.Replace(newLine, @"\[(.*?)\]\((.*?)\)", "<a href=\"$2\">$1</a>");

                // Close lists when an empty line is encountered
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    if (inUnorderedList)
                    {
                        html.Append("</ul>\n");
                        inUnorderedList = false;
                    }
                    if (inOrderedList)
                    {
                        html.Append("</ol>\n");
                        inOrderedList = false;
                    }
                    continue; // Skip adding an empty paragraph
                }

                // Handle Paragraphs
                if (!string.IsNullOrWhiteSpace(newLine))
                {
                    html.AppendFormat("<p>{0}</p>\n", newLine);
                }
            }

            // Close any open tags
            if (inUnorderedList)
            {
                html.Append("</ul>\n");
                inUnorderedList = false;
            }
            if (inOrderedList)
            {
                html.Append("</ol>\n");
                inOrderedList = false;
            }
            return html.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return string.Empty;
        }
    }
}
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

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
            Stack<string> listStack = new Stack<string>();
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
                        html.Append("<code><pre>\n");
                    }
                    else
                    {
                        html.Append("</pre></code>\n");
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

                // Unordered List Detection
                if (Regex.IsMatch(line, @"^(\s*)[-*]\s+"))
                {
                    int indentLevel = Regex.Match(line, @"^(\s*)").Groups[1].Value.Length / 4;
                    while (listStack.Count > indentLevel)
                    {
                        html.Append(listStack.Pop() == "ul" ? "</ul>\n" : "</ol>\n");
                    }
                    if (listStack.Count == 0 || listStack.Peek() != "ul")
                    {
                        html.Append("<ul>\n");
                        listStack.Push("ul");
                    }
                    html.AppendFormat("<li>{0}</li>\n", line.TrimStart().Substring(2).Trim());
                    continue;
                }

                // Ordered List Detection
                if (Regex.IsMatch(line, @"^(\s*)\d+\.\s"))
                {
                    int indentLevel = Regex.Match(line, @"^(\s*)").Groups[1].Value.Length / 4; // 4 spaces per indent level
                    while (listStack.Count > indentLevel)
                    {
                        html.Append(listStack.Pop() == "ul" ? "</ul>\n" : "</ol>\n");
                    }
                    if (listStack.Count == 0 || listStack.Peek() != "ol")
                    {
                        html.Append("<ol>\n");
                        listStack.Push("ol");
                    }
                    html.AppendFormat("<li>{0}</li>\n", line.TrimStart().Substring(2).Trim());
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

                // Handle Paragraphs
                if (!string.IsNullOrWhiteSpace(newLine))
                {
                    // Close any open lists before starting a new paragraph
                    while (listStack.Count > 0)
                    {
                        html.Append(listStack.Pop() == "ul" ? "</ul>\n" : "</ol>\n");
                    }
                    html.AppendFormat("<p>{0}</p>\n", newLine);
                }
            }

            // Close any open tags
            while (listStack.Count > 0)
            {
                html.Append(listStack.Pop() == "ul" ? "</ul>\n" : "</ol>\n");
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

using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

class HtmlConverterProgram
{
    static void Main(string[] args)
    {
        string filePath = "input.md"; // Goes up two directories and then looks for input.md

        try
        {
            // Reading all contents from file.md
            string fileContent = File.ReadAllText(filePath);

            StringBuilder html = new StringBuilder();

            // Now Splitting the files when this character is encountered \n. Now lines are divided into array of strings
            string[] lines = fileContent.Split(new[] { '\n' });

            bool notInCodingBlock = true;
            bool insideUnorderedList = false;
            bool insideOrderedList = false;

            foreach (string line in lines)
            {
                // Now trimming extra white spaces and tabs.
                string trimmedLine = line.Trim(); // This will trim at the start and at the end too.

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

                // List Detection: Unordered List (starts with - or *)
                if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                {
                    if (!insideUnorderedList)
                    {
                        html.Append("<ul>\n");
                        insideUnorderedList = true;
                    }
                    html.AppendFormat("<li>{0}</li>\n", trimmedLine.Substring(2).Trim());
                    continue;
                }
                else
                {
                    if (insideUnorderedList)
                    {
                        html.Append("</ul>\n");
                        insideUnorderedList = false;
                    }
                }

                // Ordered List Detection (starts with number followed by period)
                if (Regex.IsMatch(trimmedLine, @"^\d+\.\s"))
                {
                    if (!insideOrderedList)
                    {
                        html.Append("<ol>\n");
                        insideOrderedList = true;
                    }
                    html.AppendFormat("<li>{0}</li>\n", trimmedLine.Substring(trimmedLine.IndexOf(' ') + 1).Trim());
                    continue;
                }
                else
                {
                    if (insideOrderedList)
                    {
                        html.Append("</ol>\n");
                        insideOrderedList = false;
                    }
                }

                string newLine = trimmedLine;

                // Line Breaks
                newLine = Regex.Replace(newLine, @"\n\s*\n", "<br>");

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
                    html.AppendFormat("<p>{0}</p>\n", newLine);
                }
            }

            // Write the output HTML to file
            string outputFilePath = "output.html"; // Path where you want to save the HTML file
            File.WriteAllText(outputFilePath, html.ToString());

            Console.WriteLine($"HTML content has been saved to {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

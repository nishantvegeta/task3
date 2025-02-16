using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string inputFilePath = "input.md";
        string outputFilePath = "output.html";

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        // Read Markdown content from file
        string markdownContent = File.ReadAllText(inputFilePath);

        // Convert Markdown to HTML
        string htmlContent = ConvertMarkdownListsToHtml(markdownContent);

        // Write HTML output to file
        File.WriteAllText(outputFilePath, htmlContent);

        Console.WriteLine("Conversion completed. Check output.html");
    }

    public static string ConvertMarkdownListsToHtml(string markdown)
    {
        string[] lines = markdown.Split('\n');
        return ParseMarkdownToHtml(lines);
    }

    private static string ParseMarkdownToHtml(string[] lines)
    {
        var sb = new StringBuilder();
        var stack = new Stack<(string tag, int indent)>(); // Stack for tracking open lists
        int prevIndent = 0;
        bool insideListItem = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            // Handle Headings (H1 to H6)
            var headingMatch = Regex.Match(trimmed, @"^(#{1,6})\s+(.*)");
            if (headingMatch.Success)
            {
                int level = headingMatch.Groups[1].Value.Length;
                string content = headingMatch.Groups[2].Value;

                // Close all open lists before heading
                while (stack.Count > 0)
                {
                    var (closingTag, _) = stack.Pop();
                    sb.AppendLine($"</{closingTag}>");
                }
                if (insideListItem)
                {
                    sb.AppendLine("</li>");
                    insideListItem = false;
                }

                sb.AppendLine($"<h{level}>{content}</h{level}>");
                continue;
            }

            if (string.IsNullOrEmpty(trimmed))
                continue;

            // Detect indentation (assumes 3 spaces per level)
            int indentLevel = line.TakeWhile(c => c == ' ').Count() / 3;

            // Match unordered lists (*, -, +)
            var unorderedMatch = Regex.Match(trimmed, @"^([*+-])\s+(.*)");
            // Match ordered lists (1., 2., etc.)
            var orderedMatch = Regex.Match(trimmed, @"^(\d+)\.\s+(.*)");

            if (unorderedMatch.Success || orderedMatch.Success)
            {
                string tag = unorderedMatch.Success ? "ul" : "ol";
                string content = unorderedMatch.Success ? unorderedMatch.Groups[2].Value : orderedMatch.Groups[2].Value;

                // If indentation increases, open a new list inside the <li>
                if (stack.Count == 0 || indentLevel > prevIndent)
                {
                    if (insideListItem)
                    {
                        sb.AppendLine("</li>"); // Ensure <li> closes before opening new list
                        insideListItem = false;
                    }
                    sb.AppendLine($"{new string(' ', indentLevel * 3)}<{tag}>");
                    stack.Push((tag, indentLevel));
                }
                // If indentation decreases, close lists until we reach the correct level
                else if (indentLevel < prevIndent)
                {
                    while (stack.Count > 0 && stack.Peek().indent >= indentLevel)
                    {
                        var (closingTag, _) = stack.Pop();
                        sb.AppendLine($"</{closingTag}>");

                        // Ensure <li> is closed properly before exiting the list
                        if (stack.Count > 0 && stack.Peek().tag == "li")
                        {
                            sb.AppendLine("</li>");
                            stack.Pop();
                        }
                    }
                }

                // If it's a subitem (nested), open a <li> tag
                if (insideListItem)
                {
                    sb.AppendLine("</li>");
                    insideListItem = false;
                }

                // Add the list item and keep it open for nesting
                sb.Append($"{new string(' ', indentLevel * 3 + 2)}<li>{content}");

                // Check if the current list item has subitems
                bool hasSubitem = false;
                for (int i = 1; i < lines.Length; i++)
                {
                    string nextLine = lines[i].Trim();
                    if (nextLine.StartsWith(" ") && nextLine.Length > 0)
                    {
                        int nextIndentLevel = nextLine.TakeWhile(c => c == ' ').Count() / 3;
                        if (nextIndentLevel > indentLevel)
                        {
                            hasSubitem = true;
                            break;
                        }
                    }
                }

                // If subitems exist, open a new <ul> or <ol> for subitems
                if (hasSubitem)
                {
                    sb.AppendLine();
                    sb.Append($"{new string(' ', (indentLevel + 1) * 3)}<{tag}>");
                    stack.Push((tag, indentLevel + 1));
                }
                else
                {
                    sb.AppendLine("</li>");
                    insideListItem = false;
                }

                prevIndent = indentLevel;
                stack.Push(("li", indentLevel));
            }
        }

        // Ensure correct closure of open lists and list items
        if (insideListItem) sb.AppendLine("</li>");
        while (stack.Count > 0)
        {
            var (closingTag, _) = stack.Pop();
            sb.AppendLine($"</{closingTag}>");
        }

        return sb.ToString();
    }
}

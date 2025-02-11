using System;
using System.IO;
using Markdig;

class Program
{
    static void Main()
    {
        string inputFilePath = "input.md";
        string outputFilePath = "output.html";

        // Read Markdown content from file
        string markdownContent = File.ReadAllText(inputFilePath);

        // Convert Markdown to HTML using Markdig
        string htmlContent = Markdown.ToHtml(markdownContent);

        // Wrap in basic HTML structure
        string fullHtml = "<!DOCTYPE html>\n<html>\n<head>\n<title>Markdown Output</title>\n</head>\n<body>\n" + htmlContent + "\n</body>\n</html>";

        // Write HTML output to file
        File.WriteAllText(outputFilePath, fullHtml);

        Console.WriteLine("Conversion completed. Check output.html");
    }
}
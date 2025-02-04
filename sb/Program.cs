using System;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Hello, ");
        sb.Append("Nishant ");
        sb.Append("How are you?");

        Console.WriteLine(sb.ToString());

        List<string> names = new List<string> { "Nishant", "Ran", "bam", "yoru" };

        string name = GenerateCommaSeparatedString(names);
        Console.WriteLine(name);
    }

    static string GenerateCommaSeparatedString(List<string> names)
    {
        StringBuilder sb = new StringBuilder();

        foreach (string name in names)
        {
            sb.Append(name);
            sb.Append(", ");
        }

        if (sb.Length > 0)
        {
            sb.Length -= 2;
        }

        return sb.ToString();
    }
}

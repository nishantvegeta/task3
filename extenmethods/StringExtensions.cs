using System;
using System.Text;

public static class StringExtensions
{
    public static string ReverseString(this string s)
    {
        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        else
        {
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }

    public static string GetVowels(this string s)
    {
        string vowels = "aeiouAEIOU";
        StringBuilder sb = new StringBuilder();

        foreach (char c in s)
        {
            if (vowels.Contains(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static int Age(this DateTime dateOfBirth)
    {
        DateTime today = DateTime.Today;
        int age = today.Year - dateOfBirth.Year;
        if (today < dateOfBirth.AddYears(age)) age--;
        return age;
    }
}

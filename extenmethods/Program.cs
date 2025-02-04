using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Please enter a string:");
        string s = Console.ReadLine();

        // Reverse the string
        Console.WriteLine(s.ReverseString());

        // Convert the string to title case
        Console.WriteLine(s.ToTitleCase());

        // Get the vowels in the string
        Console.WriteLine(s.GetVowels());

        // Calculate the age of a person born on the entered date
        Console.WriteLine("Please enter your date of birth (yyyy-MM-dd):");
        DateTime dateOfBirth = DateTime.Parse(Console.ReadLine());
        Console.WriteLine(dateOfBirth.Age());
    }
}
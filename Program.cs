using System;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main()
    {
        Console.Write("Enter the file path: ");
        string path = Console.ReadLine();
        string fileName = Path.GetFileName(path);
        if (File.Exists(path))
        {
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                // Handle invalid file type error
                Console.WriteLine("Only CSV files are allowed.");
            }
            ReadCsvFile(path);
        }
        else
        {
            Console.WriteLine("File not found.");
        }       


    }   

    public static void ReadCsvFile(string filePath)
    {
       
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                // Optional: Skip header row
                
                string outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), "Santa_output_" + Path.GetFileName(filePath));

                               
                List<string[]> strline = new List<string[]>();
                while (!reader.EndOfStream)
                {                    
                    var line = reader.ReadLine();                                     
                    if (line != null){

                        var values = line.Split(','); // Split by comma
                        strline = strline.Concat(values.ToList().Select(v => new string[] { v })).ToList();                         
                        
                    }
                }              
                
                // Shuffle the employees without self-mapping
                var skipstrline = strline.Skip(1).ToList();
                var shuffledEmployees = EmployeeDerangement.ShuffleWithoutSelfMap(skipstrline);
                shuffledEmployees.Insert(0, new string[] { "\t Secret_Child_Name \t Secret_Child_EmailID\r\n" }); // Reinsert header
                strline= strline.Where(s => !string.IsNullOrWhiteSpace(s[0])).ToList();
                shuffledEmployees = shuffledEmployees.Where(s => !string.IsNullOrWhiteSpace(s[0])).ToList();
                var randomMapping = strline.Zip(shuffledEmployees, (original, mapped) => new
                {
                    OriginalEmployee = original,
                    MappedEmployee = mapped
                });
                var mappedList = new List<string[]>();                
                foreach (var mapping in randomMapping)
                {

                    mappedList.Add(new string[] { mapping.OriginalEmployee[0], "  ",mapping.MappedEmployee[0] });
                   

                }
                WriteCsv(outputFilePath, mappedList);

            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: File not found at {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public static void WriteCsv(string filePath, List<string[]> data)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string[] row in data)
            {
                writer.WriteLine(string.Join("  ", row)); // Joins fields with comma and writes the line
                
            }
        }
    }
   

}



public static class EmployeeDerangement
{
    private static readonly Random _rng = new Random();

    /// <summary>
    /// Shuffles a list of employees to a new list where no employee is in their original position.
    /// </summary>
    /// <param name="employees">The original list of employees.</param>
    /// <returns>A new list representing a derangement of the original employees.</returns>
    public static List<string[]> ShuffleWithoutSelfMap(List<string[]> employees)
    {
        if (employees.Count < 2)
        {
            // A derangement is not possible for lists with 0 or 1 item.
            // You can choose to throw an exception or return an empty/unchanged list.
            return new List<string[]>(employees);
        }

        var source = employees.ToList();
        var shuffled = employees.ToList();

        // Step 1: Fisher-Yates shuffle
        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            (shuffled[n], shuffled[k]) = (shuffled[k], shuffled[n]);
        }

        // Step 2: Fix any self-mappings
        for (int i = 0; i < shuffled.Count; i++)
        {
            if (source[i].Equals(shuffled[i]))
            {
                // Find a candidate for a swap
                int swapIndex = (i + 1) % shuffled.Count;
                if (source[i] == shuffled[swapIndex] && source[swapIndex] == shuffled[i])
                {
                    // This is a special case where we have a perfect swap pair.
                    // We need to find a third person to break the cycle.
                    int thirdSwapIndex = (i + 2) % shuffled.Count;
                    (shuffled[i], shuffled[thirdSwapIndex]) = (shuffled[thirdSwapIndex], shuffled[i]);
                }
                else
                {
                    (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
                }
            }
        }

        return shuffled;
    }
}

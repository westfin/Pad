#r ".\Chapter11.exe"

using Chapter11.Model;
using Chapter11.Queries;

var query = from defect in SampleData.AllDefects
            where defect.AssignedTo != null
            group defect.Summary by defect.AssignedTo;

foreach (var entry in query)
{
    Console.WriteLine(entry.Key.Name);
    foreach (var summary in entry)
    {
        Console.WriteLine("  {0}", summary);
    }
    Console.WriteLine();
}
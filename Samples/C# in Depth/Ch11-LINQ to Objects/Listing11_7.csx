#r ".\Chapter11.exe"

using Chapter11.Model;

User tim = SampleData.Users.TesterTim;

var query = from bug in SampleData.AllDefects 
            where bug.Status != Status.Closed
            where bug.AssignedTo == tim
            select bug.Summary;

foreach (var summary in query)
{
    Console.WriteLine(summary);
}
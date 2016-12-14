#r ".\Chapter11.exe"

using Chapter11.Model;

var query = SampleData.AllUsers.Select(user => user);

foreach (var user in query)
{
    Console.WriteLine(user);
}
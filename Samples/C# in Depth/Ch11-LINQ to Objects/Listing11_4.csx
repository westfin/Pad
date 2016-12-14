#r ".\Chapter11.exe"

using Chapter11.Model;

var query = from user in SampleData.AllUsers 
            select user.Name;

foreach (string name in query)
{
    Console.WriteLine(name);
}
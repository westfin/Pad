#r ".\Chapter11.exe"

using Chapter11.Model;

var query = from user in SampleData.AllUsers 
            select user;

foreach (var user in query)
{
    Console.WriteLine(user);
}
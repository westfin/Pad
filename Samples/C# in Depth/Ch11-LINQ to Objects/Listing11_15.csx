#r ".\Chapter11.exe"

using Chapter11.Model;
using Chapter11.Queries;

var query = from user in SampleData.AllUsers
            from project in SampleData.AllProjects
select new { User=user, Project=project };

foreach (var pair in query)
{
    Console.WriteLine("{0}/{1}", 
                      pair.User.Name,
                      pair.Project.Name);
}
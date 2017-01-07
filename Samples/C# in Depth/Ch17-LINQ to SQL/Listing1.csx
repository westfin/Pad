#r "System.Data.Linq"
#r "System.Data"

using System.Data.Linq;
using System.Data.Linq.Mapping;


[Table(Name = "Table")]
public class Companie
{
    private string name;
    [Column(IsPrimaryKey = true, Storage = nameof(name))]
    public string Name  
    {
        get
        {
            return this.name;
        }

        set
        {
            this.name = value;
        }

    }

    private int? employeeCount;
    [Column(IsPrimaryKey = true, Storage = nameof(employeeCount))]
    public int? EmployeeCount
    {
        get
        {
            return this.employeeCount;
        }

        set
        {
            this.employeeCount = value;
        }

    }


    private DateTime? startDate;
    [Column(IsPrimaryKey = true, Storage = nameof(startDate))]
    public DateTime? StartDate
    {
        get
        {
            return this.startDate;
        }

        set
        {
            this.startDate = value;
        }

    }


    private string ceo;
    [Column(Storage = nameof(ceo))]
    public string CEO
    {
        get
        {
            return this.ceo;
        }
        set
        {
            this.ceo = value;
        }
    }
}

// Use a connection string.
DataContext db = new DataContext(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\linqtest\DemoDb.mdf;Integrated Security=True;Connect Timeout=30");

// Get a typed table to run queries.
Table<Customer> Customers = db.GetTable<Customer>();

IQueryable<Customer> custQuery =
    from cust in Customers
    select cust;

foreach (var item in custQuery)
{
    item.Title.Dump(nameof(item.Title));
}
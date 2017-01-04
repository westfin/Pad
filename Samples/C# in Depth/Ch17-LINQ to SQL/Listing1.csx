#r "System.Data.Linq"
#r "System.Data"

using System.Data.Linq;
using System.Data.Linq.Mapping;


[Table(Name = "Table")]
public class Customer
{
    private int Id;
    [Column(IsPrimaryKey = true, Storage = nameof(Id))]
    public int ID
    {
        get
        {
            return this.Id;
        }
        set
        {
            this.Id = value;
        }

    }

    private string title;
    [Column(Storage = nameof(title))]
    public string Title
    {
        get
        {
            return this.title;
        }
        set
        {
            this.title = value;
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
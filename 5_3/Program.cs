using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies()        // подключение lazy loading
            .UseSqlite("Data Source=helloapp.db");
    }
}
public class Company
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public virtual List<User> Users { get; set; } = new();
}

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public int? CompanyId { get; set; }
    public virtual Company? Company { get; set; }
}
class Program
{
    static void Main(string[] args)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            // пересоздадим базу данных
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // добавляем начальные данные
            Company microsoft = new Company { Name = "Microsoft" };
            Company google = new Company { Name = "Google" };
            db.Companies.AddRange(microsoft, google);


            User tom = new User { Name = "Tom", Company = microsoft };
            User bob = new User { Name = "Bob", Company = google };
            User alice = new User { Name = "Alice", Company = microsoft };
            User kate = new User { Name = "Kate", Company = google };
            db.Users.AddRange(tom, bob, alice, kate);

            db.SaveChanges();
        }
        using (ApplicationContext db = new ApplicationContext())
        {
            var users = db.Users.ToList();
            foreach (User user in users)
                Console.WriteLine($"{user.Name} - {user.Company?.Name}");
        }
        using (ApplicationContext db = new ApplicationContext())
        {
            var companies = db.Companies.ToList();
            foreach (Company company in companies)
            {
                Console.Write($"{company.Name}:");
                foreach (User user in company.Users)
                    Console.Write($"{user.Name} ");
                Console.WriteLine();
            }
        }
    }
}
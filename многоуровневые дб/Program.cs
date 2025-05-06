using Microsoft.EntityFrameworkCore;
public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<Position> Positions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }
}
// столица страны
public class City
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
// страна компании
public class Country
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int CapitalId { get; set; }
    public City? Capital { get; set; }  // столица страны
    public List<Company> Companies { get; set; } = new();
}
public class Company
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public List<User> Users { get; set; } = new();
}
// должность пользователя
public class Position
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<User> Users { get; set; } = new();
}
public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    public int? PositionId { get; set; }
    public Position? Position { get; set; }
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

            Position manager = new Position { Name = "Manager" };
            Position developer = new Position { Name = "Developer" };
            db.Positions.AddRange(manager, developer);

            City washington = new City { Name = "Washington" };
            db.Cities.Add(washington);

            Country usa = new Country { Name = "USA", Capital = washington };
            db.Countries.Add(usa);

            Company microsoft = new Company { Name = "Microsoft", Country = usa };
            Company google = new Company { Name = "Google", Country = usa };
            db.Companies.AddRange(microsoft, google);

            User tom = new User { Name = "Tom", Company = microsoft, Position = manager };
            User bob = new User { Name = "Bob", Company = google, Position = developer };
            User alice = new User { Name = "Alice", Company = microsoft, Position = developer };
            User kate = new User { Name = "Kate", Company = google, Position = manager };
            db.Users.AddRange(tom, bob, alice, kate);

            db.SaveChanges();
        }
        using (ApplicationContext db = new ApplicationContext())
        {
            // получаем пользователей
            var users = db.Users
                            .Include(u => u.Company)  // добавляем данные по компаниям
                                .ThenInclude(comp => comp!.Country)      // к компании добавляем страну 
                                    .ThenInclude(count => count!.Capital)    // к стране добавляем столицу
                            .Include(u => u.Position) // добавляем данные по должностям
                            .ToList();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Name} - {user.Position?.Name}");
                Console.WriteLine($"{user.Company?.Name} - {user.Company?.Country?.Name} - {user.Company?.Country?.Capital?.Name}");
                Console.WriteLine("----------------------");     // для красоты
            }
        }
    }
}
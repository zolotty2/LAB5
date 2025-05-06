using Microsoft.EntityFrameworkCore;

// Контекст приложения, который управляет подключением к базе данных и определяет наборы данных
public class ApplicationContext : DbContext
{
    // Набор данных для пользователей
    public DbSet<User> Users { get; set; } = null!;
    // Набор данных для компаний
    public DbSet<Company> Companies { get; set; } = null!;

    // Метод для настройки параметров подключения к базе данных
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Используем SQLite в качестве базы данных
        optionsBuilder.UseSqlite("Data Source=helloapp.db");
    }
}

// Класс, представляющий компанию
public class Company
{
    public int Id { get; set; } // Идентификатор компании
    public string? Name { get; set; } // Название компании
    // Список пользователей, связанных с этой компанией
    public List<User> Users { get; set; } = new();
}

// Класс, представляющий пользователя
public class User
{
    public int Id { get; set; } // Идентификатор пользователя
    public string? Name { get; set; } // Имя пользователя

    // Идентификатор компании, к которой принадлежит пользователь
    public int? CompanyId { get; set; }
    // Ссылка на компанию, к которой принадлежит пользователь
    public Company? Company { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Создаем и настраиваем контекст базы данных
        using (ApplicationContext db = new ApplicationContext())
        {
            // Удаляем существующую базу данных и создаем новую
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Добавляем начальные данные
            Company microsoft = new Company { Name = "Microsoft" };
            Company google = new Company { Name = "Google" };
            db.Companies.AddRange(microsoft, google);

            // Создаем пользователей и связываем их с компаниями
            User tom = new User { Name = "Tom", Company = microsoft };
            User bob = new User { Name = "Bob", Company = google };
            User alice = new User { Name = "Alice", Company = microsoft };
            User kate = new User { Name = "Kate", Company = google };
            db.Users.AddRange(tom, bob, alice, kate);

            // Сохраняем изменения в базе данных
            db.SaveChanges();
        }

        // Чтение данных из базы данных
        using (ApplicationContext db = new ApplicationContext())
        {
            // Получаем первую компанию из базы данных
            Company? company = db.Companies.FirstOrDefault();
            if (company != null)
            {
                // Загружаем пользователей, связанных с этой компанией
                db.Users.Where(u => u.CompanyId == company.Id).Load();

                // Выводим информацию о компании и ее пользователях
                Console.WriteLine($"Company: {company.Name}");
                foreach (var u in company.Users)
                    Console.WriteLine($":User  {u.Name}");
            }
        }

        // Альтернативный способ загрузки пользователей компании
        using (ApplicationContext db = new ApplicationContext())
        {
            Company? company = db.Companies.FirstOrDefault();
            if (company != null)
            {
                // Загружаем коллекцию пользователей для компании
                db.Entry(company).Collection(c => c.Users).Load();

                // Выводим информацию о компании и ее пользователях
                Console.WriteLine($"Company: {company.Name}");
                foreach (var u in company.Users)
                    Console.WriteLine($":User  {u.Name}");
            }
        }

        // Загрузка компании для конкретного пользователя
        using (ApplicationContext db = new ApplicationContext())
        {
            User? user = db.Users.FirstOrDefault();  // Получаем первого пользователя
            if (user != null)
            {
                // Загружаем информацию о компании пользователя
                db.Entry(user).Reference(u => u.Company).Load();
                Console.WriteLine($"{user.Name} - {user.Company?.Name}");   // Выводим имя пользователя и название компании
            }
        }

        // Получение всех пользователей по компаниям
        using (ApplicationContext db = new ApplicationContext())
        {
            Company? company1 = db.Companies.Find(1); // Находим первую компанию
            if (company1 != null)
            {
                // Загружаем пользователей, связанных с первой компанией
                db.Users.Where(u => u.CompanyId == company1.Id).Load();
                foreach (var u in company1.Users) Console.WriteLine($":User  {u.Name}");
                Company? company2 = db.Companies.Find(2); // Находим вторую компанию
                if (company2 != null)
                {
                    // Загружаем пользователей, связанных со второй компанией
                    db.Users.Where(u => u.CompanyId == company2.Id).Load();
                    foreach (var u in company2.Users) Console.WriteLine($":User   {u.Name}");
                }
                // Получаем всех сотрудников из базы данных
                foreach (var u in db.Users) Console.WriteLine($":User   {u.Name}");
            }
        }
    }
}
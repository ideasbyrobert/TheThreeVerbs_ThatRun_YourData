using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using TheaterSales.DotNet.Domain;

namespace TheaterSales.DotNet.Data;

public class TheaterSalesContext : DbContext
{
    private static class TableNames
    {
        public const string Theaters = "theaters";
        public const string Movies = "movies";
        public const string Sales = "sales";
    }

    private static class ColumnNames
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Title = "title";
        public const string TheaterId = "theater_id";
        public const string MovieId = "movie_id";
        public const string SaleDate = "sale_date";
        public const string Amount = "amount";
    }

    public TheaterSalesContext(DbContextOptions<TheaterSalesContext> options) : base(options)
    {
    }

    public DbSet<Theater> Theaters { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Sale> Sales { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTheaterEntity(modelBuilder);
        ConfigureMovieEntity(modelBuilder);
        ConfigureSaleEntity(modelBuilder);
    }

    private static void ConfigureTheaterEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theater>(entity =>
        {
            entity.ToTable(TableNames.Theaters);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName(ColumnNames.Id);
            entity.Property(e => e.Name).HasColumnName(ColumnNames.Name).IsRequired();
        });
    }

    private static void ConfigureMovieEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.ToTable(TableNames.Movies);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName(ColumnNames.Id);
            entity.Property(e => e.Title).HasColumnName(ColumnNames.Title).IsRequired();
        });
    }

    private static void ConfigureSaleEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable(TableNames.Sales);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName(ColumnNames.Id);
            entity.Property(e => e.TheaterId).HasColumnName(ColumnNames.TheaterId);
            entity.Property(e => e.MovieId).HasColumnName(ColumnNames.MovieId);
            entity.Property(e => e.SaleDate).HasColumnName(ColumnNames.SaleDate);
            entity.Property(e => e.Amount)
                .HasColumnName(ColumnNames.Amount)
                .HasPrecision(10, 2);
        });
    }
}

public class DatabaseInitializer
{
    private static class FileNames
    {
        public const string Schema = "schema.sql";
        public const string TestFixtures = "test_fixtures.sql";
    }

    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Initialize()
    {
        ExecuteSqlFile(FileNames.Schema);
    }

    public void SeedData()
    {
        ExecuteSqlFile(FileNames.TestFixtures);
    }

    private void ExecuteSqlFile(string fileName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        var sql = File.ReadAllText(sqlPath);

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
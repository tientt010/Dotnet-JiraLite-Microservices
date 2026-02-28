using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JiraLite.Infrastructure.Data;

public class JiraLiteDbContextFactory : IDesignTimeDbContextFactory<JiraLiteDbContext>
{
    public JiraLiteDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "JiraLite.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<JiraLiteDbContext>();
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("PostgreSqlConnection"));

        return new JiraLiteDbContext(optionsBuilder.Options);
    }
}

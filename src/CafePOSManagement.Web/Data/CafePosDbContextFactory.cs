using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CafePOSManagement.Web.Data;

// Cho EF CLI dựng model mà không khởi động web hoặc chuyển Service in-memory sang SQL.
public sealed class CafePosDbContextFactory : IDesignTimeDbContextFactory<CafePosDbContext>
{
    public CafePosDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
            .AddUserSecrets<CafePosDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = configuration.GetConnectionString("CafePos");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Đặt ConnectionStrings__CafePos hoặc User Secrets trước khi dùng EF CLI. Không có database mặc định ngầm.");
        return new CafePosDbContext(new DbContextOptionsBuilder<CafePosDbContext>().UseSqlServer(connection).Options);
    }
}

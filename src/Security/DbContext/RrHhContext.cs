using Microsoft.EntityFrameworkCore;
using Security.Entities;

public class RrHhContext : DbContext
{
    public RrHhContext(DbContextOptions<RrHhContext> options) : base(options)
    { }

    public DbSet<VwEmployee> VwEmployees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging(true);
#endif
        base.OnConfiguring(optionsBuilder);
    }

}

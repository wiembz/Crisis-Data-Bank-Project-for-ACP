using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using backend.Models;
using System.Text.Json;
using System.Linq;

namespace backend.Data
{
    public class CrisisDbContext : DbContext
    {
        public CrisisDbContext(DbContextOptions<CrisisDbContext> options) : base(options) { }

        public DbSet<Crisis> Crises { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var jsonOptions = new JsonSerializerOptions(); 

            // Configure the Tags and AffectedSystems properties to be stored as JSON arrays
            modelBuilder.Entity<Crisis>()
                .Property(c => c.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            modelBuilder.Entity<Crisis>()
                .Property(c => c.AffectedSystems)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions),
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            base.OnModelCreating(modelBuilder);
        }
    }
}

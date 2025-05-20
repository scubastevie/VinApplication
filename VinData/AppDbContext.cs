using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VinData.Models;

namespace VinData;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Record> Records => Set<Record>();
    public DbSet<DecodedVehicle> DecodedVehicles => Set<DecodedVehicle>();
}

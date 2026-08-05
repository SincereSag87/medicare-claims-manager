using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<Claim> Claims => Set<Claim>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Patient>()
            .HasIndex(patient => patient.MedicareNumber)
            .IsUnique();

        builder.Entity<Provider>()
            .HasIndex(provider => provider.Npi)
            .IsUnique();

        builder.Entity<Claim>()
            .HasIndex(claim => claim.ClaimNumber)
            .IsUnique();

        builder.Entity<Claim>()
            .HasOne(claim => claim.Patient)
            .WithMany(patient => patient.Claims)
            .HasForeignKey(claim => claim.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Claim>()
            .HasOne(claim => claim.Provider)
            .WithMany(provider => provider.Claims)
            .HasForeignKey(claim => claim.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

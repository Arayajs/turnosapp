using Microsoft.EntityFrameworkCore;
using TurnOS.Domain.Entities;

namespace TurnOS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Business>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).HasMaxLength(100).IsRequired();
            e.Property(b => b.Address).HasMaxLength(200);
            e.Property(b => b.Phone).HasMaxLength(20);
            e.HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Service>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(100).IsRequired();
            e.Property(s => s.Price).HasColumnType("decimal(18,2)");
            e.HasOne(s => s.Business)
                .WithMany(b => b.Services)
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Notes).HasMaxLength(500);
            e.HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Business)
                .WithMany()
                .HasForeignKey(a => a.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

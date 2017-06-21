using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EmailService.Core.Entities
{
    public class EmailServiceContext : DbContext
    {
        public EmailServiceContext(DbContextOptions<EmailServiceContext> options)
            : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Layout> Layouts { get; set; }

        public DbSet<Transport> Transports { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<Translation> Translations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //
            // Applications
            modelBuilder.Entity<Application>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Application>()
                .HasIndex(u => u.Name)
                .IsUnique(true);

            modelBuilder.Entity<Application>()
                .HasMany(u => u.Templates)
                .WithOne(t => t.Application)
                .HasForeignKey(u => u.ApplicationId);

            modelBuilder.Entity<Application>()
                .HasMany(u => u.Layouts)
                .WithOne(t => t.Application)
                .HasForeignKey(u => u.ApplicationId);

            //
            // Transports
            modelBuilder.Entity<Transport>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Transport>()
                .HasIndex(u => u.Name)
                .IsUnique(true);

            //
            // Layouts
            modelBuilder.Entity<Layout>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Layout>()
                .HasOne(u => u.Application)
                .WithMany(a => a.Layouts)
                .HasForeignKey(u => u.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            //
            // Templates
            modelBuilder.Entity<Template>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Template>()
                .HasMany(t => t.Translations)
                .WithOne(t => t.Template)
                .HasForeignKey(t => t.TemplateId);

            modelBuilder.Entity<Template>()
                .HasOne(u => u.Layout)
                .WithMany(l => l.Templates)
                .HasForeignKey(u => u.LayoutId);

            modelBuilder.Entity<Template>()
                .HasIndex(u => new { u.ApplicationId, u.Name })
                .IsUnique(true);

            modelBuilder.Entity<Translation>()
                .HasKey(u => new { u.TemplateId, u.Language });

            //
            // Application transports
            modelBuilder.Entity<ApplicationTransport>()
                .HasKey(t => new { t.ApplicationId, t.TransportId });

            modelBuilder.Entity<ApplicationTransport>()
                .HasOne(pt => pt.Application)
                .WithMany(p => p.Transports)
                .HasForeignKey(pt => pt.ApplicationId);

            modelBuilder.Entity<ApplicationTransport>()
                .HasOne(pt => pt.Transport)
                .WithMany(t => t.Applications)
                .HasForeignKey(pt => pt.TransportId);

            base.OnModelCreating(modelBuilder);
        }
    }
}

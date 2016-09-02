using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using EmailService.Core.Entities;

namespace EmailService.Web.Migrations
{
    [DbContext(typeof(EmailServiceContext))]
    [Migration("20160831110147_ApiKeys")]
    partial class ApiKeys
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EmailService.Core.Entities.Application", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<DateTime>("CreatedUtc");

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<bool>("IsActive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("PrivateKey");

                    b.Property<string>("PublicKey");

                    b.Property<string>("SenderAddress")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("SenderName")
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("EmailService.Core.Entities.ApplicationTransport", b =>
                {
                    b.Property<Guid>("ApplicationId");

                    b.Property<Guid>("TransportId");

                    b.Property<int>("Priority");

                    b.HasKey("ApplicationId", "TransportId");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("TransportId");

                    b.ToTable("ApplicationTransport");
                });

            modelBuilder.Entity("EmailService.Core.Entities.Template", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ApplicationId");

                    b.Property<string>("BodyTemplate")
                        .IsRequired();

                    b.Property<byte[]>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<DateTime>("CreatedUtc");

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<bool>("IsActive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("SampleData");

                    b.Property<string>("SubjectTemplate")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<bool>("UseHtml");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("ApplicationId", "Name")
                        .IsUnique();

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("EmailService.Core.Entities.Translation", b =>
                {
                    b.Property<Guid>("TemplateId");

                    b.Property<string>("Language")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("BodyTemplate")
                        .IsRequired();

                    b.Property<byte[]>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<DateTime>("CreatedUtc");

                    b.Property<string>("SubjectTemplate")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("TemplateId", "Language");

                    b.HasIndex("TemplateId");

                    b.ToTable("Translations");
                });

            modelBuilder.Entity("EmailService.Core.Entities.Transport", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<DateTime>("CreatedUtc");

                    b.Property<string>("Hostname")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<bool>("IsActive");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Password")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<short?>("PortNum");

                    b.Property<string>("SenderAddress")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("SenderName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("Type");

                    b.Property<bool>("UseSSL");

                    b.Property<string>("Username")
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Transports");
                });

            modelBuilder.Entity("EmailService.Core.Entities.ApplicationTransport", b =>
                {
                    b.HasOne("EmailService.Core.Entities.Application", "Application")
                        .WithMany("Transports")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("EmailService.Core.Entities.Transport", "Transport")
                        .WithMany("Applications")
                        .HasForeignKey("TransportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EmailService.Core.Entities.Template", b =>
                {
                    b.HasOne("EmailService.Core.Entities.Application", "Application")
                        .WithMany("Templates")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EmailService.Core.Entities.Translation", b =>
                {
                    b.HasOne("EmailService.Core.Entities.Template", "Template")
                        .WithMany("Translations")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

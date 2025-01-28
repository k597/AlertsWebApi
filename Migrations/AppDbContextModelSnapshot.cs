﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AlertsWebApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.36");

            modelBuilder.Entity("ObWebApi3.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<int>("Severity")
                        .HasMaxLength(50)
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("ObWebApi3.AlertIpAddress", b =>
                {
                    b.Property<int>("AlertId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IpAddressId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AlertId", "IpAddressId");

                    b.HasIndex("IpAddressId");

                    b.ToTable("AlertIpAddresses");
                });

            modelBuilder.Entity("ObWebApi3.IpAddress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Blacklisted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.Property<int>("SourceType")
                        .HasMaxLength(50)
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("IpAddresses");
                });

            modelBuilder.Entity("ObWebApi3.AlertIpAddress", b =>
                {
                    b.HasOne("ObWebApi3.Alert", "Alert")
                        .WithMany("AlertIpAddresses")
                        .HasForeignKey("AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ObWebApi3.IpAddress", "IpAddress")
                        .WithMany("AlertIpAddresses")
                        .HasForeignKey("IpAddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Alert");

                    b.Navigation("IpAddress");
                });

            modelBuilder.Entity("ObWebApi3.Alert", b =>
                {
                    b.Navigation("AlertIpAddresses");
                });

            modelBuilder.Entity("ObWebApi3.IpAddress", b =>
                {
                    b.Navigation("AlertIpAddresses");
                });
#pragma warning restore 612, 618
        }
    }
}

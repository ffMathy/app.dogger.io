﻿// <auto-generated />
using System;
using Dogger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dogger.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20200331143632_UniqueIdentityName")]
    partial class UniqueIdentityName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Dogger.Domain.Models.Cluster", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Clusters");
                });

            modelBuilder.Entity("Dogger.Domain.Models.Identity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Identities");
                });

            modelBuilder.Entity("Dogger.Domain.Models.Instance", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ClusterId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsProvisioned")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PlanId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TotalPriceUsdInHundreds")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClusterId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("Dogger.Domain.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("StripeCustomerId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Dogger.Domain.Models.Cluster", b =>
                {
                    b.HasOne("Dogger.Domain.Models.User", "User")
                        .WithMany("Clusters")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Dogger.Domain.Models.Identity", b =>
                {
                    b.HasOne("Dogger.Domain.Models.User", "User")
                        .WithMany("Identities")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Dogger.Domain.Models.Instance", b =>
                {
                    b.HasOne("Dogger.Domain.Models.Cluster", "Cluster")
                        .WithMany("Instances")
                        .HasForeignKey("ClusterId");
                });
#pragma warning restore 612, 618
        }
    }
}

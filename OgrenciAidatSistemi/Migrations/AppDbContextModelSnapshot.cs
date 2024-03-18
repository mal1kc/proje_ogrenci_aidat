﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OgrenciAidatSistemi.Data;

#nullable disable

namespace OgrenciAidatSistemi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("OgrenciAidatSistemi.Models.School", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Schools");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("TEXT");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("createdAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("updatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasDiscriminator<string>("Discriminator").HasValue("User");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SchoolAdmin", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<int>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.HasIndex("SchoolId");

                    b.ToTable("Users", t =>
                        {
                            t.Property("SchoolId")
                                .HasColumnName("SchoolAdmin_SchoolId");
                        });

                    b.HasDiscriminator().HasValue("SchoolAdmin");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SiteAdmin", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<int>("SiteAdminId")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("SiteAdmin");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Student", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<int>("GradLevel")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsGraduated")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StudentId")
                        .HasColumnType("INTEGER");

                    b.HasIndex("SchoolId");

                    b.HasDiscriminator().HasValue("Student");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SchoolAdmin", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Student", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany("Students")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.School", b =>
                {
                    b.Navigation("Students");
                });
#pragma warning restore 612, 618
        }
    }
}

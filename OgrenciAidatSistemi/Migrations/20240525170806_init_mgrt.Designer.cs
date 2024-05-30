// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OgrenciAidatSistemi.Data;

#nullable disable

namespace OgrenciAidatSistemi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240525170806_init_mgrt")]
    partial class init_mgrt
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("GradeStudent", b =>
                {
                    b.Property<int>("GradesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StudentsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GradesId", "StudentsId");

                    b.HasIndex("StudentsId");

                    b.ToTable("GradeStudent");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.ContactInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Addresses")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Grade", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("GradeLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SchoolId");

                    b.ToTable("Grades");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("PaymentMethod")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PaymentPeriodId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StudentId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PaymentPeriodId");

                    b.HasIndex("SchoolId");

                    b.HasIndex("StudentId");

                    b.ToTable("Payments");

                    b.HasDiscriminator<int>("PaymentMethod");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.PaymentPeriod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Occurrence")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("PerPaymentAmount")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("StudentId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("WorkYearId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("StudentId");

                    b.HasIndex("WorkYearId");

                    b.ToTable("PaymentPeriods");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Receipt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PaymentId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Path")
                        .IsUnique();

                    b.HasIndex("PaymentId")
                        .IsUnique();

                    b.ToTable("Receipts");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.School", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
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
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("EmailAddress")
                        .IsUnique();

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Users");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.WorkYear", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SchoolId");

                    b.ToTable("WorkYears");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.PaidPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.Payment");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.UnPaidPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.Payment");

                    b.HasDiscriminator().HasValue(0);
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SchoolAdmin", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<int>("ContactInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.HasIndex("ContactInfoId");

                    b.HasIndex("SchoolId");

                    b.ToTable("SchoolAdmins");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SiteAdmin", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.ToTable("SiteAdmins");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Student", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.User");

                    b.Property<int?>("ContactInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GradLevel")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLeftSchool")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SchoolId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StudentId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasIndex("ContactInfoId");

                    b.HasIndex("SchoolId");

                    b.HasIndex("StudentId")
                        .IsUnique();

                    b.ToTable("Students");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.BankPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.PaidPayment");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BankName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BranchCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IBAN")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.ToTable("Payments", t =>
                        {
                            t.Property("BankName")
                                .HasColumnName("BankPayment_BankName");

                            t.Property("BranchCode")
                                .HasColumnName("BankPayment_BranchCode");
                        });

                    b.HasDiscriminator().HasValue(2);
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.CashPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.PaidPayment");

                    b.Property<string>("CashierName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReceiptDate")
                        .HasColumnType("TEXT");


                    b.Property<string>("ReceiptNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(1);
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.CheckPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.PaidPayment");

                    b.Property<string>("BankName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BranchCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CheckNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(4);
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.DebitCardPayment", b =>
                {
                    b.HasBaseType("OgrenciAidatSistemi.Models.PaidPayment");

                    b.Property<string>("CVC")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CardHolderName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ExpiryDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(3);
                });

            modelBuilder.Entity("GradeStudent", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.Grade", null)
                        .WithMany()
                        .HasForeignKey("GradesId")
                    // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.Student", null)
                        .WithMany()
                        .HasForeignKey("StudentsId")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.ContactInfo", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Grade", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany("Grades")
                        .HasForeignKey("SchoolId")
// .OnDelete(DeleteBehavior.Cascade)
;
                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Payment", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.PaymentPeriod", "PaymentPeriod")
                        .WithMany("Payments")
                        .HasForeignKey("PaymentPeriodId")
                        // .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany()
                        .HasForeignKey("SchoolId");

                    b.HasOne("OgrenciAidatSistemi.Models.Student", "Student")
                        .WithMany("Payments")
                        .HasForeignKey("StudentId")
// .OnDelete(DeleteBehavior.SetNull)
;
                    b.Navigation("PaymentPeriod");

                    b.Navigation("School");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.PaymentPeriod", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.Student", "Student")
                        .WithMany("PaymentPeriods")
                        .HasForeignKey("StudentId")
                        // .OnDelete(DeleteBehavior.SetNull)
                        ;

                    b.HasOne("OgrenciAidatSistemi.Models.WorkYear", "WorkYear")
                        .WithMany("PaymentPeriods")
                        .HasForeignKey("WorkYearId")
                        // .OnDelete(DeleteBehavior.SetNull)
                        ;

                    b.Navigation("Student");

                    b.Navigation("WorkYear");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Receipt", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.Payment", "Payment")
                        .WithOne("Receipt")
                        .HasForeignKey("OgrenciAidatSistemi.Models.Receipt", "PaymentId")
// .OnDelete(DeleteBehavior.SetNull);
;

                    b.Navigation("CreatedBy");

                    b.Navigation("Payment");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.WorkYear", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany("WorkYears")
                        .HasForeignKey("SchoolId")
// .OnDelete(DeleteBehavior.Cascade);
;

                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SchoolAdmin", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.ContactInfo", "ContactInfo")
                        .WithMany()
                        .HasForeignKey("ContactInfoId")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.User", null)
                        .WithOne()
                        .HasForeignKey("OgrenciAidatSistemi.Models.SchoolAdmin", "Id")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany("SchoolAdmins")
                        .HasForeignKey("SchoolId")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ContactInfo");

                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.SiteAdmin", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.User", null)
                        .WithOne()
                        .HasForeignKey("OgrenciAidatSistemi.Models.SiteAdmin", "Id")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Student", b =>
                {
                    b.HasOne("OgrenciAidatSistemi.Models.ContactInfo", "ContactInfo")
                        .WithMany()
                        .HasForeignKey("ContactInfoId");

                    b.HasOne("OgrenciAidatSistemi.Models.User", null)
                        .WithOne()
                        .HasForeignKey("OgrenciAidatSistemi.Models.Student", "Id")
                        // .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OgrenciAidatSistemi.Models.School", "School")
                        .WithMany("Students")
                        .HasForeignKey("SchoolId")
// .OnDelete(DeleteBehavior.Cascade);
;

                    b.Navigation("ContactInfo");

                    b.Navigation("School");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Payment", b =>
                {
                    b.Navigation("Receipt");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.PaymentPeriod", b =>
                {
                    b.Navigation("Payments");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.School", b =>
                {
                    b.Navigation("Grades");

                    b.Navigation("SchoolAdmins");

                    b.Navigation("Students");

                    b.Navigation("WorkYears");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.WorkYear", b =>
                {
                    b.Navigation("PaymentPeriods");
                });

            modelBuilder.Entity("OgrenciAidatSistemi.Models.Student", b =>
                {
                    b.Navigation("PaymentPeriods");

                    b.Navigation("Payments");
                });
#pragma warning restore 612, 618
        }
    }
}

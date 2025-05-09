﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OCMS_BOs;

#nullable disable

namespace OCMS_BOs.Migrations
{
    [DbContext(typeof(OCMSDbContext))]
    [Migration("20250221080313_firstdbv1")]
    partial class firstdbv1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OCMS_BOs.Entities.ApprovalLog", b =>
                {
                    b.Property<int>("ApprovalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ApprovalId"));

                    b.Property<string>("ActionDetails")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ActionType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ApprovedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ApprovedUserUserId")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RequestedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestedUserUserId")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("ApprovalId");

                    b.HasIndex("ApprovedUserUserId");

                    b.HasIndex("RequestedUserUserId");

                    b.ToTable("ApprovalLogs");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.AuditLog", b =>
                {
                    b.Property<int>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LogId"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ActionDetails")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LogId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.BackupLog", b =>
                {
                    b.Property<int>("BackupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("BackupId"));

                    b.Property<string>("BackupFilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("BackupTimestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("BackupId");

                    b.HasIndex("CreatedBy");

                    b.ToTable("BackupLogs");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Certificate", b =>
                {
                    b.Property<string>("CertificateId")
                        .HasColumnType("text");

                    b.Property<string>("CertificateTemplateId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DigitalSignature")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("IssueDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RevocationReason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CertificateId");

                    b.HasIndex("CertificateTemplateId");

                    b.HasIndex("CourseId");

                    b.HasIndex("UserId");

                    b.ToTable("Certificates");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CertificateTemplate", b =>
                {
                    b.Property<string>("CertificateTemplateId")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TemplateFile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TemplateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CertificateTemplateId");

                    b.HasIndex("CreatedBy");

                    b.ToTable("CertificatesTemplate");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Course", b =>
                {
                    b.Property<string>("CourseId")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ApprovalDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CourseName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("CourseType")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedByUserUserId")
                        .HasColumnType("text");

                    b.Property<string>("InstructorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Progress")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("CourseId");

                    b.HasIndex("CreatedByUserUserId");

                    b.HasIndex("InstructorId");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CourseChangeRequest", b =>
                {
                    b.Property<string>("ChangeRequestId")
                        .HasColumnType("text");

                    b.Property<string>("ApprovedBy")
                        .HasColumnType("text");

                    b.Property<string>("ApprovedUserUserId")
                        .HasColumnType("text");

                    b.Property<string>("ChangeDetails")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RequestType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RequestedUserUserId")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("ChangeRequestId");

                    b.HasIndex("ApprovedUserUserId");

                    b.HasIndex("CourseId");

                    b.HasIndex("RequestedUserUserId");

                    b.ToTable("CourseChangeRequests");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CourseParticipant", b =>
                {
                    b.Property<string>("ParticipantId")
                        .HasColumnType("text");

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("GradeId")
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ParticipantId");

                    b.HasIndex("CourseId");

                    b.HasIndex("GradeId");

                    b.HasIndex("UserId");

                    b.ToTable("CourseParticipants");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.ExternalCertificate", b =>
                {
                    b.Property<int>("ExternalCertificateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ExternalCertificateId"));

                    b.Property<string>("CertificateFile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ExternalCertificateId");

                    b.HasIndex("UserId");

                    b.ToTable("ExternalCertificates");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Grade", b =>
                {
                    b.Property<string>("GradeId")
                        .HasColumnType("text");

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EvaluationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("GradeValue")
                        .HasColumnType("integer");

                    b.Property<string>("SubmittedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TraineeId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GradeId");

                    b.HasIndex("CourseId");

                    b.HasIndex("SubmittedBy");

                    b.HasIndex("TraineeId");

                    b.ToTable("Grades");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RoleId"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            RoleId = 1,
                            RoleName = "Admin"
                        },
                        new
                        {
                            RoleId = 2,
                            RoleName = "HeadMaster"
                        },
                        new
                        {
                            RoleId = 3,
                            RoleName = "Training staff"
                        },
                        new
                        {
                            RoleId = 4,
                            RoleName = "HR"
                        },
                        new
                        {
                            RoleId = 5,
                            RoleName = "Instructor"
                        },
                        new
                        {
                            RoleId = 6,
                            RoleName = "Reviewer"
                        },
                        new
                        {
                            RoleId = 7,
                            RoleName = "Trainee"
                        });
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Subject", b =>
                {
                    b.Property<string>("SubjectId")
                        .HasColumnType("text");

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ScheduleFile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SubjectName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("SubjectId");

                    b.HasIndex("CourseId");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.TraineeNotification", b =>
                {
                    b.Property<int>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("NotificationId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsRead")
                        .HasColumnType("boolean");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NotificationType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("NotificationId");

                    b.HasIndex("UserId");

                    b.ToTable("TraineeNotifications");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.TraineeProfile", b =>
                {
                    b.Property<string>("TraineeProfileId")
                        .HasColumnType("text");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExternalCertificate")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TraineeProfileId");

                    b.HasIndex("UserId");

                    b.ToTable("TraineeProfiles");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("RoleId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = "ADM-1",
                            CreatedAt = new DateTime(2025, 2, 21, 8, 3, 11, 845, DateTimeKind.Utc).AddTicks(4676),
                            Email = "admin@gmail.com",
                            IsDeleted = false,
                            PasswordHash = "$2a$11$EoSDaFL7/q574IjgRlwWkOUjvS.m6indATZVGJSYgiFlVy9b432.6",
                            RoleId = 1,
                            Status = 1,
                            UpdatedAt = new DateTime(2025, 2, 21, 8, 3, 11, 845, DateTimeKind.Utc).AddTicks(4674),
                            Username = "Admin"
                        });
                });

            modelBuilder.Entity("OCMS_BOs.Entities.ApprovalLog", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "ApprovedUser")
                        .WithMany()
                        .HasForeignKey("ApprovedUserUserId");

                    b.HasOne("OCMS_BOs.Entities.User", "RequestedUser")
                        .WithMany()
                        .HasForeignKey("RequestedUserUserId");

                    b.Navigation("ApprovedUser");

                    b.Navigation("RequestedUser");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.AuditLog", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.BackupLog", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Certificate", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.CertificateTemplate", "CertificateTemplate")
                        .WithMany()
                        .HasForeignKey("CertificateTemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CertificateTemplate");

                    b.Navigation("Course");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CertificateTemplate", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Course", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserUserId");

                    b.HasOne("OCMS_BOs.Entities.User", "Instructor")
                        .WithMany()
                        .HasForeignKey("InstructorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedByUser");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CourseChangeRequest", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "ApprovedUser")
                        .WithMany()
                        .HasForeignKey("ApprovedUserUserId");

                    b.HasOne("OCMS_BOs.Entities.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.User", "RequestedUser")
                        .WithMany()
                        .HasForeignKey("RequestedUserUserId");

                    b.Navigation("ApprovedUser");

                    b.Navigation("Course");

                    b.Navigation("RequestedUser");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.CourseParticipant", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.Grade", "Grade")
                        .WithMany()
                        .HasForeignKey("GradeId");

                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Grade");

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.ExternalCertificate", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Grade", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.User", "Instructor")
                        .WithMany()
                        .HasForeignKey("SubmittedBy")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OCMS_BOs.Entities.User", "Trainee")
                        .WithMany()
                        .HasForeignKey("TraineeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Instructor");

                    b.Navigation("Trainee");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.Subject", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.TraineeNotification", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.TraineeProfile", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OCMS_BOs.Entities.User", b =>
                {
                    b.HasOne("OCMS_BOs.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");
                });
#pragma warning restore 612, 618
        }
    }
}

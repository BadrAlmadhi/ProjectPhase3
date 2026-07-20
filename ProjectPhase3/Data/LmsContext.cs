using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProjectPhase3.Models;

namespace ProjectPhase3.Data;

public partial class LmsContext : DbContext
{
    public LmsContext(DbContextOptions<LmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Assignmentcategory> Assignmentcategories { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("administrators_pkey");

            entity.ToTable("administrators");

            entity.Property(e => e.Userid)
                .ValueGeneratedNever()
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Administrator)
                .HasForeignKey<Administrator>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administrators_userid_fkey");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => new { e.Assignmentid, e.Classid }).HasName("assignments_pkey");

            entity.ToTable("assignments");

            entity.Property(e => e.Assignmentid)
                .ValueGeneratedOnAdd()
                .HasColumnName("assignmentid");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Assignmentname)
                .HasMaxLength(100)
                .HasColumnName("assignmentname");
            entity.Property(e => e.Categorynames)
                .HasMaxLength(100)
                .HasColumnName("categorynames");
            entity.Property(e => e.Content)
                .HasMaxLength(8192)
                .HasColumnName("content");
            entity.Property(e => e.Duedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("duedate");
            entity.Property(e => e.Maxpoint).HasColumnName("maxpoint");

            entity.HasOne(d => d.Assignmentcategory).WithMany(p => p.Assignments)
                .HasForeignKey(d => new { d.Categorynames, d.Classid })
                .HasConstraintName("assignments_categorynames_classid_fkey");
        });

        modelBuilder.Entity<Assignmentcategory>(entity =>
        {
            entity.HasKey(e => new { e.Categorynames, e.Classid }).HasName("assignmentcategories_pkey");

            entity.ToTable("assignmentcategories");

            entity.Property(e => e.Categorynames)
                .HasMaxLength(100)
                .HasColumnName("categorynames");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Gradingweight).HasColumnName("gradingweight");

            entity.HasOne(d => d.Class).WithMany(p => p.Assignmentcategories)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("assignmentcategories_classid_fkey");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Classid).HasName("classes_pkey");

            entity.ToTable("classes");

            entity.HasIndex(e => new { e.Semester, e.Catalogid }, "classes_semester_catalogid_key").IsUnique();

            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Catalogid).HasColumnName("catalogid");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
            entity.Property(e => e.Professorid).HasColumnName("professorid");
            entity.Property(e => e.Semester)
                .HasMaxLength(11)
                .HasColumnName("semester");
            entity.Property(e => e.Starttime).HasColumnName("starttime");

            entity.HasOne(d => d.Catalog).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Catalogid)
                .HasConstraintName("classes_catalogid_fkey");

            entity.HasOne(d => d.Professor).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Professorid)
                .HasConstraintName("classes_professorid_fkey");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Catalogid).HasName("courses_pkey");

            entity.ToTable("courses");

            entity.Property(e => e.Catalogid)
                .ValueGeneratedNever()
                .HasColumnName("catalogid");
            entity.Property(e => e.Coursename)
                .HasMaxLength(100)
                .HasColumnName("coursename");
            entity.Property(e => e.Coursenumber).HasColumnName("coursenumber");
            entity.Property(e => e.Subjectabbreviation)
                .HasMaxLength(4)
                .HasColumnName("subjectabbreviation");

            entity.HasOne(d => d.SubjectabbreviationNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.Subjectabbreviation)
                .HasConstraintName("courses_subjectabbreviation_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Subjectabbreviation).HasName("departments_pkey");

            entity.ToTable("departments");

            entity.Property(e => e.Subjectabbreviation)
                .HasMaxLength(4)
                .HasColumnName("subjectabbreviation");
            entity.Property(e => e.Departmentname)
                .HasMaxLength(100)
                .HasColumnName("departmentname");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Classid }).HasName("enrollments_pkey");

            entity.ToTable("enrollments");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Grade)
                .HasMaxLength(2)
                .HasColumnName("grade");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enrollments_classid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enrollments_userid_fkey");
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("professors_pkey");

            entity.ToTable("professors");

            entity.Property(e => e.Userid)
                .ValueGeneratedNever()
                .HasColumnName("userid");
            entity.Property(e => e.Subjectabbreviation)
                .HasMaxLength(4)
                .HasColumnName("subjectabbreviation");

            entity.HasOne(d => d.SubjectabbreviationNavigation).WithMany(p => p.Professors)
                .HasForeignKey(d => d.Subjectabbreviation)
                .HasConstraintName("professors_subjectabbreviation_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Professor)
                .HasForeignKey<Professor>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("professors_userid_fkey");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Userid)
                .ValueGeneratedNever()
                .HasColumnName("userid");
            entity.Property(e => e.Subjectabbreviation)
                .HasMaxLength(4)
                .HasColumnName("subjectabbreviation");

            entity.HasOne(d => d.SubjectabbreviationNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.Subjectabbreviation)
                .HasConstraintName("students_subjectabbreviation_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_userid_fkey");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => new { e.Assignmentid, e.Userid }).HasName("submissions_pkey");

            entity.ToTable("submissions");

            entity.Property(e => e.Assignmentid).HasColumnName("assignmentid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Submissioncontents)
                .HasMaxLength(8192)
                .HasColumnName("submissioncontents");
            entity.Property(e => e.Submissiontime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("submissiontime");

            entity.HasOne(d => d.User).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submissions_userid_fkey");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => new { d.Assignmentid, d.Classid })
                .HasConstraintName("submissions_assignmentid_classid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

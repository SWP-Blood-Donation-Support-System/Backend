using System;
using System.Collections.Generic;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI;

public partial class BloodDonationSystemContext : DbContext
{
    public BloodDonationSystemContext()
    {
    }

    public BloodDonationSystemContext(DbContextOptions<BloodDonationSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppointmentRecord> AppointmentRecords { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BloodBank> BloodBanks { get; set; }

    public virtual DbSet<BloodDetail> BloodDetails { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Emergency> Emergencies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Hospital> Hospitals { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public virtual DbSet<UserSurveyAnswer> UserSurveyAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentRecord>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC215502E78");

            entity.ToTable("AppointmentRecord");

            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Event).WithMany(p => p.AppointmentRecords)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Appointme__Event__182C9B23");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.AppointmentRecords)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Appointme__Usern__173876EA");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E30286302EC");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogTitle).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Blog__Username__2A4B4B5E");
        });

        modelBuilder.Entity<BloodBank>(entity =>
        {
            entity.HasKey(e => e.BloodType).HasName("PK__BloodBan__33141D171FCDBCEB");

            entity.ToTable("BloodBank");

            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.BloodBankStatus).HasMaxLength(100);
        });

        modelBuilder.Entity<BloodDetail>(entity =>
        {
            entity.HasKey(e => e.BloodDetailId).HasName("PK__BloodDet__BC368EB12D27B809");

            entity.ToTable("BloodDetail");

            entity.Property(e => e.BloodDetailStatus).HasMaxLength(100);
            entity.Property(e => e.BloodType).HasMaxLength(5);

            entity.HasOne(d => d.Appointment).WithMany(p => p.BloodDetails)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__BloodDeta__Appoi__300424B4");

            entity.HasOne(d => d.BloodTypeNavigation).WithMany(p => p.BloodDetails)
                .HasForeignKey(d => d.BloodType)
                .HasConstraintName("FK__BloodDeta__Blood__2F10007B");

            entity.HasOne(d => d.Hospital).WithMany(p => p.BloodDetails)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__BloodDeta__Hospi__30F848ED");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Certific__8ECDFCC21B0907CE");

            entity.ToTable("Certificate");

            entity.Property(e => e.AppointmentId).ValueGeneratedNever();
            entity.Property(e => e.CertificateCode).HasMaxLength(50);

            entity.HasOne(d => d.Appointment).WithOne(p => p.Certificate)
                .HasForeignKey<Certificate>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Appoi__1CF15040");
        });

        modelBuilder.Entity<Emergency>(entity =>
        {
            entity.HasKey(e => e.EmergencyId).HasName("PK__Emergenc__7B5544D307020F21");

            entity.ToTable("Emergency");

            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.EmergencyStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Hospital).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__Emergency__Hospi__09DE7BCC");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Emergency__Usern__08EA5793");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C810117F9D94");

            entity.Property(e => e.EventTitle).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
        });

        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.HospitalId).HasName("PK__Hospital__38C2E5AF7F60ED59");

            entity.ToTable("Hospital");

            entity.Property(e => e.HospitalAddress).HasMaxLength(200);
            entity.Property(e => e.HospitalName).HasMaxLength(100);
            entity.Property(e => e.HospitalPhone).HasMaxLength(20);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E120CBAE877");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationStatus).HasMaxLength(50);
            entity.Property(e => e.NotificationTitle).HasMaxLength(100);

            entity.HasOne(d => d.Emergency).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.EmergencyId)
                .HasConstraintName("FK__Notificat__Emerg__0EA330E9");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.NotificationRecipientId).HasName("PK__Notifica__F6659EE433D4B598");

            entity.ToTable("NotificationRecipient");

            entity.Property(e => e.ResponseDate).HasColumnType("datetime");
            entity.Property(e => e.ResponseStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK__Notificat__Notif__35BCFE0A");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Notificat__Usern__36B12243");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__D5BD4805F0D0B0B5");

            entity.ToTable("Report");

            entity.Property(e => e.ReportContent).HasMaxLength(500);
            entity.Property(e => e.ReportDate).HasColumnType("date");
            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Report__Username__3A81B327");
        });

        modelBuilder.Entity<SurveyQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId);

            entity.ToTable("SurveyQuestion");

            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.QuestionType).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<UserSurveyAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId);

            entity.ToTable("UserSurveyAnswer");

            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AnswerText);
            entity.Property(e => e.AnswerDate).HasColumnType("datetime");

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK_UserSurveyAnswer_User");

            entity.HasOne(d => d.Question)
                .WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_UserSurveyAnswer_SurveyQuestion");

            entity.HasOne(d => d.Option)
                .WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK_UserSurveyAnswer_SurveyOption");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__User__536C85E503317E3D");

            entity.ToTable("User");

            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfileStatus).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

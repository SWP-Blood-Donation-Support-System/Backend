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

    public virtual DbSet<DeferralReason> DeferralReasons { get; set; }

    public virtual DbSet<DonorDeferral> DonorDeferrals { get; set; }

    public virtual DbSet<Emergency> Emergencies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Hospital> Hospitals { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SurveyOption> SurveyOptions { get; set; }

    public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }

    public virtual DbSet<User> Users { get; set; }

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
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7C16A30C649");

            entity.ToTable("Certificate");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CertificateCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.HospitalName).HasMaxLength(255);

            entity.HasOne(d => d.Appointment).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificate_Appointment");
        });

        modelBuilder.Entity<DeferralReason>(entity =>
        {
            entity.HasKey(e => e.ReasonCode).HasName("PK__Deferral__A6278DA24BAC3F29");

            entity.ToTable("DeferralReason");

            entity.Property(e => e.ReasonCode).HasMaxLength(50);
            entity.Property(e => e.ReasonText).HasMaxLength(255);
        });

        modelBuilder.Entity<DonorDeferral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DonorDef__3214EC075070F446");

            entity.ToTable("DonorDeferral");

            entity.Property(e => e.ReasonCode).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.ReasonCodeNavigation).WithMany(p => p.DonorDeferrals)
                .HasForeignKey(d => d.ReasonCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonorDefe__Reaso__5441852A");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.DonorDeferrals)
                .HasForeignKey(d => d.Username)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonorDefe__Usern__534D60F1");
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

            entity.Property(e => e.BloodTypeRequired).HasMaxLength(10);
            entity.Property(e => e.CurrentParticipants).HasDefaultValue(0);
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
            entity.HasKey(e => e.ReportId).HasName("PK__Report__D5BD4805239E4DCF");

            entity.ToTable("Report");

            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Report__Username__25869641");
        });

        modelBuilder.Entity<SurveyOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__SurveyOp__92C7A1FF3D5E1FD2");

            entity.ToTable("SurveyOption");

            entity.Property(e => e.RequireText).HasDefaultValue(false);

            entity.HasOne(d => d.Question).WithMany(p => p.SurveyOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__SurveyOpt__Quest__3F466844");
        });

        modelBuilder.Entity<SurveyQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__SurveyQu__0DC06FAC398D8EEE");

            entity.ToTable("SurveyQuestion");

            entity.Property(e => e.QuestionType).HasMaxLength(20);
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
            entity.Property(e => e.UserStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<UserSurveyAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__UserSurv__D48250044316F928");

            entity.ToTable("UserSurveyAnswer");

            entity.Property(e => e.AnswerDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.UserSurveyAnswers)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__UserSurve__Appoi__44FF419A");

            entity.HasOne(d => d.Option).WithMany(p => p.UserSurveyAnswers)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("FK__UserSurve__Optio__46E78A0C");

            entity.HasOne(d => d.Question).WithMany(p => p.UserSurveyAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__UserSurve__Quest__45F365D3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

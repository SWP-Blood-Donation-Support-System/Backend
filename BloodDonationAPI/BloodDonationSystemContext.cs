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

    public virtual DbSet<AppointmentHistory> AppointmentHistories { get; set; }

    public virtual DbSet<AppointmentList> AppointmentLists { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BloodBank> BloodBanks { get; set; }

    public virtual DbSet<BloodMove> BloodMoves { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<DonationHistory> DonationHistories { get; set; }

    public virtual DbSet<Emergency> Emergencies { get; set; }

    public virtual DbSet<Hospital> Hospitals { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseSqlServer(GetConnectionString());

    //}

    //private string GetConnectionString()
    //{
    //    IConfiguration configuration = new ConfigurationBuilder()
    //        .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json",true ,true).Build();
    //    var strConn = configuration["ConnectionStrings:DefaultConnection"];

    //    return strConn;
    //}   


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentHistory>(entity =>
        {
            entity.HasKey(e => e.AppointmentHistoryId).HasName("PK__Appointm__6795700E36B12243");

            entity.ToTable("AppointmentHistory");

            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentHistories)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Appointme__Appoi__398D8EEE");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.AppointmentHistories)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Appointme__Usern__38996AB5");
        });

        modelBuilder.Entity<AppointmentList>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2182C9B23");

            entity.ToTable("AppointmentList");

            entity.Property(e => e.AppointmentTitle).HasMaxLength(100);
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E302B3F6F97");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogTitle).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Blog__Username__2D27B809");
        });

        modelBuilder.Entity<BloodBank>(entity =>
        {
            entity.HasKey(e => e.BloodTypeId).HasName("PK__BloodBan__B489BA637F60ED59");

            entity.ToTable("BloodBank");

            entity.Property(e => e.BloodTypeId).ValueGeneratedNever();
            entity.Property(e => e.BloodTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<BloodMove>(entity =>
        {
            entity.HasKey(e => e.BloodMoveId).HasName("PK__BloodMov__3A1A9F67300424B4");

            entity.ToTable("BloodMove");

            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodMoves)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__BloodMove__Blood__32E0915F");

            entity.HasOne(d => d.Hospital).WithMany(p => p.BloodMoves)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__BloodMove__Hospi__33D4B598");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.BloodMoves)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__BloodMove__Usern__31EC6D26");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.DonationHistoryId).HasName("PK__Certific__A1E5FD5321B6055D");

            entity.ToTable("Certificate");

            entity.Property(e => e.DonationHistoryId).ValueGeneratedNever();
            entity.Property(e => e.CertificateCode).HasMaxLength(50);

            entity.HasOne(d => d.DonationHistory).WithOne(p => p.Certificate)
                .HasForeignKey<Certificate>(d => d.DonationHistoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Donat__239E4DCF");
        });

        modelBuilder.Entity<DonationHistory>(entity =>
        {
            entity.HasKey(e => e.DonationHistoryId).HasName("PK__Donation__A1E5FD531BFD2C07");

            entity.ToTable("DonationHistory");

            entity.Property(e => e.DonationStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.BloodType).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__DonationH__Blood__1ED998B2");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__DonationH__Usern__1DE57479");
        });

        modelBuilder.Entity<Emergency>(entity =>
        {
            entity.HasKey(e => e.EmergencyId).HasName("PK__Emergenc__7B5544D30BC6C43E");

            entity.ToTable("Emergency");

            entity.Property(e => e.EmergencyStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.BloodType).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__Emergency__Blood__0EA330E9");

            entity.HasOne(d => d.Hospital).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__Emergency__Hospi__0F975522");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Emergency__Usern__0DAF0CB0");
        });

        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.HospitalId).HasName("PK__Hospital__38C2E5AF03317E3D");

            entity.ToTable("Hospital");

            entity.Property(e => e.HospitalId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.EmergencyId).HasName("PK__Notifica__7B5544D31273C1CD");

            entity.ToTable("Notification");

            entity.Property(e => e.EmergencyId).ValueGeneratedNever();
            entity.Property(e => e.NotificationStatus).HasMaxLength(50);
            entity.Property(e => e.NotificationTitle).HasMaxLength(100);

            entity.HasOne(d => d.BloodType).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__Notificat__Blood__15502E78");

            entity.HasOne(d => d.Emergency).WithOne(p => p.Notification)
                .HasForeignKey<Notification>(d => d.EmergencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Emerg__145C0A3F");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.NotificationRecipientId).HasName("PK__Notifica__F6659EE43C69FB99");

            entity.ToTable("NotificationRecipient");

            entity.Property(e => e.ResponseDate).HasColumnType("datetime");
            entity.Property(e => e.ResponseStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Emergency).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.EmergencyId)
                .HasConstraintName("FK__Notificat__Emerg__3E52440B");

            entity.HasOne(d => d.NotificationEmergency).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationEmergencyId)
                .HasConstraintName("FK__Notificat__Notif__3F466844");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Notificat__Usern__403A8C7D");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__D5BD4805267ABA7A");

            entity.ToTable("Report");

            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Report__Username__286302EC");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__User__536C85E507020F21");

            entity.ToTable("User");

            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfileStatus).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(d => d.BloodType).WithMany(p => p.Users)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__User__BloodTypeI__08EA5793");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

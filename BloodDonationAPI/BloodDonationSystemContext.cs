using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Entities;

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
    public IEnumerable<object> BloodDonations { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Blood_Donation_System;User Id=sa;Password=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentHistory>(entity =>
        {
            entity.HasKey(e => e.AppointmentHistoryId).HasName("PK__Appointm__6795700EBFE6FE50");

            entity.ToTable("AppointmentHistory");

            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.AppointmentStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentHistories)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Appointme__Appoi__693CA210");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.AppointmentHistories)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Appointme__Usern__68487DD7");
        });

        modelBuilder.Entity<AppointmentList>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2E3750728");

            entity.ToTable("AppointmentList");

            entity.Property(e => e.AppointmentTitle).HasMaxLength(100);
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E3091246BFD");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogTitle).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Blog__Username__619B8048");
        });

        modelBuilder.Entity<BloodBank>(entity =>
        {
            entity.HasKey(e => e.BloodTypeId).HasName("PK__BloodBan__B489BA63CF330E40");

            entity.ToTable("BloodBank");

            entity.Property(e => e.BloodTypeName).HasMaxLength(50);

            entity.HasOne(d => d.DonationHistory).WithMany(p => p.BloodBanks)
                .HasForeignKey(d => d.DonationHistoryId)
                .HasConstraintName("FK__BloodBank__Donat__5BE2A6F2");
        });

        modelBuilder.Entity<BloodMove>(entity =>
        {
            entity.HasKey(e => e.BloodMoveId).HasName("PK__BloodMov__3A1A9F677A8CCDCE");

            entity.ToTable("BloodMove");

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodMoves)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__BloodMove__Blood__6477ECF3");

            entity.HasOne(d => d.Hospital).WithMany(p => p.BloodMoves)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__BloodMove__Hospi__656C112C");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.DonationHistoryId).HasName("PK__Certific__A1E5FD5317E60852");

            entity.ToTable("Certificate");

            entity.Property(e => e.DonationHistoryId).ValueGeneratedNever();
            entity.Property(e => e.CertificateCode).HasMaxLength(50);

            entity.HasOne(d => d.DonationHistory).WithOne(p => p.Certificate)
                .HasForeignKey<Certificate>(d => d.DonationHistoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Donat__59063A47");
        });

        modelBuilder.Entity<DonationHistory>(entity =>
        {
            entity.HasKey(e => e.DonationHistoryId).HasName("PK__Donation__A1E5FD53939A9178");

            entity.ToTable("DonationHistory");

            entity.Property(e => e.BloodType).HasMaxLength(1);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__DonationH__Usern__5629CD9C");
        });

        modelBuilder.Entity<Emergency>(entity =>
        {
            entity.HasKey(e => e.EmergencyId).HasName("PK__Emergenc__7B5544D33E261FC0");

            entity.ToTable("Emergency");

            entity.Property(e => e.BloodType)
                .HasMaxLength(5)
                .HasColumnName("bloodType");
            entity.Property(e => e.EmergencyStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Hospital).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.HospitalId)
                .HasConstraintName("FK__Emergency__Hospi__4E88ABD4");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Emergencies)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Emergency__Usern__4D94879B");
        });

        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.HospitalId).HasName("PK__Hospital__38C2E5AFEB8AD806");

            entity.ToTable("Hospital");

            entity.Property(e => e.HospitalAddress).HasMaxLength(200);
            entity.Property(e => e.HospitalName).HasMaxLength(100);
            entity.Property(e => e.HospitalPhone).HasMaxLength(20);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12311982A4");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationStatus).HasMaxLength(50);
            entity.Property(e => e.NotificationTitle).HasMaxLength(100);

            entity.HasOne(d => d.Emergency).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.EmergencyId)
                .HasConstraintName("FK__Notificat__Emerg__5165187F");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.NotificationRecipientId).HasName("PK__Notifica__F6659EE40C7CF71F");

            entity.ToTable("NotificationRecipient");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.ResponseDate).HasColumnType("datetime");
            entity.Property(e => e.ResponseStatus).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK__Notificat__Notif__6C190EBB");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Notificat__Usern__6D0D32F4");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__D5BD48057F97A342");

            entity.ToTable("Report");

            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__Report__Username__5EBF139D");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__User__536C85E5537BFC5F");

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

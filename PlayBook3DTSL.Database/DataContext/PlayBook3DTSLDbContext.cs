using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlayBook3DTSL.Database.Entities;

namespace PlayBook3DTSL.Database.DataContext;

public partial class PlayBook3DTSLDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public PlayBook3DTSLDbContext()
    {
    }

    public PlayBook3DTSLDbContext(DbContextOptions<PlayBook3DTSLDbContext> options)
        : base(options)
    {
    }

    public PlayBook3DTSLDbContext(DbContextOptions<PlayBook3DTSLDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<CaseConfiguration> CaseConfigurations { get; set; }

    public virtual DbSet<CaseImage> CaseImages { get; set; }

    public virtual DbSet<CaseMeasurementStep> CaseMeasurementSteps { get; set; }
    public virtual DbSet<CaseMeasurementSubSteps> CaseMeasurementSubSteps { get; set; }

    public virtual DbSet<CaseResult> CaseResults { get; set; }

    public virtual DbSet<CaseStudyMapping> CaseStudyMappings { get; set; }

    public virtual DbSet<AzureCommandEndpoint> AzureCommandEndpoints { get; set; } = null!;

    public virtual DbSet<ResultCaseImage> ResultCaseImages { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=VLP127;Initial Catalog=PlayBook3DTSL;Integrated Security=false;User Id=sa;Password=Password12@;Command Timeout=300;TrustServerCertificate=True;");

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cases__3214EC27FD0B2CC1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CaseId).ValueGeneratedOnAdd();
            entity.Property(e => e.CaseName).HasMaxLength(255);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.Period).HasMaxLength(255);
        });

        modelBuilder.Entity<CaseConfiguration>(entity =>
        {
            entity.ToTable("CaseConfiguration");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CaseResultImageDetail).HasDefaultValue("");
            entity.Property(e => e.CaseResultRotateState).HasDefaultValue("");
            entity.Property(e => e.CaseResultToolState).HasDefaultValue("");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.ImageName).HasDefaultValue("");
            entity.Property(e => e.ImageViewType).HasMaxLength(255);
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<CaseImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CaseImag__3214EC0742E267C0");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.PixelSpacing).HasMaxLength(250);
            entity.Property(e => e.SeriesInstanceUid)
                .HasMaxLength(250)
                .HasColumnName("SeriesInstanceUID");
            entity.Property(e => e.SeriesName).HasMaxLength(500);
            entity.Property(e => e.SopinstanceUid)
                .HasMaxLength(250)
                .HasColumnName("SOPInstanceUID");
            entity.Property(e => e.StudyDate).HasColumnType("datetime");
            entity.Property(e => e.StudyDescription).HasMaxLength(250);
            entity.Property(e => e.StudyInstanceUid)
                .HasMaxLength(250)
                .HasColumnName("StudyInstanceUID");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseImages)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_CaseImages_Cases");
        });

        modelBuilder.Entity<CaseMeasurementStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CaseMeas__3214EC0715502E78");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ImageViewType).HasMaxLength(255);
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.StepName).HasMaxLength(255);

            entity.HasOne(d => d.Case).WithMany(p => p.CaseMeasurementSteps)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CaseMeasurementSteps_Cases");
        });

        modelBuilder.Entity<CaseMeasurementSubSteps>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.StepName).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.CaseMeasurementStep)
                  .WithMany(p => p.SubSteps)
                  .HasForeignKey(d => d.CaseMeasurementStepsId)
                  .HasConstraintName("FK_CaseMeasurementSubSteps_CaseMeasurementSteps");
        });

        modelBuilder.Entity<CaseResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CaseResu__3214EC07FA048AD1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AppixelSize)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("APPixelSize");
            entity.Property(e => e.Cilmm)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CILmm");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cslt1l1)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CSLT1L1");
            entity.Property(e => e.CspineLength)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CSpineLength");
            entity.Property(e => e.Ct1s1height)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CT1S1height");
            entity.Property(e => e.Ct1t12height)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CT1T12height");
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.LateralPixelSize).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Lil)
                .HasMaxLength(255)
                .HasColumnName("LIL");
            entity.Property(e => e.Period).HasMaxLength(255);
            entity.Property(e => e.Silmm)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SILmm");
            entity.Property(e => e.Sslt1l1)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SSLT1L1");
            entity.Property(e => e.SspineLength)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SSpineLength");
            entity.Property(e => e.St1s1height)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ST1S1height");
            entity.Property(e => e.St1t12height)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ST1T12height");
            entity.Property(e => e.T1l13dtsl)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("T1L13DTSL");
            entity.Property(e => e.ThreeDitsl)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ThreeDITSL");
            entity.Property(e => e.ThreeDtsl)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ThreeDTSL");
            entity.Property(e => e.Uil)
                .HasMaxLength(255)
                .HasColumnName("UIL");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseResults)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_CaseResults_Cases");
        });

        modelBuilder.Entity<CaseStudyMapping>(entity =>
        {
            entity.ToTable("CaseStudyMapping");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasMaxLength(255);
            entity.Property(e => e.StudyId).HasMaxLength(255);

            entity.HasOne(d => d.Case).WithMany(p => p.CaseStudyMappings)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaseStudyMapping_Cases");
        });

        modelBuilder.Entity<ResultCaseImage>(entity =>
        {
            entity.HasIndex(e => e.CaseId, "UQ__ResultCa__6CAE524DE51986F9").IsUnique();

            entity.HasIndex(e => e.CaseResultId, "UQ__ResultCa__E058ADD2D9F6F297").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageViewType).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    #region Call DB's Stored Procedure 

    public List<T> GetResultFromStoredProc<T>(string query, DynamicParameters sp_params, CommandType commandType = CommandType.StoredProcedure)
    {
        if (_configuration == null)
            return new List<T>();

        using IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        return db.Query<T>(query, sp_params, commandType: commandType).ToList();
    }

    #endregion
}

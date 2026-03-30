using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DTPortal.Core.Domain.Models;

public partial class idp_dtplatformContext : DbContext
{
    public idp_dtplatformContext()
    {
    }

    public idp_dtplatformContext(DbContextOptions<idp_dtplatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<IpBasedAccess> IpBasedAccesses { get; set; }

    public virtual DbSet<MakerChecker> MakerCheckers { get; set; }

    public virtual DbSet<OperationsAuthscheme> OperationsAuthschemes { get; set; }

    public virtual DbSet<PasswordPolicy> PasswordPolicies { get; set; }

    public virtual DbSet<PortalSetting> PortalSettings { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleActivity> RoleActivities { get; set; }

    public virtual DbSet<Smtp> Smtps { get; set; }

    public virtual DbSet<TimeBasedAccess> TimeBasedAccesses { get; set; }

    public virtual DbSet<UserAuthDatum> UserAuthData { get; set; }

    public virtual DbSet<UserLoginDetail> UserLoginDetails { get; set; }

    public virtual DbSet<UserTable> UserTables { get; set; }

     
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("activities_pkey");

            entity.ToTable("activities");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(260)
                .HasColumnName("display_name");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasColumnName("hash");
            entity.Property(e => e.IsCritical).HasColumnName("is_critical");
            entity.Property(e => e.McEnabled).HasColumnName("mc_enabled");
            entity.Property(e => e.McSupported).HasColumnName("mc_supported");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("configuration_pkey");

            entity.ToTable("configuration");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasColumnName("hash");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<IpBasedAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ip_based_access_pkey");

            entity.ToTable("ip_based_access");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("description");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.Ip)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("ip");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("name");
            entity.Property(e => e.Permission).HasColumnName("permission");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("type");
        });

        modelBuilder.Entity<MakerChecker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maker_checker_pkey");

            entity.ToTable("maker_checker");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.MakerId).HasColumnName("maker_id");
            entity.Property(e => e.MakerRoleId).HasColumnName("maker_role_id");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.OperationPriority)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("operation_priority");
            entity.Property(e => e.OperationType)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("operation_type");
            entity.Property(e => e.RequestData)
                .IsRequired()
                .HasColumnName("request_data");
            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("state");

            entity.HasOne(d => d.Activity).WithMany(p => p.MakerCheckers)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity");

            entity.HasOne(d => d.Maker).WithMany(p => p.MakerCheckers)
                .HasForeignKey(d => d.MakerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_maker");
        });

        modelBuilder.Entity<OperationsAuthscheme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("operations_authscheme_pkey");

            entity.ToTable("operations_authscheme");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthenticationRequired).HasColumnName("authentication_required");
            entity.Property(e => e.AuthenticationSchemeName)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("authentication_scheme_name");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("description");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("display_name");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.OperationName)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("operation_name");
        });

        modelBuilder.Entity<PasswordPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_policy_pkey");

            entity.ToTable("password_policy");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BadPwdCount).HasColumnName("bad_pwd_count");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.IsReversibleEncryption).HasColumnName("is_reversible_encryption");
            entity.Property(e => e.MaximumPwdAge).HasColumnName("maximum_pwd_age");
            entity.Property(e => e.MaximumPwdLength).HasColumnName("maximum_pwd_length");
            entity.Property(e => e.MinimumPwdAge).HasColumnName("minimum_pwd_age");
            entity.Property(e => e.MinimumPwdLength).HasColumnName("minimum_pwd_length");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.PasswordHistory).HasColumnName("password_history");
            entity.Property(e => e.PwdContains).HasColumnName("pwd_contains");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PasswordPolicyCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("fk_created_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PasswordPolicyUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("fk_updated_by");
        });

        modelBuilder.Entity<PortalSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("portal_settings_pkey");

            entity.ToTable("portal_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("updated_by");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnName("value");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "role_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("display_name");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<RoleActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_activity_pkey");

            entity.ToTable("role_activity");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.GeoLocCoordinates).HasColumnName("geo_loc_coordinates");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.IsChecker).HasColumnName("is_checker");
            entity.Property(e => e.IsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("is_enabled");
            entity.Property(e => e.LocationOnlyAccess).HasColumnName("location_only_access");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.NativeAccess).HasColumnName("native_access");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.WebAccess).HasColumnName("web_access");

            entity.HasOne(d => d.Activity).WithMany(p => p.RoleActivities)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleActivities)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_role");
        });

        modelBuilder.Entity<Smtp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("smtp_pkey");

            entity.ToTable("smtp");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FromEmailAddr)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("from_email_addr");
            entity.Property(e => e.FromName)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("from_name");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.MailSubject)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("mail_subject");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.RequireAuth).HasColumnName("require_auth");
            entity.Property(e => e.RequiresSsl).HasColumnName("requires_ssl");
            entity.Property(e => e.SmtpHost)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("smtp_host");
            entity.Property(e => e.SmtpPort).HasColumnName("smtp_port");
            entity.Property(e => e.SmtpPwd)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("smtp_pwd");
            entity.Property(e => e.SmtpUserName)
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnName("smtp_user_name");
            entity.Property(e => e.Template)
                .IsRequired()
                .HasColumnName("template");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<TimeBasedAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("time_based_access_pkey");

            entity.ToTable("time_based_access");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessDenyTimeZone)
                .HasMaxLength(10)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("access_deny_time_zone");
            entity.Property(e => e.ApplicableRoles)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("applicable_roles");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Deny).HasColumnName("deny");
            entity.Property(e => e.Description)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("modified_by");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("status");
        });

        modelBuilder.Entity<UserAuthDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_auth_data_pkey");

            entity.ToTable("user_auth_data");

            entity.HasIndex(e => e.UserId, "idx_user_auth_data_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthData)
                .IsRequired()
                .HasColumnName("auth_data");
            entity.Property(e => e.AuthScheme)
                .HasMaxLength(50)
                .HasColumnName("auth_scheme");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Expiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiry");
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");
            entity.Property(e => e.Istemporary).HasColumnName("istemporary");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<UserLoginDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_login_details_pkey");

            entity.ToTable("user_login_details");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BadLoginTime)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("bad_login_time");
            entity.Property(e => e.DeniedCount)
                .HasDefaultValue(0)
                .HasColumnName("denied_count");
            entity.Property(e => e.IsReversibleEncryption).HasColumnName("is_reversible_encryption");
            entity.Property(e => e.IsScrambled).HasColumnName("is_scrambled");
            entity.Property(e => e.LastAuthData)
                .HasMaxLength(360)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("last_auth_data");
            entity.Property(e => e.PriAuthSchId).HasColumnName("pri_auth_sch_id");
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(36)
                .HasColumnName("user_id");
            entity.Property(e => e.WrongCodeCount)
                .HasDefaultValue(0)
                .HasColumnName("wrong_code_count");
            entity.Property(e => e.WrongPinCount)
                .HasDefaultValue(0)
                .HasColumnName("wrong_pin_count");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_table_pkey");

            entity.ToTable("user_table");

            entity.HasIndex(e => e.MailId, "user_table_mail_id_key").IsUnique();

            entity.HasIndex(e => e.Uuid, "user_table_uuid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthData)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("auth_data");
            entity.Property(e => e.AuthScheme)
                .HasMaxLength(50)
                .HasDefaultValueSql("'DEFAULT'::character varying")
                .HasColumnName("auth_scheme");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.CurrentLoginTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("current_login_time");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Hash)
                .HasMaxLength(260)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("hash");
            entity.Property(e => e.LastLoginTime)
                 .HasColumnType("timestamp with time zone")
                 .HasColumnName("last_login_time");
            entity.Property(e => e.LockedTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("locked_time");
            entity.Property(e => e.MailId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("mail_id");
            entity.Property(e => e.MobileNo)
                .HasMaxLength(20)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("mobile_no");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_date");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("old_status");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.Uuid)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("uuid");

            entity.HasOne(d => d.Role).WithMany(p => p.UserTables)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_role");
        });
        modelBuilder.HasSequence<int>("configuration_id_seq");
        modelBuilder.HasSequence<int>("kyc_method_types_id_seq");
        modelBuilder.HasSequence<int>("kyc_segments_id_seq");
        modelBuilder.HasSequence<int>("organization_verification_methods_id_seq");
        modelBuilder.HasSequence<int>("verification_methods_id_seq");
        modelBuilder.HasSequence<int>("wallet_consent_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

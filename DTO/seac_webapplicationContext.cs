using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Nri_Webapplication_Backend.DTO
{
    public partial class seac_webapplicationContext : DbContext
    {
        public seac_webapplicationContext()
        {
        }

        public seac_webapplicationContext(DbContextOptions<seac_webapplicationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CertifiedTrainer> CertifiedTrainer { get; set; }
        public virtual DbSet<ContentBusinessType> ContentBusinessType { get; set; }
        public virtual DbSet<ContentFormatMaster> ContentFormatMaster { get; set; }
        public virtual DbSet<ContentFormatVariety> ContentFormatVariety { get; set; }
        public virtual DbSet<ContentMaster> ContentMaster { get; set; }
        public virtual DbSet<EmailLog> EmailLog { get; set; }
        public virtual DbSet<EventCategory> EventCategory { get; set; }
        public virtual DbSet<EventRoomMaster> EventRoomMaster { get; set; }
        public virtual DbSet<EventRoomType> EventRoomType { get; set; }
        public virtual DbSet<EventSchedule> EventSchedule { get; set; }
        public virtual DbSet<EventScheduleLogs> EventScheduleLogs { get; set; }
        public virtual DbSet<EventScheduleModerator> EventScheduleModerator { get; set; }
        public virtual DbSet<EventScheduleTrainer> EventScheduleTrainer { get; set; }
        public virtual DbSet<Language> Language { get; set; }
        public virtual DbSet<LearningTypeMappingForinstancy> LearningTypeMappingForinstancy { get; set; }
        public virtual DbSet<LearningTypeMaster> LearningTypeMaster { get; set; }
        public virtual DbSet<ModeratorSkill> ModeratorSkill { get; set; }
        public virtual DbSet<ModeratorSupportBusinessType> ModeratorSupportBusinessType { get; set; }
        public virtual DbSet<ModeratorSupportRoomType> ModeratorSupportRoomType { get; set; }
        public virtual DbSet<ScheduleAcceptedLog> ScheduleAcceptedLog { get; set; }
        public virtual DbSet<Skill> Skill { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<TimeZone> TimeZone { get; set; }
        public virtual DbSet<TrainerContractType> TrainerContractType { get; set; }
        public virtual DbSet<TrainerMaster> TrainerMaster { get; set; }
        public virtual DbSet<TrainerType> TrainerType { get; set; }
        public virtual DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UsersRole> UsersRole { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()

                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                 .AddJsonFile("appsettings.json")
                 .Build();
                optionsBuilder.UseMySQL(configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CertifiedTrainer>(entity =>
            {
                entity.ToTable("certified_trainer");

                entity.Property(e => e.CertifiedTrainerId).HasColumnName("CertifiedTrainerID");

                entity.Property(e => e.ContentId).HasColumnName("ContentID");

                entity.Property(e => e.TrainerId).HasColumnName("TrainerID");
            });

            modelBuilder.Entity<ContentBusinessType>(entity =>
            {
                entity.ToTable("content_business_type");

                entity.Property(e => e.ContentBusinessTypeId).HasColumnName("ContentBusinessTypeID");

                entity.Property(e => e.ContentBusinessTypeAbbreviate)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContentBusinessTypeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ContentFormatMaster>(entity =>
            {
                entity.HasKey(e => e.ContentFormatId)
                    .HasName("PRIMARY");

                entity.ToTable("content_format_master");

                entity.Property(e => e.ContentFormatId).HasColumnName("ContentFormatID");

                entity.Property(e => e.ContentFormatName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ContentFormatVariety>(entity =>
            {
                entity.ToTable("content_format_variety");

                entity.HasIndex(e => e.ContentFormatId)
                    .HasName("ContentFormatID");

                entity.HasIndex(e => e.ContentId)
                    .HasName("ContentID");

                entity.Property(e => e.ContentFormatVarietyId).HasColumnName("ContentFormatVarietyID");

                entity.Property(e => e.ContentFormatId).HasColumnName("ContentFormatID");

                entity.Property(e => e.ContentId).HasColumnName("ContentID");

                entity.HasOne(d => d.ContentFormat)
                    .WithMany(p => p.ContentFormatVariety)
                    .HasForeignKey(d => d.ContentFormatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("content_format_variety_ibfk_2");

                entity.HasOne(d => d.Content)
                    .WithMany(p => p.ContentFormatVariety)
                    .HasForeignKey(d => d.ContentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("content_format_variety_ibfk_1");
            });

            modelBuilder.Entity<ContentMaster>(entity =>
            {
                entity.HasKey(e => e.ContentId)
                    .HasName("PRIMARY");

                entity.ToTable("content_master");

                entity.HasIndex(e => e.ContentBusinessTypeId)
                    .HasName("ContentBusinessTypeID");

                entity.HasIndex(e => e.LearningTypeId)
                    .HasName("LearningTypeID");

                entity.Property(e => e.ContentId).HasColumnName("ContentID");

                entity.Property(e => e.ContentBusinessTypeId).HasColumnName("ContentBusinessTypeID");

                entity.Property(e => e.ContentCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContentName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Duration).HasColumnType("decimal(4,0)");

                entity.Property(e => e.LearningTypeId).HasColumnName("LearningTypeID");

                entity.Property(e => e.OriginalContentName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContentBusinessType)
                    .WithMany(p => p.ContentMaster)
                    .HasForeignKey(d => d.ContentBusinessTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("content_master_ibfk_2");

                entity.HasOne(d => d.LearningType)
                    .WithMany(p => p.ContentMaster)
                    .HasForeignKey(d => d.LearningTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("content_master_ibfk_1");
            });

            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.ToTable("email_log");

                entity.HasIndex(e => e.Id)
                    .HasName("Id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.TrainerId).HasColumnName("TrainerID");

                entity.Property(e => e.Uuid)
                    .HasColumnName("UUID")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.ToTable("event_category");

                entity.Property(e => e.EventCategoryId).HasColumnName("EventCategoryID");

                entity.Property(e => e.EventCategoryName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EventRoomMaster>(entity =>
            {
                entity.HasKey(e => e.EventRoomId)
                    .HasName("PRIMARY");

                entity.ToTable("event_room_master");

                entity.HasIndex(e => e.ContentFormatId)
                    .HasName("ContentFormatID");

                entity.HasIndex(e => e.EventRoomTypeId)
                    .HasName("EventRoomTypeID");

                entity.Property(e => e.EventRoomId).HasColumnName("EventRoomID");

                entity.Property(e => e.ContentFormatId).HasColumnName("ContentFormatID");

                entity.Property(e => e.EventRoomName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EventRoomTypeId).HasColumnName("EventRoomTypeID");

                entity.Property(e => e.InstancyDisplayName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.InstancyLocationId)
                    .HasColumnName("InstancyLocationID")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContentFormat)
                    .WithMany(p => p.EventRoomMaster)
                    .HasForeignKey(d => d.ContentFormatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_room_master_ibfk_2");

                entity.HasOne(d => d.EventRoomType)
                    .WithMany(p => p.EventRoomMaster)
                    .HasForeignKey(d => d.EventRoomTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_room_master_ibfk_1");
            });

            modelBuilder.Entity<EventRoomType>(entity =>
            {
                entity.ToTable("event_room_type");

                entity.Property(e => e.EventRoomTypeId).HasColumnName("EventRoomTypeID");

                entity.Property(e => e.EventRoomTypeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EventSchedule>(entity =>
            {
                entity.ToTable("event_schedule");

                entity.HasIndex(e => e.ContentBusinessTypeId)
                    .HasName("ContentBusinessTypeID");

                entity.HasIndex(e => e.ContentFormatId)
                    .HasName("ContentFormatID");

                entity.HasIndex(e => e.ContentId)
                    .HasName("ContentID");

                entity.HasIndex(e => e.EventCategoryId)
                    .HasName("EventCategoryID");

                entity.HasIndex(e => e.EventRoomId)
                    .HasName("EventRoomID");

                entity.HasIndex(e => e.LanguageId)
                    .HasName("LanguageID");

                entity.HasIndex(e => e.LearningTypeId)
                    .HasName("LearningTypeID");

                entity.HasIndex(e => e.StatusId)
                    .HasName("StatusID");

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.BookBy)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Company)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContentBusinessTypeId).HasColumnName("ContentBusinessTypeID");

                entity.Property(e => e.ContentFormatId).HasColumnName("ContentFormatID");

                entity.Property(e => e.ContentId).HasColumnName("ContentID");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EventCategoryId).HasColumnName("EventCategoryID");

                entity.Property(e => e.EventRoomId).HasColumnName("EventRoomID");

                entity.Property(e => e.LanguageId).HasColumnName("LanguageID");

                entity.Property(e => e.LearningTypeId).HasColumnName("LearningTypeID");

                entity.Property(e => e.IsModerator).HasColumnName("isMod");

                entity.Property(e => e.Link)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Location)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProjectName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Session)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.UploadToInstancyDate).HasColumnType("date");

                entity.HasOne(d => d.ContentBusinessType)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.ContentBusinessTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_7");

                entity.HasOne(d => d.ContentFormat)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.ContentFormatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_5");

                entity.HasOne(d => d.Content)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.ContentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_1");

                entity.HasOne(d => d.EventCategory)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.EventCategoryId)
                    .HasConstraintName("event_schedule_ibfk_3");

                entity.HasOne(d => d.EventRoom)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.EventRoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_2");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_6");

                entity.HasOne(d => d.LearningType)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.LearningTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_ibfk_4");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.EventSchedule)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("event_schedule_ibfk_8");
            });

            modelBuilder.Entity<EventScheduleLogs>(entity =>
            {
                entity.ToTable("event_schedule_logs");

                entity.Property(e => e.EventScheduleLogsId).HasColumnName("EventScheduleLogsID");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContentId).HasColumnName("ContentID");

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.Field)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FromValue)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.ToValue)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<EventScheduleModerator>(entity =>
            {
                entity.ToTable("event_schedule_moderator");

                entity.HasIndex(e => e.EventScheduleId)
                    .HasName("event_schedule_moderator_ibfk_1");

                entity.HasIndex(e => e.ModeratorId)
                    .HasName("ModeratorID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.ModeratorId).HasColumnName("ModeratorID");

                entity.HasOne(d => d.EventSchedule)
                    .WithMany(p => p.EventScheduleModerator)
                    .HasForeignKey(d => d.EventScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_moderator_ibfk_1");

                entity.HasOne(d => d.Moderator)
                    .WithMany(p => p.EventScheduleModerator)
                    .HasForeignKey(d => d.ModeratorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_moderator_ibfk_2");
            });

            modelBuilder.Entity<EventScheduleTrainer>(entity =>
            {
                entity.ToTable("event_schedule_trainer");

                entity.HasIndex(e => e.EventScheduleId)
                    .HasName("EventScheduleID");

                entity.HasIndex(e => e.TrainerId)
                    .HasName("TrainerID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.IsTrainerReply).HasColumnName("isTrainerReply");

                entity.Property(e => e.TrainerId).HasColumnName("TrainerID");

                entity.HasOne(d => d.EventSchedule)
                    .WithMany(p => p.EventScheduleTrainer)
                    .HasForeignKey(d => d.EventScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_trainer_ibfk_1");

                entity.HasOne(d => d.Trainer)
                    .WithMany(p => p.EventScheduleTrainer)
                    .HasForeignKey(d => d.TrainerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("event_schedule_trainer_ibfk_2");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("language");

                entity.Property(e => e.LanguageId).HasColumnName("LanguageID");

                entity.Property(e => e.LanguageName)
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LearningTypeMappingForinstancy>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("learning_type_mapping_forinstancy");

                entity.HasIndex(e => e.ContentFormatId)
                    .HasName("ContentFormatID");

                entity.HasIndex(e => e.LearningTypeId)
                    .HasName("LearningTypeID");

                entity.Property(e => e.ContentFormatId).HasColumnName("ContentFormatID");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LearningTypeId).HasColumnName("LearningTypeID");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContentFormat)
                    .WithMany()
                    .HasForeignKey(d => d.ContentFormatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("learning_type_mapping_forinstancy_ibfk_2");

                entity.HasOne(d => d.LearningType)
                    .WithMany()
                    .HasForeignKey(d => d.LearningTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("learning_type_mapping_forinstancy_ibfk_1");
            });

            modelBuilder.Entity<LearningTypeMaster>(entity =>
            {
                entity.HasKey(e => e.LearningTypeId)
                    .HasName("PRIMARY");

                entity.ToTable("learning_type_master");

                entity.Property(e => e.LearningTypeId).HasColumnName("LearningTypeID");

                entity.Property(e => e.LearningTypeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ModeratorSkill>(entity =>
            {
                entity.ToTable("moderator_skill");

                entity.HasIndex(e => e.ModeratorId)
                    .HasName("ModeratorID");

                entity.HasIndex(e => e.SkillId)
                    .HasName("SkillID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ModeratorId).HasColumnName("ModeratorID");

                entity.Property(e => e.SkillId).HasColumnName("SkillID");

                entity.HasOne(d => d.Moderator)
                    .WithMany(p => p.ModeratorSkill)
                    .HasForeignKey(d => d.ModeratorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_skill_ibfk_1");

                entity.HasOne(d => d.Skill)
                    .WithMany(p => p.ModeratorSkill)
                    .HasForeignKey(d => d.SkillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_skill_ibfk_2");
            });

            modelBuilder.Entity<ModeratorSupportBusinessType>(entity =>
            {
                entity.ToTable("moderator_support_business_type");

                entity.HasIndex(e => e.ContentBusinessTypeId)
                    .HasName("ContentBusinessTypeID");

                entity.HasIndex(e => e.ModeratorId)
                    .HasName("ModeratorID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ContentBusinessTypeId).HasColumnName("ContentBusinessTypeID");

                entity.Property(e => e.ModeratorId).HasColumnName("ModeratorID");

                entity.HasOne(d => d.ContentBusinessType)
                    .WithMany(p => p.ModeratorSupportBusinessType)
                    .HasForeignKey(d => d.ContentBusinessTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_support_business_type_ibfk_1");

                entity.HasOne(d => d.Moderator)
                    .WithMany(p => p.ModeratorSupportBusinessType)
                    .HasForeignKey(d => d.ModeratorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_support_business_type_ibfk_2");
            });

            modelBuilder.Entity<ModeratorSupportRoomType>(entity =>
            {
                entity.ToTable("moderator_support_room_type");

                entity.HasIndex(e => e.EventRoomTypeId)
                    .HasName("EventRoomTypeID");

                entity.HasIndex(e => e.ModeratorId)
                    .HasName("ModeratorID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EventRoomTypeId).HasColumnName("EventRoomTypeID");

                entity.Property(e => e.ModeratorId).HasColumnName("ModeratorID");

                entity.HasOne(d => d.EventRoomType)
                    .WithMany(p => p.ModeratorSupportRoomType)
                    .HasForeignKey(d => d.EventRoomTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_support_room_type_ibfk_2");

                entity.HasOne(d => d.Moderator)
                    .WithMany(p => p.ModeratorSupportRoomType)
                    .HasForeignKey(d => d.ModeratorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("moderator_support_room_type_ibfk_1");
            });

            modelBuilder.Entity<ScheduleAcceptedLog>(entity =>
            {
                entity.HasKey(e => e.ScheduleAcceptedId)
                    .HasName("PRIMARY");

                entity.ToTable("schedule_accepted_log");

                entity.Property(e => e.ScheduleAcceptedId).HasColumnName("ScheduleAcceptedID");

                entity.Property(e => e.EventScheduleId).HasColumnName("EventScheduleID");

                entity.Property(e => e.TrainerId).HasColumnName("TrainerID");

                entity.Property(e => e.Uuid)
                    .IsRequired()
                    .HasColumnName("UUID")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Skill>(entity =>
            {
                entity.ToTable("skill");

                entity.Property(e => e.SkillId).HasColumnName("SkillID");

                entity.Property(e => e.SkillName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("status");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.Status1)
                    .HasColumnName("Status")
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TimeZone>(entity =>
            {
                entity.ToTable("time_zone");

                entity.Property(e => e.TimeZoneId).HasColumnName("TimeZoneID");

                entity.Property(e => e.TimeZoneFullName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TimeZoneName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TrainerContractType>(entity =>
            {
                entity.HasKey(e => e.TraninerContractTypeId)
                    .HasName("PRIMARY");

                entity.ToTable("trainer_contract_type");

                entity.Property(e => e.TraninerContractTypeId).HasColumnName("TraninerContractTypeID");

                entity.Property(e => e.TraninerContractTypeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TrainerMaster>(entity =>
            {
                entity.HasKey(e => e.TrainerId)
                    .HasName("PRIMARY");

                entity.ToTable("trainer_master");

                entity.HasIndex(e => e.TrainerTypeId)
                    .HasName("TrainerTypeID");

                entity.HasIndex(e => e.TraninerContractTypeId)
                    .HasName("TraninerContractTypeID");

                entity.Property(e => e.TrainerId).HasColumnName("TrainerID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ICalendarLink)
                    .HasColumnName("iCalendarLink")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NickNameEn)
                    .HasColumnName("NickNameEN")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NickNameTh)
                    .HasColumnName("NickNameTH")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Telephone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TrainerTypeId).HasColumnName("TrainerTypeID");

                entity.Property(e => e.TraninerContractTypeId).HasColumnName("TraninerContractTypeID");

                entity.HasOne(d => d.TrainerType)
                    .WithMany(p => p.TrainerMaster)
                    .HasForeignKey(d => d.TrainerTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trainer_master_ibfk_1");

                entity.HasOne(d => d.TraninerContractType)
                    .WithMany(p => p.TrainerMaster)
                    .HasForeignKey(d => d.TraninerContractTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trainer_master_ibfk_2");
            });

            modelBuilder.Entity<TrainerType>(entity =>
            {
                entity.ToTable("trainer_type");

                entity.Property(e => e.TrainerTypeId).HasColumnName("TrainerTypeID");

                entity.Property(e => e.TypeName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserLoginHistory>(entity =>
            {
                entity.HasKey(e => e.UserLogInHistory1)
                    .HasName("PRIMARY");

                entity.ToTable("user_login_history");

                entity.HasIndex(e => e.UserId)
                    .HasName("UserID");

                entity.Property(e => e.UserLogInHistory1).HasColumnName("UserLogInHistory");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLoginHistory)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_login_history_ibfk_1");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("users");

                entity.HasIndex(e => e.UserRoleId)
                    .HasName("UserRoleID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");
                entity.Property(e => e.UserRoleId2).HasColumnName("UserRoleID2");

                entity.HasOne(d => d.UserRole)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_ibfk_1");
            });

            modelBuilder.Entity<UsersRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId)
                    .HasName("PRIMARY");

                entity.ToTable("users_role");

                entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");

                entity.Property(e => e.UserRoleName)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

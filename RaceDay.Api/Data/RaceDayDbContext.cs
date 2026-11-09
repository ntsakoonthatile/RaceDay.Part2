using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<EventType> EventTypes => Set<EventType>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(x => x.UserId);

                entity.HasIndex(x => x.Email)
                    .IsUnique();

                entity.Property(x => x.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.LastName)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasMaxLength(20);

                entity.Property(x => x.ProfilePictureUrl)
                    .HasMaxLength(500);

                entity.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser','Participant')");
            });

            modelBuilder.Entity<EventType>(entity =>
            {
                entity.ToTable("EventTypes");

                entity.HasKey(x => x.EventTypeId);

                entity.HasIndex(x => x.TypeName)
                    .IsUnique();

                entity.Property(x => x.TypeName)
                    .HasMaxLength(20)
                    .IsRequired();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events");

                entity.HasKey(x => x.EventId);

                entity.Property(x => x.Name)
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.Location)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.DistanceKm)
                    .HasPrecision(6, 2);

                entity.Property(x => x.BannerImageUrl)
                    .HasMaxLength(500);

                entity.HasCheckConstraint(
                    "CK_Events_Distance",
                    "[DistanceKm] > 0");

                entity.HasOne(x => x.Organiser)
                    .WithMany(x => x.OrganisedEvents)
                    .HasForeignKey(x => x.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.EventType)
                    .WithMany(x => x.Events)
                    .HasForeignKey(x => x.EventTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(x => x.CategoryId);

                entity.Property(x => x.CategoryName)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(250);

                entity.HasIndex(x =>
                    new { x.EventId, x.CategoryName })
                    .IsUnique();

                entity.HasOne(x => x.Event)
                    .WithMany(x => x.Categories)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolments");

                entity.HasKey(x => x.EnrolmentId);

                entity.Property(x => x.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Confirmed");

                entity.Property(x => x.EnrolmentDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(x =>
                    new { x.ParticipantId, x.EventId })
                    .IsUnique();

                entity.HasCheckConstraint(
                    "CK_Enrolments_Status",
                    "[Status] IN ('Pending','Confirmed','Cancelled')");

                entity.HasOne(x => x.Participant)
                    .WithMany(x => x.Enrolments)
                    .HasForeignKey(x => x.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Event)
                    .WithMany(x => x.Enrolments)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Enrolments)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Results");

                entity.HasKey(x => x.ResultId);

                entity.HasIndex(x => x.EnrolmentId)
                    .IsUnique();

                entity.Property(x => x.Published)
                    .HasDefaultValue(false);

                entity.HasCheckConstraint(
                    "CK_Results_Position",
                    "[FinishPosition] > 0");

                entity.HasOne(x => x.Enrolment)
                    .WithOne(x => x.Result)
                    .HasForeignKey<Result>(x => x.EnrolmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

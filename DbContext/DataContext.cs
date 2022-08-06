namespace NepalDictServer.DbContext
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
        protected readonly IConfiguration Configuration;
        public DbSet<WordModel>? Words { get; set; }
        public DbSet<UserModel>? Users { get; set; }
        public DbSet<NoticeModel>? Notices { get; set; }

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to mysql with connection string from app settings
            var connectionString = Configuration.GetConnectionString("NepalDict");
            options.UseMySQL(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WordModel>().ToTable("WordResource");
            builder.Entity<WordModel>().HasKey(p => p.Id);
            builder.Entity<WordModel>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<WordModel>().Property(p => p.Word).HasMaxLength(50);
            builder.Entity<WordModel>().Property(p => p.Sound).HasMaxLength(50);
            builder.Entity<WordModel>().Property(p => p.Meaning).HasMaxLength(50);

            builder.Entity<UserModel>().ToTable("UserResource");
            builder.Entity<UserModel>().HasKey(p => p.UserPhone);
            builder.Entity<UserModel>().Property(p => p.UserPhone).IsRequired().HasMaxLength(20);
            builder.Entity<UserModel>().Property(p => p.UserName).IsRequired().HasMaxLength(50);
            builder.Entity<UserModel>().Property(p => p.UserSite).IsRequired().HasMaxLength(50);
            builder.Entity<UserModel>().Property(p => p.LoginYN).IsRequired().HasMaxLength(1);
            builder.Entity<UserModel>().Property(p => p.Auth_WordEdit).IsRequired().HasMaxLength(1);
            builder.Entity<UserModel>().Property(p => p.Auth_Approval).IsRequired().HasMaxLength(1);
            builder.Entity<UserModel>().Property(p => p.Auth_Master).IsRequired().HasMaxLength(1);
            builder.Entity<UserModel>().Property(p => p.UseYN).IsRequired().HasMaxLength(1);

            builder.Entity<NoticeModel>().ToTable("NoticeResource");
            builder.Entity<NoticeModel>().HasKey(p => p.Id);
        }


    }
}

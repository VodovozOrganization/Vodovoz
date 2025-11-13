using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace WarehouseApi.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
			{
				b.Property<string>("Id")
					.HasColumnType("varchar(450)");

				b.Property<string>("ConcurrencyStamp")
					.IsConcurrencyToken()
					.HasColumnType("varchar(2000)");

				b.Property<string>("Name")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.Property<string>("NormalizedName")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.HasKey("Id");

				b.HasIndex("NormalizedName")
					.IsUnique()
					.HasDatabaseName("RoleNameIndex")
					.HasFilter("[NormalizedName] IS NOT NULL");

				b.ToTable("AspNetRoles");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnType("int");

				b.Property<string>("ClaimType")
					.HasColumnType("varchar(2000)");

				b.Property<string>("ClaimValue")
					.HasColumnType("varchar(2000)");

				b.Property<string>("RoleId")
					.IsRequired()
					.HasColumnType("varchar(450)");

				b.HasKey("Id");

				b.HasIndex("RoleId");

				b.ToTable("AspNetRoleClaims");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
			{
				b.Property<string>("Id")
					.HasColumnType("varchar(450)");

				b.Property<int>("AccessFailedCount")
					.HasColumnType("int");

				b.Property<string>("ConcurrencyStamp")
					.IsConcurrencyToken()
					.HasColumnType("varchar(2000)");

				b.Property<string>("Email")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.Property<bool>("EmailConfirmed")
					.HasColumnType("bit");

				b.Property<bool>("LockoutEnabled")
					.HasColumnType("bit");

				b.Property<DateTimeOffset?>("LockoutEnd")
					.HasConversion<DateTime?>(
						property => property != null ? property.Value.DateTime : null,
						column => column
					)
					.HasColumnType("datetime");

				b.Property<string>("NormalizedEmail")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.Property<string>("NormalizedUserName")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.Property<string>("PasswordHash")
					.HasColumnType("varchar(2000)");

				b.Property<string>("PhoneNumber")
					.HasColumnType("varchar(2000)");

				b.Property<bool>("PhoneNumberConfirmed")
					.HasColumnType("bit");

				b.Property<string>("SecurityStamp")
					.HasColumnType("varchar(2000)");

				b.Property<bool>("TwoFactorEnabled")
					.HasColumnType("bit");

				b.Property<string>("UserName")
					.HasColumnType("varchar(256)")
					.HasMaxLength(256);

				b.HasKey("Id");

				b.HasIndex("NormalizedEmail")
					.HasDatabaseName("EmailIndex");

				b.HasIndex("NormalizedUserName")
					.IsUnique()
					.HasDatabaseName("UserNameIndex")
					.HasFilter("[NormalizedUserName] IS NOT NULL");

				b.ToTable("AspNetUsers");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd()
					.HasColumnType("int");

				b.Property<string>("ClaimType")
					.HasColumnType("varchar(2000)");

				b.Property<string>("ClaimValue")
					.HasColumnType("varchar(2000)");

				b.Property<string>("UserId")
					.IsRequired()
					.HasColumnType("varchar(450)");

				b.HasKey("Id");

				b.HasIndex("UserId");

				b.ToTable("AspNetUserClaims");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
			{
				b.Property<string>("LoginProvider")
					.HasColumnType("varchar(128)")
					.HasMaxLength(128);

				b.Property<string>("ProviderKey")
					.HasColumnType("varchar(128)")
					.HasMaxLength(128);

				b.Property<string>("ProviderDisplayName")
					.HasColumnType("varchar(2000)");

				b.Property<string>("UserId")
					.IsRequired()
					.HasColumnType("varchar(450)");

				b.HasKey("LoginProvider", "ProviderKey");

				b.HasIndex("UserId");

				b.ToTable("AspNetUserLogins");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
			{
				b.Property<string>("UserId")
					.HasColumnType("varchar(450)");

				b.Property<string>("RoleId")
					.HasColumnType("varchar(450)");

				b.HasKey("UserId", "RoleId");

				b.HasIndex("RoleId");

				b.ToTable("AspNetUserRoles");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
			{
				b.Property<string>("UserId")
					.HasColumnType("varchar(450)");

				b.Property<string>("LoginProvider")
					.HasColumnType("varchar(128)")
					.HasMaxLength(128);

				b.Property<string>("Name")
					.HasColumnType("varchar(128)")
					.HasMaxLength(128);

				b.Property<string>("Value")
					.HasColumnType("varchar(2000)");

				b.HasKey("UserId", "LoginProvider", "Name");

				b.ToTable("AspNetUserTokens");
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
			{
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
					.WithMany()
					.HasForeignKey("RoleId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
			{
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
			{
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
			{
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
					.WithMany()
					.HasForeignKey("RoleId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();

				b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
			{
				b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
					.WithMany()
					.HasForeignKey("UserId")
					.OnDelete(DeleteBehavior.Cascade)
					.IsRequired();
			});
		}
	}
}

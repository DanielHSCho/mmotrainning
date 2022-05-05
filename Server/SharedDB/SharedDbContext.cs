using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    { 
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }

        // Note : 게임서버 방식에서 사용할 문자열
        // 계정서버는 options로 알아서 전달 받음
        public static string ConnectionString { get; set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        // 게임 서버에서 사용할 방식
        public SharedDbContext()
        {

        }

        // ASP.NET 방식
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {

        }

        // 게임서버 방식 
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(options.IsConfigured == false) {
                options
               //.UseLoggerFactory(_logger)
               .UseSqlServer(ConnectionString);
            }
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
    }
}

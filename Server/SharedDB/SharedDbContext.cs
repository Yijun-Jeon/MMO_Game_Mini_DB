using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }

        #region GameServer 방식
        public SharedDbContext()
        {

        }

        public static string ConnectionString { get; set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // ASP .NET은 이미 미리 옵션 설정이 되어있음
            if(options.IsConfigured == false)
            {
                options
                //.UseLoggerFactory(_logger)
                .UseSqlServer(ConnectionString);
            }
        }
        #endregion

        #region ASP .NET 방식
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 빠르게 찾기 위해 인덱스를 걸어줌
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
        #endregion
    }
}



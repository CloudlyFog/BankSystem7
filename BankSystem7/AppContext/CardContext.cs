﻿using Microsoft.EntityFrameworkCore;
using Standart7.Models;

namespace BankSystem7.AppContext
{
    public sealed class CardContext : DbContext
    {
        private readonly string queryConnection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        public CardContext(string queryConnection)
        {
            this.queryConnection = queryConnection;
            Database.EnsureCreated();
        }
        public CardContext() => Database.EnsureCreated();
        internal DbSet<Card> Cards { get; set; } = null!;
        internal DbSet<User> Users { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(queryConnection);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>().Ignore("MaxLength").Ignore("Exception");
            base.OnModelCreating(modelBuilder);
        }
    }
}
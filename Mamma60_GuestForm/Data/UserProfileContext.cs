using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mamma60_GuestForm.Models;
using Microsoft.EntityFrameworkCore;

namespace Mamma60_GuestForm.Data
{
    public class UserProfileContext : DbContext
    {
        public UserProfileContext(DbContextOptions<UserProfileContext> options) : base(options)
        {
        }

        public DbSet<ProfileViewModel> ProfileViewModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfileViewModel>().ToTable("UserProfile");
        }
    }
}

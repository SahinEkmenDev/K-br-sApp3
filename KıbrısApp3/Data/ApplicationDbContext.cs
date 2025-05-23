﻿using KıbrısApp3.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KıbrısApp3.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AdListing> AdListings { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FavoriteAd> FavoriteAds { get; set; }
        public DbSet<AdImage> AdImages { get; set; }
        public DbSet<CarAdDetail> CarAdDetails { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<MotorcycleAdDetail> MotorcycleAdDetails { get; set; }
        public DbSet<UserExpoToken> UserExpoTokens { get; set; }






    }
}

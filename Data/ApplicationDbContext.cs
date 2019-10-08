using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Manifest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUserComment>()
                .HasKey( x => new { x.FromUserId, x.ToUserId });

            builder.Entity<ApplicationUserComment>()
                .HasOne( x => x.FromUser )
                .WithMany( x => x.CommentsTo)
                .HasForeignKey(x => x.FromUserId);

            builder.Entity<ApplicationUserComment>()
                .HasOne( x => x.ToUser )
                .WithMany( x => x.CommentsFrom)
                .HasForeignKey( x => x.ToUserId);

            base.OnModelCreating(builder);
        }

        public DbSet<ApplicationUserComment> UserComments { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        public string FbName { get; set;}

        public string FbProfilePicUrl { get; set; }
        
        public string FbProfilePicLargeUrl { get; set; }

        public ICollection<ApplicationUserComment> CommentsTo { get; set; }
        public ICollection<ApplicationUserComment> CommentsFrom { get; set; }
    }

    public class ApplicationUserComment 
    {
        public string FromUserId { get;set; }

        public string ToUserId { get;set; }

        public ApplicationUser FromUser { get; set; }

        public ApplicationUser ToUser { get; set; }

        public string Comment { get; set; }
    }
}

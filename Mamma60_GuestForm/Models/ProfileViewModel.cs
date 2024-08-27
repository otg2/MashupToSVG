using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Mamma60_GuestForm.Models
{
    public class ProfileViewModel
    {
        public  string Id { get; set; }

        public string Name { get; set; }

        public string Memory { get; set; }

        public string HexColor { get; set; }

        public byte[] ProfilePicture { get; set; }

        public DateTime DateCreated { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }


    //public class ApplicationDbCotext : IdentityDbContext<ProfileViewModel>
    //{
    //    public DbSet<ProfileViewModel> Assets { get; set; }
    //}
}

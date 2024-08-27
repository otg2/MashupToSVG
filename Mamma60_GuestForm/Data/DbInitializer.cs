using Mamma60_GuestForm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mamma60_GuestForm.Data
{
    public static class DbInitializer
    {
        
        public static void Initialize(UserProfileContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.ProfileViewModel.Any())
            {
                return;   // DB has been seeded
            }

            var entities = new ProfileViewModel[]
            {
                new ProfileViewModel{Id= "47880332-06d6-4413-9b04-9c3b69cf76d5" ,Name="Ottar", Memory="Ég man þegar að xxx", HexColor="#1f3cff"}
            };
            foreach (ProfileViewModel s in entities)
            {
                context.ProfileViewModel.Add(s);
            }
            context.SaveChanges();
        }
    }
}

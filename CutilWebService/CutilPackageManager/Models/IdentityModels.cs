using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace CutilPackageManager.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
      public string ApiKey { get; set; }
      public string Email { get; set; }
      public bool IsConfirmed { get; set; }

      public string ConfirmationToken { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
        
        public IDbSet<Package> Packages { get; set; }
    }
}
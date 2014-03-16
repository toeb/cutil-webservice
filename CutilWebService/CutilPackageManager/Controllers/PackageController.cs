using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CutilPackageManager.Controllers
{
  public class Package
  {
    public int Id { get; set; }
    public string PackageName { get; set; }
    public string PackageDescription { get; set; }
    public string PackageSource { get; set; }
    public string Owner { get; set; }
    public string Password { get; set; }

    public bool Visible { get; set; }
    public string Version { get; set; }
  }
  public class PackageManagerDbContext : DbContext
  {
    public IDbSet<Package> Packages { get; set; }
  }
  public class PackageController : ApiController
  {
    PackageManagerDbContext context = new PackageManagerDbContext();
    public IEnumerable<Test> Get()
    {
      yield return new Test() { Id="a", Name = "A"};
      yield return new Test() { Id = "b", Name = "B" };
      yield return new Test() { Id = "c", Name = "C" };
      /*
      foreach (var package in context.Packages.Where(p => p.Visible).AsNoTracking())
      {
        package.Password = "";
        yield return package;
      }*/
    }
    public Package Post([FromBody] Package package)
    {

      package.Visible = true;
      package.Owner = User.Identity.Name;
      context.Packages.Add(package);
      context.SaveChanges();
      package.Password = Guid.NewGuid().ToString();
      return package;
    
    }

    public class Test
    {
      public string Id { get; set; }
      public string Name { get; set; }
    }
    public async Task<Test> Put()
    {
      var data = await this.Request.Content.ReadAsStringAsync();
      return new Test(){Id = "asd", Name = "bdbd"};
    }


    public void Delete(Package package)
    {
      var pack = context.Packages.Single(p => p.Id == package.Id);
      pack.Visible = false;
      context.SaveChanges();
      return;
    }
  }
}
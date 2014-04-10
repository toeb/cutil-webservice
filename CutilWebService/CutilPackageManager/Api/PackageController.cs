using CutilPackageManager.Models;
using CutilPackageManager.Util;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CutilPackageManager.Controllers.Api
{
  class package_query
  {
    public string whole { get; set; }
    public string @string { get; set; }
    public string package_constraint { get; set; }
    public string id_constraint { get; set; }
    public version_constraint version_constraint { get; set; }
  }
  class version_constraint
  {
    public string template { get; set; }
    public string elements { get; set; }

  }
  public class PackageController : ApiController
  {
    ApplicationDbContext context = new ApplicationDbContext();
    [HttpGet]
    [HttpPut]
    [ActionName("query")]
    public async Task<IEnumerable<string>> Query()
    {
      var content = await Request.Content.ReadAsStringAsync();
      var packages = context.Packages.Where(p => p.Visible).ToArray();
      try
      {
        if (content != "")
        {
          var query = JsonConvert.DeserializeObject<package_query>(content);
          if (!string.IsNullOrWhiteSpace(query.id_constraint))
          {
            packages = packages.Where(p => p.Identifier.StartsWith(query.id_constraint)).ToArray();
          }
          //todo correct version query
          if (query.version_constraint != null && !string.IsNullOrWhiteSpace(query.version_constraint.template))
          {
            packages = packages.Where(p => p.Version.StartsWith(query.version_constraint.template)).ToArray();
          }
        }
      }
      catch (Exception e)
      {

      }


      var res = packages.Select(p => p.Identifier + "@" + p.Version).ToArray();
      return res;
    }




    [HttpGet]
    [ActionName("resolve")]
    public object Resolve(string package)
    {
      try
      {
        var match = Regex.Match(package, @"(?<name>[^@]*)(@(?<version>.*))?");
        var id = match.Groups["name"].Value;
        var version = match.Groups["version"].Value;
        Package pkg;
        if (string.IsNullOrEmpty(version))
        {

          pkg = context.Packages.Where(p => p.Identifier == id).OrderByDescending(p => p.Version).FirstOrDefault();

        }
        else
        {
          pkg = context.Packages.FirstOrDefault(p => p.Identifier == id && p.Version == version);
        }
        return JsonConvert.DeserializeObject(pkg.Descriptor);
      }
      catch (Exception e)
      {

        throw e;
      }
    }

    [ActionName("download")]
    [HttpGet]
    public HttpResponseMessage Download(string id)
    {
      var res = new HttpResponseMessage(HttpStatusCode.OK);

      var match = Regex.Match(id, @"(?<name>[^@]*)(@(?<version>.*))?");
      var pkgid = match.Groups["name"].Value;
      var version = match.Groups["version"].Value;
      Package pkg;
      if (string.IsNullOrEmpty(version))
      {
        pkg = context.Packages.Where(p => p.Identifier == pkgid).OrderByDescending(p => p.Version).FirstOrDefault();
      }
      else
      {
        pkg = context.Packages.FirstOrDefault(p => p.Identifier == pkgid && p.Version == version);
      }
      if (pkg == null) throw new HttpException((int)HttpStatusCode.NotFound, "could not resolve " + id);
      pkg.DownloadCount++;
      context.SaveChanges();

      res.Content = new ByteArrayContent(pkg.Data);
      // res.Headers.Add("Content-Type", "application/x-compressed");
      return res;
      throw new Exception();
    }


    [ActionName("unpublish")]
    [HttpPut]
    [HttpGet]
    [ApiKeyAuthenticationFilterAttribute]
    public void Unpublish(string id)
    {

      if (!Thread.CurrentPrincipal.Identity.IsAuthenticated) throw new HttpException((int)HttpStatusCode.Unauthorized, "not authorized");
      var username = Thread.CurrentPrincipal.Identity.Name;


      var match = Regex.Match(id, @"(?<name>[^@]*)(@(?<version>.*))?");
      var pkgid = match.Groups["name"].Value;
      var version = match.Groups["version"].Value;
      Package pkg;


      pkg = context.Packages.FirstOrDefault(p => p.Identifier == pkgid && p.Version == version);

      if (pkg.User != username)
      {
        throw new HttpException((int)HttpStatusCode.Unauthorized, "you are not the owner of the package");
      }



      if (pkg == null) throw new HttpException((int)HttpStatusCode.NotFound, "could not resolve " + id);


      context.Packages.Remove(pkg);
      context.SaveChanges();

    }


    [ActionName("publish")]
    [HttpPut]
    [ApiKeyAuthenticationFilterAttribute]
    public async Task<PackageDto> Publish(string format, string action)
    {
      if (!Thread.CurrentPrincipal.Identity.IsAuthenticated) throw new HttpException((int)HttpStatusCode.Unauthorized, "not authorized");
      var username = Thread.CurrentPrincipal.Identity.Name;

      if (format == "tgz")
      {
        var content = await Request.Content.ReadAsByteArrayAsync();
        var contentStream = new MemoryStream(content);
        var gzStream = new GZipInputStream(contentStream);

        var tarIn = new TarInputStream(gzStream);
        TarEntry entry;

        while ((entry = tarIn.GetNextEntry()) != null)
        {
          if (entry.Name == "package.cmake") break;
        }
        if (entry == null) throw new Exception("file does not contain package.cmake");

        StringWriter writer = new StringWriter();

        byte[] data = new byte[2048];
        while (true)
        {

          var size = tarIn.Read(data, 0, data.Length);
          if (size > 0)
          {

            var chars = Encoding.UTF8.GetChars(data, 0, size);

            writer.Write(chars);
          }
          else
          {
            break;
          }
        }

        var pd = writer.ToString();

        var descriptor = JsonConvert.DeserializeObject<PackageDto>(pd);
        var package = new Package();
        package.Identifier = descriptor.id;
        package.Version = descriptor.version;
        package.User = username;
        package.PackageName = descriptor.name;
        if (package.PackageName == null) package.PackageName = package.Identifier;
        package.PackageDescription = descriptor.description;
        package.Data = content;
        package.Visible = true;
        var existingPackage = context.Packages.FirstOrDefault(p => p.Identifier == descriptor.id && p.Version == descriptor.version);
        if (existingPackage != null) throw new Exception("package already exists in specified version");

        package.Descriptor = pd;

        var previousPackage = context.Packages.FirstOrDefault(p => p.Identifier == descriptor.id);
        if (previousPackage != null)
        {
          if (previousPackage.User != username)
          {
            throw new HttpException((int)HttpStatusCode.BadRequest, "you are not owner of package");
          }
        }

        context.Packages.Add(package);
        // set current version
        if (action == "make_current")
        {
          package.CurrentVersion = true;
          var currentPackages = context.Packages.Where(p => p.Identifier == package.Identifier && package.CurrentVersion);
          foreach (var pp in currentPackages)
          {
            pp.CurrentVersion = false;

          }
        }



        await context.SaveChangesAsync();

        return new PackageDto(package);

      }
      return null;
    }




    public class PackageDto
    {
      public PackageDto() { }
      public PackageDto(Package package)
      {
        id = package.Identifier;
        version = package.Version;
        description = package.PackageDescription;
      }

      public string id { get; set; }
      public string name { get; set; }
      public string version { get; set; }
      public string[] authors { get; set; }
      public string owner { get; set; }

      public string[] keywords { get; set; }
      public string description { get; set; }
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
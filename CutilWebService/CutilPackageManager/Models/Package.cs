using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CutilPackageManager.Models
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

    public string Identifier { get; set; }

    public string Descriptor { get; set; }

    public byte[] Data { get; set; }

    public string User { get; set; }

    public int DownloadCount { get; set; }

    public bool CurrentVersion { get; set; }
  }

}
using CutilPackageManager.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;

using Microsoft.Owin.Security;
using System.Security.Claims;
namespace CutilPackageManager.Util
{
  public class ApiKeyAuthenticationFilterAttribute : AuthorizationFilterAttribute
  {
    private bool active;
    public ApiKeyAuthenticationFilterAttribute(bool active = true)
    {
      this.active = active;
    }
    public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
    {
      
      base.OnAuthorization(actionContext);
//Thread.CurrentPrincipal.Identity.IsAuthenticated

      if (active)
      {
        var query_args = actionContext.Request.GetQueryNameValuePairs().ToDictionary(k => k.Key, v => v.Value);

        if (!query_args.ContainsKey("apikey")) return;
        var key = query_args["apikey"] as string;
        if (string.IsNullOrEmpty(key)) return;

        var context = new ApplicationDbContext();
        var user = context.Users.Where(u=>u.ApiKey == key).FirstOrDefault();
        if (user == null) return;

        var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
       
        var task = um.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
        task.Wait();
        var identity = task.Result;
        Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

      }
    }
  }

}
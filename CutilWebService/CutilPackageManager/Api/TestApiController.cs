using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace CutilPackageManager.Api
{
  public class TestApiController : ApiController
  {
    [ActionName("CompressedWebSourcePush")]
    [HttpPut]
    [HttpGet]
    public HttpResponseMessage CompressedWebSourcePush()
    {
      return Request.CreateResponse(HttpStatusCode.OK);
    }


    [ActionName("CompressedWebSourcePull")]
    [HttpPut]
    [HttpGet]
    public HttpResponseMessage CompressedWebSourcePull()
    {
      var res = new HttpResponseMessage(HttpStatusCode.OK);
      var path = HostingEnvironment.MapPath("~/Content/pushed.tgz");
      res.Content = new ByteArrayContent(File.ReadAllBytes(path));
      return Request.CreateResponse(HttpStatusCode.OK);
    }




    [ActionName("WebServicePackageSourcePull")]
    public HttpResponseMessage WebServicePackageSourcePull()
    {
      return Request.CreateResponse(HttpStatusCode.OK);
    }

  }
}

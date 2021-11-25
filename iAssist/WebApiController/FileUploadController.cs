using System;
using System.Linq;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using System.Web.Routing;
using System.IO;

namespace iAssist.WebApiControllers
{
    public class FileUploadController : ApiController
    {
        [Authorize]
        [HttpPost]
        [Route("api/Upload")]
        public async Task<string> UploadFile()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var fileName = postedFile.FileName.Split('\\').LastOrDefault().Split('/').LastOrDefault();

                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);

                        fileName = name + DateTime.Now.ToString("yymmssfff") + extension;

                        var filePath = HttpContext.Current.Server.MapPath("~/image/" + fileName);
                        postedFile.SaveAs(filePath);
                        return fileName;
                    }
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
            return "no files";
        }
    }
}
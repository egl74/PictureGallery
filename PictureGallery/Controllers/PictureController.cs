using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using MvcFileUploader;
using MvcFileUploader.Models;
using PictureGallery.Models;

namespace PictureGallery.Controllers
{
    [Authorize]
    public class PictureController : BaseController
    {
        readonly Cloudinary cloudinary = new Cloudinary(new Account(
                        "djnqdhxa1",
                        "339589888966938",
                        "SY4SK3NWfoed9K7BjoBLhQJ4lu4"));
        // GET: Gallery
        public ActionResult Index()
        {
            return View(GetAllPicturesOfCurrentUser());
        }

        public ActionResult UploadFile(int? entityId) // optionally receive values specified with Html helper
        {
            // here we can send in some extra info to be included with the delete url 
            var statuses = new List<ViewDataUploadFileResult>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var x = new MvcFileSave();
                x.File = Request.Files[i];
                var filePath = Server.MapPath("~\\" + x.File.FileName);
                var fileStream = System.IO.File.Create(filePath);
                x.File.InputStream.Seek(0, SeekOrigin.Begin);
                x.File.InputStream.CopyTo(fileStream);
                fileStream.Close();
                //BinaryReader b = new BinaryReader(x.File.InputStream);
                //byte[] binImage = b.ReadBytes(Convert.ToInt32(x.File.InputStream.Length));
                //Stream stream = new MemoryStream(binImage);
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(filePath)
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                System.IO.File.Delete(filePath);
                x.UrlPrefix = uploadResult.SecureUri.AbsoluteUri;
                string currentUserId = User.Identity.GetUserId();
                
                var currentUser = Context.Users.SingleOrDefault(u => u.Id == currentUserId);
                currentUser.Pictures.Add(new Picture {Url = x.UrlPrefix});
                
                Context.SaveChanges();
                x.FileName = Request.Files[i].FileName; // default is filename suffixed with filetimestamp
                x.ThrowExceptions = false; //default is false, if false exception message is set in error property
                statuses.Add(new ViewDataUploadFileResult
                {
                    FullPath = x.UrlPrefix,
                    size = x.File.ContentLength,
                    deleteUrl = x.DeleteUrl,
                    thumbnailUrl = x.UrlPrefix,
                    url = x.UrlPrefix,
                    name = x.FileName
                });
            }

            //statuses contains all the uploaded files details (if error occurs then check error property is not null or empty)
            //todo: add additional code to generate thumbnail for videos, associate files with entities etc

            //adding thumbnail url for jquery file upload javascript plugin
            statuses.ForEach(x => x.thumbnailUrl = x.url);// + " width=300px height=200px"); // uses ImageResizer httpmodule to resize images from this url
            //setting custom download url instead of direct url to file which is default
            statuses.ForEach(x => x.url = Url.Action("DownloadFile", new { fileUrl = x.url, mimetype = x.type }));
            
            //server side error generation, generate some random error if entity id is 13
            if (entityId == 13)
            {
                var rnd = new Random();
                statuses.ForEach(x =>
                {
                    //setting the error property removes the deleteUrl, thumbnailUrl and url property values
                    x.error = rnd.Next(0, 2) > 0 ? "We do not have any entity with unlucky Id : '13'" : String.Format("Your file size is {0} bytes which is un-acceptable", x.size);
                    //delete file by using FullPath property
                    if (System.IO.File.Exists(x.FullPath)) System.IO.File.Delete(x.FullPath);
                });
            }

            var viewresult = Json(new { files = statuses });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return viewresult;
        }

        public ActionResult DownloadFile(string fileUrl, string mimetype)
        {
            return Redirect(fileUrl);
        }

        public List<string> GetAllPicturesOfCurrentUser()
        {
            string currentUserId = User.Identity.GetUserId();

            //var result = Context.Pictures.Where(p => p.UserId.ToString() == currentUserId).Select(l => l.Url).ToList();
            var result = Context.Pictures.Where(p => p.UserId.ToString() == currentUserId).Select(u => u.Url).ToList();
            return result;
        }
    }
}
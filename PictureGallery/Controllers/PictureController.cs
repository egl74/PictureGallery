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
using PictureGallery.Migrations;
using PictureGallery.Models;

namespace PictureGallery.Controllers
{
    [Authorize]
    public class PictureController : BaseController
    {
        // GET: Gallery
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadFile(int? entityId) // optionally receive values specified with Html helper
        {
            // here we can send in some extra info to be included with the delete url 
            var statuses = new List<ViewDataUploadFileResult>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var st = FileSaver.StoreFile(x =>
                {
                    x.File = Request.Files[i];
                    //note how we are adding an additional value to be posted with delete request
                    //and giving it the same value posted with upload
                    x.DeleteUrl = Url.Action("DeleteFile", new { entityId = entityId });
                    var cloudinary = new Cloudinary(new Account(
                        "djnqdhxa1",
                        "339589888966938",
                        "SY4SK3NWfoed9K7BjoBLhQJ4lu4"));
                    var filePath = Server.MapPath("~") + "\\file";
                    var fileStream = System.IO.File.Create(filePath);
                    x.File.InputStream.Seek(0, SeekOrigin.Begin);
                    x.File.InputStream.CopyTo(fileStream);
                    fileStream.Close();
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(
                            filePath)
                    };
                    var uploadResult = cloudinary.Upload(uploadParams);
                    System.IO.File.Delete(filePath);
                    //x.StorageDirectory = Server.MapPath("~/Content/uploads");
                    x.UrlPrefix = uploadResult.SecureUri.AbsoluteUri;//"/Content/uploads";// this is used to generate the relative url of the file
                    var c = ApplicationDbContext.Create();
                    //c.Users.FirstOrDefault(u => u.Id == User.Identity.GetUserId()).Pictures.Add(new Picture{UserId = new Guid(User.Identity.GetUserId()), Url = x.UrlPrefix});
                    string currentUserId = User.Identity.GetUserId();
                    var currentUser = c.Users.FirstOrDefault(u => u.Id == currentUserId);
                    if (currentUser.Pictures != null)
                    {
                        currentUser.Pictures.Add(new Picture { UserId = new Guid(User.Identity.GetUserId()), Url = x.UrlPrefix });
                    }
                    else
                    {
                        currentUser.Pictures = new List<Picture>
                        {
                            new Picture {UserId = new Guid(User.Identity.GetUserId()), Url = x.UrlPrefix}
                        };
                    }
                    //overriding defaults
                    x.FileName = Request.Files[i].FileName;// default is filename suffixed with filetimestamp
                    x.ThrowExceptions = false;//default is false, if false exception message is set in error property
                });

                statuses.Add(st);
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





        //here i am receving the extra info injected
        [HttpPost] // should accept only post
        public ActionResult DeleteFile(int? entityId, string fileUrl)
        {
            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var viewresult = Json(new { error = String.Empty });
            //for IE8 which does not accept application/json
            if (Request.Headers["Accept"] != null && !Request.Headers["Accept"].Contains("application/json"))
                viewresult.ContentType = "text/plain";

            return viewresult; // trigger success
        }


        public ActionResult DownloadFile(string fileUrl, string mimetype)
        {
            var filePath = Server.MapPath("~" + fileUrl);

            if (System.IO.File.Exists(filePath))
                return File(filePath, mimetype);
            else
            {
                return new HttpNotFoundResult("File not found");
            }
        }

        [HttpPost]
        public void UploadImage(string path)
        {
            var cloudinary = new Cloudinary(new Account(
                "djnqdhxa1",
                "339589888966938",
                "SY4SK3NWfoed9K7BjoBLhQJ4lu4"));
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(path)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
        }
    }
}
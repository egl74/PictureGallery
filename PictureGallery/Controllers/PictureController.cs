using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNet.Identity;

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
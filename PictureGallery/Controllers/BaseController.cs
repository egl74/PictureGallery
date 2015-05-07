using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PictureGallery.Models;

namespace PictureGallery.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext Context = ApplicationDbContext.Create();
        protected ApplicationUserManager _userManager;
        protected ApplicationSignInManager _signInManager; 

        public BaseController(): this(new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public BaseController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
    }
}
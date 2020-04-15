using BDInSelfLove.Services.Data.User;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Web.ViewModels.Forum.Profile;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    public class ProfileController : BaseForumController
    {
        private readonly IUserService userService;

        public ProfileController(IUserService userService)
        {
            this.userService = userService;
        }

        [Breadcrumb("Profile")]
        public async Task<ActionResult> Index(string username)
        {
            var user = AutoMapperConfig.MapperInstance.Map<ProfileUserViewModel>(
                await this.userService.GetProfileInfo(username));

            var breadcrumb = new BreadcrumbNode("Profile", "Index", "Profile", null, null);
            this.ViewData["BreadcrumbNode"] = breadcrumb;

            return this.View(user);
        }
    }
}

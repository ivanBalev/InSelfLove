namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using BDInSelfLove.Common;
    using BDInSelfLove.Web.Controllers;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    [Area("Administration")]
    public class AdministrationController : BaseController
    {
    }
}

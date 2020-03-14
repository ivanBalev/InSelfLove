namespace BDInSelfLove.Web.Controllers
{
    using System.Diagnostics;
    using BDInSelfLove.Web.Filters;
    using BDInSelfLove.Web.Filters.AuthorizationFilters;
    using BDInSelfLove.Web.Filters.ExceptionFilters;
    using BDInSelfLove.Web.Filters.ResourceFilters;
    using BDInSelfLove.Web.Filters.ResultFilters;
    using BDInSelfLove.Web.ViewModels;

    using Microsoft.AspNetCore.Mvc;

    // DEPENDENCY INJECTION WITH FILTERS
    //[TypeFilter(typeof(AddHeaderActionFilterAttribute))]
    public class HomeController : BaseController
    {
        //[AddHeaderAsyncActionFilter]
        //// SERVICEFILTER ALLOWS US TO CONTROL HOW WE INSTANTIATE THE FILTER (SINGLETON, TRANSIENT, SCOPED) IN THE FILE STARTUP.CS
        ////[ServiceFilter(typeof(AddHeaderAsyncActionFilterAttribute))]
        //[MyAuthorizationFilter]
        //[MyExceptionFilter]
        //[MyResourceFilter]
        //[MyResultFilter]
        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        public IActionResult Contact()
        {
            return this.View();
        }
    }
}

namespace InSelfLove.Web.Infrastructure.Filters.AuthorizationFilters
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ReadableBodyStreamAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Request.EnableBuffering();
        }
    }
}

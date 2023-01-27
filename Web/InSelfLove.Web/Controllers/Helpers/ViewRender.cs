namespace InSelfLove.Web.Controllers.Helpers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Routing;

    public class ViewRender : IViewRender
    {
        private readonly IRazorViewEngine viewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IServiceProvider serviceProvider;

        public ViewRender(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            this.viewEngine = viewEngine;
            this.tempDataProvider = tempDataProvider;
            this.serviceProvider = serviceProvider;
        }

        // Finds view and returns it stringified
        public async Task<string> RenderPartialViewToString<TModel>(string name, TModel model)
        {
            // Default empty action context
            var actionContext = this.GetActionContext();

            // Get view engine result
            var viewEngineResult = this.viewEngine.FindView(actionContext, name, isMainPage: false);

            if (!viewEngineResult.Success)
            {
                // Throw exception if view is not found
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                // Provide view context with default empty data + out model for the view to render
                var viewContext = new ViewContext(
                                      actionContext,
                                      view,
                                      new ViewDataDictionary<TModel>(
                                          metadataProvider: new EmptyModelMetadataProvider(),
                                          modelState: new ModelStateDictionary())
                                      {
                                          Model = model,
                                      },
                                      new TempDataDictionary(
                                          actionContext.HttpContext,
                                          this.tempDataProvider),
                                      output,
                                      new HtmlHelperOptions());

                // Render view
                await view.RenderAsync(viewContext);
                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = this.serviceProvider,
            };

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}

namespace BDInSelfLove.Web.Controllers.Helpers
{
    using System.Threading.Tasks;

    public interface IViewRender
    {
        Task<string> RenderPartialViewToString<TModel>(string name, TModel model);
    }
}

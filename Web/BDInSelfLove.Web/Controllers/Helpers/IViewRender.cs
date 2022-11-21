using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers.Helpers
{
    public interface IViewRender
    {
        Task<string> RenderPartialViewToString<TModel>(string name, TModel model);
    }
}

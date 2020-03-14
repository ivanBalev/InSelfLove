namespace BDInSelfLove.Web.Infrastructure.ModelBinders
{

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    // In order for our binder to work globally, without specifically using it as an attribute, we need a provider
    public class YearModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if(context.BindingInfo?.BinderModelName?.ToLower() == "year" && context.BindingInfo?.BinderType == typeof(int))
            {
                return new YearModelBinder();
            }

            return null;
        }
    }
}

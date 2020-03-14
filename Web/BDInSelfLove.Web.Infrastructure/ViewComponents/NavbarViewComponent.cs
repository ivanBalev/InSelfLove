namespace BDInSelfLove.Web.Infrastructure.ViewComponents
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    [ViewComponent(Name = "Navbar")]
    public class NavbarViewComponent : ViewComponent
    {
        public NavbarViewComponent(/*here we can inject services*/)
        {
        }

        public IViewComponentResult Invoke(int count)
        {
            //and then use those services here
            var viewModel = new NavbarViewModel();
            viewModel.Years = Enumerable.Range(1, count);
            return this.View(viewModel);
        }
    }

    public class NavbarViewModel
    {
        public IEnumerable<int> Years { get; set; }
    }
}

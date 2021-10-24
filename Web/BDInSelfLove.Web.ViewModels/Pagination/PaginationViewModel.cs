namespace BDInSelfLove.Web.ViewModels.Pagination
{
    public class PaginationViewModel
    {
        public int PagesCount { get; set; }

        public int CurrentPage { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }
    }
}

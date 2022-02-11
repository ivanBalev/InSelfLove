namespace BDInSelfLove.Web.ViewModels.Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BDInSelfLove.Web.ViewModels.Video;

    public class HomeViewModel
    {
        private const int DefaultItemsCount = 3;
        private List<ArticlePreviewViewModel> articles;
        private List<VideoPreviewViewModel> videos;
        private FeaturedItem featuredItem;

        public HomeViewModel(List<ArticlePreviewViewModel> articles, List<VideoPreviewViewModel> videos)
        {
            this.articles = articles;
            this.videos = videos;
            this.SetFeaturedItem();
        }

        public List<ArticlePreviewViewModel> Articles => this.articles;

        public List<VideoPreviewViewModel> Videos => this.videos;

        public FeaturedItem FeaturedItem => this.featuredItem;

        private void SetFeaturedItem()
        {
            var lastArticle = this.articles.FirstOrDefault();
            var lastVideo = this.videos.FirstOrDefault();
            bool videoIsLatest = DateTime.Compare(lastArticle.CreatedOn, lastVideo.CreatedOn) == -1;

            FeaturedItem featuredItem = new FeaturedItem();
            if (videoIsLatest)
            {
                featuredItem.Title = lastVideo.Title;
                featuredItem.Slug = lastVideo.Slug;
                featuredItem.Controller = "Videos";
            }
            else
            {
                featuredItem.Title = lastArticle.Title;
                featuredItem.Slug = lastArticle.Slug;
                featuredItem.Controller = "Articles";
            }

            this.featuredItem = featuredItem;
        }
    }
}

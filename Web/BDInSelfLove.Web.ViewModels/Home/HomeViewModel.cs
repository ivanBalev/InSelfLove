namespace BDInSelfLove.Web.ViewModels.Home
{
    using BDInSelfLove.Web.ViewModels.Video;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HomeViewModel
    {
        private const int DefaultItemsCount = 3;
        private List<ArticlePreviewViewModel> articles;
        private List<VideoPreviewViewModel> videos;

        public HomeViewModel(List<ArticlePreviewViewModel> articles, List<VideoPreviewViewModel> videos)
        {
            this.articles = articles;
            this.videos = videos;
        }

        public List<ArticlePreviewViewModel> Articles => this.articles;

        public List<VideoPreviewViewModel> Videos => this.videos;

        public FeaturedItem FeaturedItem
        {
            get
            {
                ArticlePreviewViewModel lastArticle = this.articles.FirstOrDefault();
                VideoPreviewViewModel lastVideo = this.videos.FirstOrDefault();
                bool videoIsLatest = DateTime.Compare(lastArticle.CreatedOn, lastVideo.CreatedOn) == -1;

                FeaturedItem featuredItem = new FeaturedItem();
                if (videoIsLatest)
                {
                    // Only 3 items needed for home page
                    if (this.videos.Count > DefaultItemsCount)
                    {
                        this.videos = this.videos.Skip(1).ToList();
                    }

                    featuredItem.Title = lastVideo.Title;
                    featuredItem.Slug = lastVideo.Slug;
                    featuredItem.Controller = "Videos";
                }
                else
                {
                    // Only 3 items needed for home page
                    if (this.articles.Count > DefaultItemsCount)
                    {
                        this.articles = this.articles.Skip(1).ToList();
                    }

                    featuredItem.Title = lastArticle.Title;
                    featuredItem.Slug = lastArticle.Slug;
                    featuredItem.Controller = "Articles";
                }

                return featuredItem;
            }
        }
    }
}

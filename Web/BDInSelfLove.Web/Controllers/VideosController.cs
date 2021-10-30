﻿namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.InputModels.Video;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class VideosController : PreviewAndPaginationHelper
    {
        public VideosController(
            IVideoService videoService)
            : base(videoService)
        {
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var viewModel = await this.GetVideosPreviewAndPagination(page);

            return this.View(viewModel);
        }

        [Route("Videos/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<VideoViewModel>(await this.VideoService
                .GetBySlug(slug));

            for (int i = 0; i < viewModel.Comments.Count; i++)
            {
                viewModel.Comments[i].CreatedOn = TimezoneHelper.ToLocalTime(
                    viewModel.Comments[i].CreatedOn, this.TimezoneCookieValue);
            }

            return this.View(viewModel);
        }

        // Admin access only below
        [HttpGet]
        [Route("Videos/Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Route("Videos/Create")]
        [YoutubeLinkActionFilter]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create(CreateVideoInputModel inputModel)
        {
            string slug = await this.VideoService
                .Create(AutoMapperConfig.MapperInstance.Map<Video>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.VideoService.Delete(id);

            return this.Redirect("/");
        }
    }
}

﻿namespace BDInSelfLove.Services.Data.Category
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;
    using BDInSelfLove.Web.InputModels.Forum.Category;
    using Microsoft.EntityFrameworkCore;

    public class CategoryService : ICategoryService
    {
        private readonly IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository;
        private readonly ICommentService commentService;

        private Dictionary<string, List<string>> availableSortingCriteria = new Dictionary<string, List<string>>
            {
                { "TimeCriteria", new List<string> { "all posts", "day", "month", "year" } },
                { "GroupingCriteria",  new List<string> { "date created", "author", "replies", "topic" } },
                { "OrderingCriteria", new List<string> { "descending", "ascending" } },
            };

        public CategoryService(IDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository, ICommentService commentService)
        {
            this.categoryRepository = categoryRepository;
            this.commentService = commentService;
        }

        public IQueryable<CategoryServiceModel> Search(string searchTerm)
        {
            var searchItems = SearchHelpers.GetSearchItems(searchTerm);

            var categories = this.categoryRepository.All();

            foreach (var item in searchItems)
            {
                categories = categories.Where(p =>
                p.Name.ToLower().Contains(item) ||
                p.Description.ToLower().Contains(item));
            }

            return categories
                .Distinct()
                .OrderByDescending(p => p.CreatedOn)
                .To<CategoryServiceModel>();
        }

        public Dictionary<string, List<string>> GetAvailableSortingCriteria()
        {
            return this.availableSortingCriteria;
        }

        public async Task<int> Create(CategoryServiceModel categoryServiceModel)
        {
            var category = AutoMapperConfig.MapperInstance.Map<BDInSelfLove.Data.Models.Category>(categoryServiceModel);

            await this.categoryRepository.AddAsync(category);
            await this.categoryRepository.SaveChangesAsync();

            return category.Id;
        }

        public async Task<CategoryServiceModel> GetById(int id, CategorySortingInputModel sortingModel)
        {
            var test = this.categoryRepository.All()
                .Where(x => x.Id == id)
                .Select(x => new CategoryServiceModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    UserId = x.UserId,
                    User = AutoMapperConfig.MapperInstance.Map<ApplicationUserServiceModel>(x.User),
                    Posts = SortPosts(
                        x.Posts.Select(p => new PostServiceModel
                        {
                            Id = p.Id,
                            CreatedOn = p.CreatedOn,
                            Title = p.Title,
                            UserId = p.UserId,
                            User = AutoMapperConfig.MapperInstance.Map<ApplicationUserServiceModel>(p.User),
                            CommentsCount = p.Comments.Count,
                        }).ToList(),
                        sortingModel),
                });

            return await test.FirstOrDefaultAsync();
        }

        public IQueryable<CategoryServiceModel> GetHomeCategoryInfo()
        {
            var result = this.categoryRepository.All().Select(c => new CategoryServiceModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                PostsCount = c.Posts.Count,
                CommentsCount = this.commentService.CommentsCountByCategoryId(c.Id),
                LastPost = c.Posts.OrderByDescending(p => p.CreatedOn)
                   .Select(p => new PostServiceModel
                   {
                       Title = p.Title,
                       Id = p.Id,
                       User = new Models.User.ApplicationUserServiceModel
                       {
                           UserName = p.User.UserName,
                       },
                       UserId = p.UserId,
                       CreatedOn = p.CreatedOn,
                   })
                   .FirstOrDefault(),
            });

            return result;
        }

        private static ICollection<PostServiceModel> GetCurrentDayPosts(ICollection<PostServiceModel> posts) =>
            posts.Where(c => c.CreatedOn.Day == DateTime.UtcNow.Day).ToList();

        private static ICollection<PostServiceModel> GetCurrentMonthPosts(ICollection<PostServiceModel> posts) =>
            posts.Where(c => c.CreatedOn.Month == DateTime.UtcNow.Month).ToList();

        private static ICollection<PostServiceModel> GetCurrentYearPosts(ICollection<PostServiceModel> query) =>
            query.Where(c => c.CreatedOn.Year == DateTime.UtcNow.Year).ToList();

        private static ICollection<PostServiceModel> OrderByDateCreatedDescending(ICollection<PostServiceModel> posts) =>
            posts.OrderByDescending(c => c.CreatedOn).ToList();

        private static ICollection<PostServiceModel> OrderByAuthorDescending(ICollection<PostServiceModel> query) =>
            query.OrderByDescending(c => c.User.UserName).ToList();

        private static ICollection<PostServiceModel> OrderByRepliesDescending(ICollection<PostServiceModel> query) =>
            query.OrderByDescending(c => c.CommentsCount).ToList();

        private static ICollection<PostServiceModel> OrderByTopicDescending(ICollection<PostServiceModel> query) =>
            query.OrderByDescending(c => c.Title).ToList();

        private static ICollection<PostServiceModel> OrderByDateCreatedAscending(ICollection<PostServiceModel> posts) =>
           posts.OrderBy(c => c.CreatedOn).ToList();

        private static ICollection<PostServiceModel> OrderByAuthorAscending(ICollection<PostServiceModel> query) =>
            query.OrderBy(c => c.User.UserName).ToList();

        private static ICollection<PostServiceModel> OrderByRepliesAscending(ICollection<PostServiceModel> query) =>
            query.OrderBy(c => c.CommentsCount).ToList();

        private static ICollection<PostServiceModel> OrderByTopicAscending(ICollection<PostServiceModel> query) =>
            query.OrderBy(c => c.Title).ToList();

        private static ICollection<PostServiceModel> SortPosts(ICollection<PostServiceModel> posts, CategorySortingInputModel sortingModel)
        {
            if (sortingModel.CategoryId != 0)
            {
                switch (sortingModel.TimeCriterion)
                {
                    case "day": posts = GetCurrentDayPosts(posts); break;
                    case "month": posts = GetCurrentMonthPosts(posts); break;
                    case "year": posts = GetCurrentYearPosts(posts); break;
                }

                switch (sortingModel.OrderingCriterion)
                {
                    case "descending":
                        switch (sortingModel.GroupingCriterion)
                        {
                            case "date created": posts = OrderByDateCreatedDescending(posts); break;
                            case "author": posts = OrderByAuthorDescending(posts); break;
                            case "replies": posts = OrderByRepliesDescending(posts); break;
                            case "topic": posts = OrderByTopicDescending(posts); break;
                        }

                        break;

                    case "ascending":
                        switch (sortingModel.GroupingCriterion)
                        {
                            case "date created": posts = OrderByDateCreatedAscending(posts); break;
                            case "author": posts = OrderByAuthorAscending(posts); break;
                            case "replies": posts = OrderByRepliesAscending(posts); break;
                            case "topic": posts = OrderByTopicAscending(posts); break;
                        }

                        break;
                }
            }

            return posts;
        }
    }
}

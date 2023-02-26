namespace InSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data;
    using InSelfLove.Data.Models;
    using InSelfLove.Data.Repositories;
    using InSelfLove.Services.Data.Comments;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Videos;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class VideoServiceTests
    {
        private VideoService videoService;

        public VideoServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new ApplicationDbContext(options.Options);
            var videoRepository = new EfDeletableEntityRepository<Video>(dbContext);
            var commentRepository = new EfDeletableEntityRepository<Comment>(dbContext);
            var commentService = new CommentService(commentRepository);
            var videoService = new VideoService(videoRepository, commentService);
            this.videoService = videoService;
        }

        // Create
        [Fact]
        public async Task CreateReturnsSlug()
        {
            var video = new Video()
            {
                Title = "TEST Test",
                Url = "test",
                AssociatedTerms = "test",
                Slug = "test-test",
            };

            var slug = await this.videoService.Create(video);
            Assert.True(slug.Equals(video.Title.ToLower().Replace(' ', '-')));
        }

        [Theory]
        [InlineData(null, "test", "test")]
        [InlineData("test", null, "test")]
        [InlineData("test", "test", null)]
        [InlineData("", "test", "test")]
        [InlineData("test", "", "test")]
        [InlineData("test", "test", "")]
        [InlineData("   ", "test", "test")]
        [InlineData("test", "   ", "test")]
        [InlineData("test", "test", "   ")]
        public async Task CreateIncompleteVideoThrowsArgumentException(string title, string url, string associatedTerms)
        {
            var video = new Video()
            {
                Title = title,
                Url = url,
                AssociatedTerms = associatedTerms,
            };

            await Assert.ThrowsAsync<ArgumentException>(() => this.videoService.Create(video));
        }

        [Fact]
        public async Task CreateWithSameIdTwiceThrowsArgumentException()
        {
            var video = new Video()
            {
                Id = 1,
                Title = "test",
                Url = "test",
                AssociatedTerms = "test",
                Slug = "test",
            };

            var slug = await this.videoService.Create(video);
            await Assert.ThrowsAsync<ArgumentException>(() => this.videoService.Create(video));
        }

        [Fact]
        public async Task CreateWithNullArgumentThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => this.videoService.Create(null));
        }

        // GetBySlug
        [Fact]
        public async Task GetBySlugReturnsCorrectVideo()
        {
            await this.ClearVideoRepository();

            var video = new Video()
            {
                Title = "test",
                Url = "test",
                AssociatedTerms = "test",
                Slug = "test",
            };

            var slug = await this.videoService.Create(video);
            var dbVideo = await this.videoService.GetBySlug(slug, null);

            Assert.Equal(video.Title, dbVideo.Title);
            Assert.Equal(video.Url, dbVideo.Url);
        }

        [Fact]
        public async Task GetBySlugReturnsCorrectVideoAndComments()
        {
            await this.ClearVideoRepository();

            var video = new Video()
            {
                Title = "test",
                Url = "test",
                AssociatedTerms = "test",
                Slug = "test",
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Content = "test test",
                        VideoId = 1,
                        User = new ApplicationUser
                        {
                            UserName = "Pesho",
                            ProfilePhoto = "test",
                        },
                    },
                    new Comment
                    {
                        Content = "test",
                        VideoId = 1,
                        User = new ApplicationUser
                        {
                            UserName = "Pesho1",
                            ProfilePhoto = "test1",
                        },
                    },
                },
            };

            var slug = await this.videoService.Create(video);
            var dbVideo = await this.videoService.GetBySlug(slug, null);

            var videoComments = video.Comments.OrderByDescending(c => c.CreatedOn).ToArray();
            var dbVideoComments = dbVideo.Comments.ToArray();

            for (int i = 0; i < videoComments.Length; i++)
            {
                Assert.Equal(videoComments[i].Content, dbVideoComments[i].Content);
                Assert.Equal(TimezoneHelper.ToLocalTime(videoComments[i].CreatedOn, null), dbVideoComments[i].CreatedOn);
                Assert.Equal(videoComments[i].User.UserName, dbVideoComments[i].User.UserName);
                Assert.Equal(videoComments[i].User.ProfilePhoto, dbVideoComments[i].User.ProfilePhoto);
            }
        }

        [Theory]
        [InlineData("does-not-exist")]
        [InlineData("   ")]
        [InlineData("---")]
        [InlineData(null)]
        public async Task GetBySlugReturnsNullWhenRequestingNonExistentVideo(string slug)
        {
            await this.ClearVideoRepository();

            var nonExistentVideo = await this.videoService.GetBySlug(slug, null);

            Assert.Null(nonExistentVideo);
        }

        // Delete
        [Fact]
        public async Task DeleteIsSuccessfulWithValidId()
        {
            await this.ClearVideoRepository();

            var video = new Video()
            {
                Title = "test",
                Url = "test",
                AssociatedTerms = "test",
                Slug = "test",
            };

            var slug = await this.videoService.Create(video);
            var dbVideo = await this.videoService.GetBySlug(slug, null);
            var result = await this.videoService.Delete(dbVideo.Id);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task DeleteThrowsArgumentExceptionWithInvaildId()
        {
            await this.ClearVideoRepository();

            await Assert.ThrowsAsync<ArgumentException>(() => this.videoService.Delete(1));
        }

        // GetAll
        [Fact]
        public async Task GetAllWithNoParametersReturnsAllVideos()
        {
            await this.ClearVideoRepository();
            var videos = (await this.SeedData()).OrderByDescending(x => x.CreatedOn).ToList();
            var dbVideos = await this.videoService.GetAll().ToListAsync();

            Assert.Equal(videos.Count, dbVideos.Count);

            for (int i = 0; i < videos.Count; i++)
            {
                Assert.Equal(videos[i].Title, dbVideos[i].Title);
                Assert.Equal(videos[i].Url, dbVideos[i].Url);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        public async Task GetAllSkipsAndTakesCorrectly(int take, int skip)
        {
            await this.ClearVideoRepository();
            var videos = (await this.SeedData()).OrderByDescending(x => x.CreatedOn).Skip(skip).Take(take).ToList();
            var dbVideos = await this.videoService.GetAll(take, skip).ToListAsync();

            Assert.Equal(videos.Count, dbVideos.Count);

            for (int i = 0; i < videos.Count; i++)
            {
                Assert.Equal(videos[i].Title, dbVideos[i].Title);
                Assert.Equal(videos[i].Url, dbVideos[i].Url);
            }
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test1")]
        [InlineData("test2")]
        [InlineData("test2 test1")]
        public async Task GetAllSearchesCorrectly(string searchString)
        {
            await this.ClearVideoRepository();
            var videos = await this.SeedData();
            var dbVideos = await this.videoService.GetAll(null, 0, searchString).ToListAsync();

            var filteredVideos = new List<Video>();
            string[] searchTermsArray = SearchHelper.GetSearchItems(searchString);

            foreach (var term in searchTermsArray)
            {
                var videosThatIncludeTerm = videos.Where(a => a.Title.Contains(term));
                filteredVideos.AddRange(videosThatIncludeTerm);
            }

            filteredVideos = filteredVideos.Distinct().OrderByDescending(x => x.CreatedOn).ToList();

            Assert.Equal(filteredVideos.Count, dbVideos.Count);

            for (int i = 0; i < filteredVideos.Count; i++)
            {
                Assert.Equal(filteredVideos[i].Title, dbVideos[i].Title);
                Assert.Equal(filteredVideos[i].Url, dbVideos[i].Url);
            }
        }

        // GetSideVideos
        [Fact]
        public async Task GetSideVideosReturnsCorrectData()
        {
            var count = 3;

            await this.ClearVideoRepository();
            await this.SeedData();
            var seedData = this.GetSeedData();

            var dbVideos = (await this.videoService.GetSideVideos(count, seedData[0].CreatedOn)).ToList();

            var range = seedData.GetRange(1, count);
            Assert.True(!dbVideos.Any(a => a.Id == seedData[0].Id));
            Assert.True(dbVideos.Count() == count);
            Assert.True(range.All(rangeVid => dbVideos.Any(v => rangeVid.Title == v.Title)));

            var dbVideos1 = (await this.videoService.GetSideVideos(count, seedData[seedData.Count - 1].CreatedOn)).ToList();
            var range1 = dbVideos.GetRange(dbVideos1.Count - count, count);
            Assert.True(range1.All(rangeVid => dbVideos1.Any(v => rangeVid.Title == v.Title)));
        }

        private async Task ClearVideoRepository()
        {
            var allEntries = await this.videoService.GetAll().ToListAsync();

            foreach (var video in allEntries)
            {
                await this.videoService.Delete(video.Id);
            }
        }

        private async Task<List<Video>> SeedData()
        {
            var videos = GetSeedData();
            foreach (var video in videos)
            {
                await this.videoService.Create(video);
            }

            return videos;
        }

        private List<Video> GetSeedData()
        {
            var date = new DateTime(2020, 2, 2);

            return new List<Video>
            {
                new Video()
                {
                    Title = "test",
                    Url = "test",
                    AssociatedTerms = "test",
                    Slug = "test",
                    CreatedOn = date,
                },
                new Video()
                {
                    Title = "test1",
                    Url = "test1",
                    AssociatedTerms = "test1",
                    Slug = "test1",
                    CreatedOn = date.Date.AddDays(1),
                },
                new Video()
                {
                    Title = "test2",
                    Url = "test2",
                    AssociatedTerms = "test2",
                    Slug = "test2",
                    CreatedOn = date.Date.AddDays(2),
                },
                new Video()
                {
                    Title = "test3",
                    Url = "test3",
                    AssociatedTerms = "test3",
                    Slug = "test3",
                    CreatedOn = date.Date.AddDays(3),
                },
                new Video()
                {
                    Title = "test4",
                    Url = "tes4",
                    AssociatedTerms = "test4",
                    Slug = "tes4",
                    CreatedOn = date.Date.AddDays(4),
                },
            };
        }
    }
}

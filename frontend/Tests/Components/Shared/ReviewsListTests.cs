using Bunit;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using FluentAssertions;

namespace GpuShare.Frontend.Tests.Components.Shared
{
    public class ReviewsListTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IReviewService> _reviewServiceMock = new();
        private readonly Review review1 = new Review()
        {
            Id = 1,
            DeviceId = 123,
            AuthorUsername = "User1",
            Rating = 4,
            Comment = "Great GPU, I needed exaxtly this",
            CreatedAt = DateTime.Now.AddDays(-1)
        };
        private readonly Review review2 = new Review()
        {
            Id = 3,
            DeviceId = 123,
            AuthorUsername = "Ileavelongreviews",
            Rating = 5,
            Comment = "I rented this GPU for a machine learning project and overall the experience was excellent. "
                + "The device was available exactly at the scheduled time, the connection process was straightforward, "
                + "and the performance matched the specifications listed on the platform. Training times were consistent, "
                + "GPU utilization remained stable throughout the session, and I did not encounter any unexpected disconnects. "
                + "The owner was responsive and answered my questions quickly. I would definitely rent this device again "
                + "for future workloads and would recommend it to anyone looking for reliable GPU resources.",
            CreatedAt = DateTime.Now.AddDays(-2)
        };
        private readonly Review review3 = new Review()
        {
            Id = 3,
            DeviceId = 123,
            AuthorUsername = "jonh",
            Rating = 1,
            Comment = "I am just a hater lol",
            CreatedAt = DateTime.Now.AddHours(-5)
        };
        private readonly Review newReview = new Review()
        {
            Id = 4,
            DeviceId = 123,
            AuthorUsername = "newUser",
            Rating = 1,
            Comment = "This is a brand new review just added",
            CreatedAt = DateTime.Now
        };

        public ReviewsListTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_reviewServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3],
                    Page = 1,
                    TotalCount = 3,
                    PageSize = 5
                }
            );

            _reviewServiceMock.Setup(x => x.GetUserReviewsAsync(It.IsAny<string>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3],
                    Page = 1,
                    TotalCount = 3,
                    PageSize = 5
                }
            );
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Empty_Reviews_Should_Show_Empty_Message()
        {
            var cut = Render<ReviewsList>();

            cut.Markup.Contains("No reviews yet.");
        }

        [Fact]
        public async Task Username_Should_Load_User_Reviews()
        {
            var cut = Render<ReviewsList>(p => p.Add(x => x.Username, "john"));

            _reviewServiceMock.Verify(x => x.GetUserReviewsAsync("john"), Times.Once);
        }

        [Fact]
        public async Task DeviceId_Should_Load_Device_Reviews()
        {
            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            _reviewServiceMock.Verify(x => x.GetDeviceReviewsAsync(123), Times.Once);
        }

        [Fact]
        public void Review_Should_Render_Author_Rating_And_Comment()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            cut.Markup.Contains("User1");
            cut.Markup.Contains("Great GPU");
        }

        [Fact]
        public void Review_Should_Render_Correct_Start_Count()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            cut.Markup.Contains("★★★★☆");
        }

        [Fact]
        public void Long_Review_Should_Show_ReadMore_Button()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review2],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            cut.Markup.Contains("Read more...");
        }

        [Fact]
        public void Short_Review_Should_Not_Show_ReadMore_Button()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            cut.Markup.Should().NotContain("Read more...");
        }

        [Fact]
        public void ReadMore_Should_Expand_Review()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review2],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));
            cut.Find(".toggle-text-btn").Click();

            cut.Markup.Contains("Show less");
        }

        [Fact]
        public void ShowLess_Should_Collapse_Review()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review2],
                    Page = 1,
                    TotalCount = 1,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));
            cut.Find(".toggle-text-btn").Click();
            cut.Find(".toggle-text-btn").Click();

            cut.Markup.Contains("Show less");
        }

        [Fact]
        public void LoadMore_Should_Append_Reviews()
        {
            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3, newReview],
                    Page = 1,
                    TotalCount = 4,
                    PageSize = 5
                }
            );

            cut.Markup.Contains("User1");
            cut.Markup.Contains("Ileavelongreviews");
            cut.Markup.Contains("jonh");
            cut.Markup.Contains("newUser");
        }

        [Fact]
        public void LoadMore_Should_Request_Next_Page()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1, 3)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3],
                    Page = 1,
                    TotalCount = 3,
                    PageSize = 3
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123).Add(x => x.PageSize, 3));

            var reviewsCards = cut.FindAll(".review-card");
            reviewsCards.Should().HaveCount(3);

            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 2, 3)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [newReview],
                    Page = 2,
                    TotalCount = 1,
                    PageSize = 3
                }
            );

            cut.Find(".load-more-link").Click();

            reviewsCards = cut.FindAll(".review-card");

            reviewsCards.Should().HaveCount(4);
            _reviewServiceMock.Verify(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public void LoadMore_Should_Not_Add_Duplicate_Reviews()
        {
            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            var reviewsCards = cut.FindAll(".review-card");
            reviewsCards.Should().HaveCount(3);

            _reviewServiceMock.Invocations.Clear();
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3, newReview],
                    Page = 1,
                    TotalCount = 4,
                    PageSize = 5
                }
            );

            cut.Find(".load-more-link").Click();

            reviewsCards = cut.FindAll(".review-card");

            reviewsCards.Should().HaveCount(4);
            cut.Markup.Should().Contain("newUser");
            cut.Markup.Should().Contain("Ileavelongreviews");
            cut.Markup.Should().Contain("User1");
            cut.Markup.Should().Contain("jonh");
        }

        [Fact]
        public void Reviews_Should_Be_Sorted_By_Newest()
        {
            _reviewServiceMock.Setup(x => x.GetDeviceReviewsAsync(It.IsAny<int>(), 1)).ReturnsAsync(
                new PagedResult<Review>()
                {
                    Items = [review1, review2, review3, newReview],
                    Page = 1,
                    TotalCount = 4,
                    PageSize = 5
                }
            );

            var cut = Render<ReviewsList>(p => p.Add(x => x.DeviceId, 123));

            var reviewsCards = cut.FindAll(".review-card");

            reviewsCards.Should().HaveCount(4);
            reviewsCards[0].InnerHtml.Should().Contain("newUser");
            reviewsCards[1].InnerHtml.Should().Contain("jonh");
            reviewsCards[2].InnerHtml.Should().Contain("User1");
            reviewsCards[3].InnerHtml.Should().Contain("Ileavelongreviews");
        }
    }
}

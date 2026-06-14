using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GpuShare.Frontend.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<ReviewService> _logger;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<ReviewService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new ReviewService(_apiClient, _logger);
        }

        private readonly CreateReviewRequest _createReviewRequest = new()
        {
            Rating = 4,
            Comment = "Great GPU, I needed exactly this"
        };

        private readonly string _createReviewResponseJson = """
            {
              "reviewId": 1,
              "authorUsername": "user1",
              "createdAt": "2026-06-06T12:00:00Z"
            }
            """;

        private readonly List<Review> _reviews = [ 
            new() {
                ReviewId = 1,
                OrderId = 456,
                AuthorUsername = "user1",
                Rating = 4,
                Comment = "Great GPU, I needed exactly this",
                CreatedAt = DateTime.Now.AddDays(-1)
            },
            new() {
                ReviewId = 3,
                OrderId = 457,
                AuthorUsername = "Ileavelongreviews",
                Rating = 5,
                Comment = "I rented this GPU for a machine learning project and overall the experience was excellent. "
                    + "The device was available exactly at the scheduled time, the connection process was straightforward, "
                    + "and the performance matched the specifications listed on the platform. Training times were consistent, "
                    + "GPU utilization remained stable throughout the session, and I did not encounter any unexpected disconnects. "
                    + "The owner was responsive and answered my questions quickly. I would definitely rent this device again "
                    + "for future workloads and would recommend it to anyone looking for reliable GPU resources.",
                CreatedAt = DateTime.Now.AddDays(-2)
            }
        ];

        private readonly string _reviewsJson = """
             [{
              "reviewId": 1,
              "orderId": "456",
              "authorUsername": "user1",
              "rating": 4,
              "comment": "Great GPU, I needed exactly this",
              "createdAt": "2026-06-05T12:00:00Z"
            },
            {
              "reviewId": 3,
              "orderId": "457",
              "authorUsername": "Ileavelongreviews",
              "rating": 5,
              "comment": "I rented this GPU for a machine learning project and overall the experience was excellent. The device was available exactly at the scheduled time, the connection process was straightforward, and the performance matched the specifications listed on the platform. Training times were consistent, GPU utilization remained stable throughout the session, and I did not encounter any unexpected disconnects. The owner was responsive and answered my questions quickly. I would definitely rent this device again for future workloads and would recommend it to anyone looking for reliable GPU resources.",
              "createdAt": "2026-06-04T12:00:00Z"
            }
            ]
            """;

        private readonly UserRatingDto _userRating = new()
        {
            AverageRating = 4.56,
            RatingCount = 21
        };

        private readonly string _userRatingJson = """
            {
              "averageRating": "4.56",
              "ratingCount": 21
            }
            """;

        // =====================================================
        // CREATE REVIEW
        // =====================================================

        [Fact]
        public async Task CreateReviewAsync_Should_Send_Correct_Request()
        {
            await ApiContract<CreateReviewRequest, Review>
                .Post(_mockHttp, () => _sut.CreateReviewAsync(456, _createReviewRequest))
                .To("/api/orders/reviewOrder/456")
                .Returns(_createReviewResponseJson)
                .WithBody(_createReviewRequest)
                .ShouldSendBody(body =>
                {
                    body.Rating.Should().Be(4);
                    body.Comment.Should().Be("Great GPU, I needed exactly this");
                });
        }

        [Fact]
        public async Task CreateReviewAsync_Should_Map_Response()
        {
            await ApiContract<CreateReviewRequest, Review>
                .Post(_mockHttp, () => _sut.CreateReviewAsync(456, _createReviewRequest))
                .To("/api/orders/reviewOrder/456")
                .Returns(_createReviewResponseJson)
                .ShouldMapTo(_reviews[0]);
        }

        [Fact]
        public async Task CreateReviewAsync_Should_Throw_When_Request_Invalid()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/orders/reviewOrder/456")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.CreateReviewAsync(456, _createReviewRequest),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReviewAsync_Should_Throw_When_User_Not_Logged_In()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/orders/reviewOrder/456")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.CreateReviewAsync(456, _createReviewRequest),
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReviewAsync_Should_Throw_When_Review_Already_Exists()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/orders/reviewOrder/456")
                .Respond(HttpStatusCode.Conflict);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.CreateReviewAsync(456, _createReviewRequest),
                HttpStatusCode.Conflict);
        }

        // =====================================================
        // GET DEVICE REVIEWS
        // =====================================================

        [Fact]
        public async Task GetDeviceReviewsAsync_Should_Call_Correct_Endpoint()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp, () => _sut.GetDeviceReviewsAsync(123))
                .To("/api/reviews/device/123")
                .Returns(_reviewsJson)
                .ExecuteAction();
        }

        [Fact]
        public async Task GetDeviceReviewsAsync_Should_Convert_Pagination_To_Query()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp, () => _sut.GetDeviceReviewsAsync(deviceId: 123, page: 2, count: 25))
                .To("/api/reviews/device/123")
                .Returns(_reviewsJson)
                .ExpectQuery(q =>
                {
                    q["page"].Should().Be("2");
                    q["count"].Should().Be("25");
                }).ExecuteAction();
        }

        [Fact]
        public async Task GetDeviceReviewsAsync_Should_Map_Response()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp, () => _sut.GetDeviceReviewsAsync(123, page: 2, count: 25))
                .To("/api/reviews/device/123")
                .Returns(_reviewsJson)
                .ShouldMapTo(new PagedResult<Review>()
                {
                    Items = _reviews,
                    Page = 1,
                    PageSize = 50,
                    TotalCount = 2
                });
        }

        [Fact]
        public async Task GetDeviceReviewsAsync_Should_Throw_ApiException_When_Device_Not_Found()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/reviews/device/123*")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetDeviceReviewsAsync(123),
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDeviceReviewsAsync_Should_Throw_ApiException_On_Server_Error()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/reviews/device/123*")
                .Respond(HttpStatusCode.InternalServerError);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetDeviceReviewsAsync(123),
                HttpStatusCode.InternalServerError);
        }

        // =====================================================
        // GET USER REVIEWS
        // =====================================================

        [Fact]
        public async Task GetUserReviewsAsync_Should_Use_Username_In_Route()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp, () => _sut.GetUserReviewsAsync("john"))
                .To("/api/reviews/user/john")
                .Returns(_reviewsJson).ExecuteAction();
        }

        [Fact]
        public async Task GetUserReviewsAsync_Should_Send_Pagination()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp, () => _sut.GetUserReviewsAsync("john", 3, 50))
                .To("/api/reviews/user/john")
                .Returns(_reviewsJson)
                .ExpectQuery(q =>
                {
                    q["page"].Should().Be("3");
                    q["count"].Should().Be("50");
                }).ExecuteAction();
        }

        [Fact]
        public async Task GetUserReviewsAsync_Should_Map_Response()
        {
            await ApiContract<object, PagedResult<Review>>
                .Get(_mockHttp,
                    () => _sut.GetUserReviewsAsync("john", 3, 50))
                .To("/api/reviews/user/john")
                .Returns(_reviewsJson)
                .ShouldMapTo(new PagedResult<Review>()
                {
                    Items = _reviews,
                    Page = 1,
                    PageSize = 150,
                    TotalCount = 2
                });
        }

        [Fact]
        public async Task GetUserReviewsAsync_Should_Throw_When_User_Not_Found()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/reviews/user/john*")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetUserReviewsAsync("john"),
                HttpStatusCode.NotFound);
        }

        // =====================================================
        // GET USER RATING
        // =====================================================

        [Fact]
        public async Task GetUserRatingAsync_Should_Call_Correct_Endpoint()
        {
            await ApiContract<object, UserRatingDto>
                .Get(_mockHttp, () => _sut.GetUserRatingAsync("john"))
                .To("/api/reviews/userRating/john")
                .Returns(_userRatingJson)
                .ExecuteAction();
        }

        [Fact]
        public async Task GetUserRatingAsync_Should_Map_Response()
        {
            await ApiContract<object, UserRatingDto>
                .Get(_mockHttp, () => _sut.GetUserRatingAsync("john"))
                .To("/api/reviews/userRating/john")
                .Returns(_userRatingJson)
                .ShouldMapTo(_userRating);
        }

        [Fact]
        public async Task GetUserRatingAsync_Should_Throw_When_User_Not_Found()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/reviews/userRating/john")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetUserRatingAsync("john"),
                HttpStatusCode.NotFound);
        }
    }
}

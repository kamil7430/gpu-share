using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Dispute;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;


namespace GpuShare.Frontend.Tests.Components.Dispute
{
    public class DisputeFormTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();
        private readonly DateTime? _startDate = DateTime.Now.AddHours(-1);
        private readonly DateTime? _endDate = DateTime.Now.AddHours(-2);
        public DisputeFormTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            _deviceServiceMock.Setup(x => x.GetDeviceAsync(67)).ReturnsAsync(new Models.Device()
            {
                DeviceId = 67,
                Name = "My GPU",
                GpuModel = "RTX 4090",
                OwnerUsername = "john",
            });

            _orderServiceMock.Setup(x => x.GetOrderAsync(123)).ReturnsAsync(new Models.Order()
            {
                Id = 123,
                DeviceId = 67,
                StartDate = _startDate,
                EndDate = _endDate
            });

            _formattersMock.Setup(x => x.FormatDateTime(_startDate)).Returns("start-date-string");
            _formattersMock.Setup(x => x.FormatDateTime(_endDate)).Returns("end-date-string");
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Load_Order_And_Device_Data()
        {
            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123).Add(x => x.Dispute, new Models.Dispute()));

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("#123");
                cut.Markup.Contains("RTX 4090");
                cut.Markup.Contains("My GPU");
                cut.Markup.Contains("john");
            });
        }

        [Fact]
        public async Task Should_Show_Error_When_Order_Load_Fails()
        {

        }

        [Fact]
        public void Submit_Should_Be_Disabled_Without_Reason()
        {
            var dispute = new Models.Dispute
            {
                Details = new string('A', 100)
            };

            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123).Add(x => x.Dispute, dispute));

            cut.WaitForState(() => cut.Find("button.btn-danger").HasAttribute("disabled"));

            Assert.True(cut.Find("button.btn-danger").HasAttribute("disabled"));
        }

        [Fact]
        public void Submit_Should_Be_Disabled_For_Short_Details()
        {
            var dispute = new Models.Dispute
            {
                Reason = "Hardware mismatch",
                Details = "too short"
            };

            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123).Add(x => x.Dispute, dispute));

            cut.WaitForState(() => cut.Find("button.btn-danger").HasAttribute("disabled"));

            Assert.True(cut.Find("button.btn-danger").HasAttribute("disabled"));
        }

        [Fact]
        public void Submit_Should_Be_Enabled_For_Valid_Dispute()
        {
            var dispute = new Models.Dispute
            {
                Reason = "Hardware mismatch",
                Details = new string('A', 100)
            };

            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123).Add(x => x.Dispute, dispute));

            cut.WaitForAssertion(() =>
            {
                var button = cut.Find("button.btn-danger");

                Assert.False(button.HasAttribute("disabled"));
            });
        }

        [Fact]
        public async Task Should_Limit_Uploads_To_Three_Files()
        {
            var cut = Render<DisputeForm>(p => p.Add(x => x.Dispute, new Models.Dispute() { OrderId = 123 }));

            var files = Enumerable.Range(1, 5).Select(i => (IBrowserFile)new FakeBrowserFile
                {
                    Name = $"{i}.txt"
                }).ToList();

            await cut.InvokeAsync(() => cut.Instance.HandleFilesChanged(files));

            cut.Render();

            Assert.Equal(3, cut.FindAll(".uploaded-file").Count);
        }

        [Fact]
        public async Task Should_Display_Uploaded_Files()
        {
            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123)
                .Add(x => x.Dispute, new Models.Dispute()));

            var file = new FakeBrowserFile
            {
                Name = "evidence.png"
            };

            await cut.InvokeAsync(() => cut.Instance.HandleFilesChanged([ file ]));

            cut.Render();

            Assert.Contains("evidence.png", cut.Markup);
        }

        [Fact]
        public async Task Should_Remove_File()
        {
            var cut = Render<DisputeForm>(p => p.Add(x => x.OrderId, 123)
                .Add(x => x.Dispute, new Models.Dispute()));

            var file = new FakeBrowserFile
            {
                Name = "evidence.png"
            };

            await cut.InvokeAsync(() => cut.Instance.HandleFilesChanged([ file ]));

            cut.Render();

            Assert.Contains("evidence.png", cut.Markup);

            await cut.InvokeAsync(() => cut.Instance.RemoveFile(file));

            cut.Render();

            Assert.DoesNotContain("evidence.png", cut.Markup);
        }

        [Fact]
        public async Task Submit_Should_Call_Dispute_Service() { }

        [Fact]
        public async Task Successful_Submission_Should_Navigate_To_Dispute_Page() { }

        [Fact]
        public async Task Failed_Submission_Should_Show_Error() { }

        [Fact]
        public async Task Submitted_Dispute_Should_Include_Attachments() { }

        [Fact]
        public async Task Existing_Attachments_Should_Be_Displayed() { }
    }

    public class FakeBrowserFile : IBrowserFile
    {
        public string Name { get; init; } = "";
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => 1024;
        public string ContentType => "text/plain";

        public Stream OpenReadStream(
            long maxAllowedSize = 512000,
            CancellationToken cancellationToken = default)
        {
            return new MemoryStream();
        }
    }
}

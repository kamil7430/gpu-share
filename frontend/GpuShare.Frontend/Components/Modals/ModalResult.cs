using GpuShare.Frontend.Models;
using Microsoft.AspNetCore.Components;

namespace GpuShare.Frontend.Components.Modals
{
    public class BaseModalState<TResult> : ComponentBase
    {
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public string Title { get; set; } = "";

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public RenderFragment? FooterContent { get; set; }

        [Parameter]
        public EventCallback<ModalResult<TResult>> OnClose { get; set; }

        //[Parameter]
        //public EventCallback<ModalResult<TResult>> OnCompleted { get; set; }

        [Parameter]
        public bool CloseOnBackdrop { get; set; } = true;

        [Parameter]
        public bool ShowCloseButton { get; set; } = true;

        [Parameter]
        public string SizeClass { get; set; } = "";

        protected async Task HandleBackdropClick()
        {
            if (CloseOnBackdrop)
            {
                await Close();
            }
        }

        protected async Task Close()
        {
            await OnClose.InvokeAsync(ModalResult<TResult>.Cancel());
        }
    }

    public enum ModalResultStatus
    {
        Success,
        Cancelled,
        Failed
    }

    public sealed record ModalResult<T>(
        ModalResultStatus Status,
        T? Data = default,
        string? Message = null)
    {
        public static ModalResult<T> Ok(T? data = default, string? message = null)
            => new(ModalResultStatus.Success, data, message);

        public static ModalResult<T> Cancel(string? message = null)
            => new(ModalResultStatus.Cancelled, default, message);

        public static ModalResult<T> Fail(string? message)
            => new(ModalResultStatus.Failed, default, message);
    }
}

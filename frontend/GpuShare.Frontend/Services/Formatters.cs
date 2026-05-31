using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class Formatters : IFormatters
    {
        public string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy H:mm");
        }
        public string FormatDateTime(DateTime? dateTime)
        {
            if (dateTime != null)
                return dateTime.Value.ToString("dd.MM.yyyy H:mm");
            else return "N/A";
        }

        public string FormatDuration(int totalSeconds)
        {
            throw new NotImplementedException();
        }

        public string FormatOrderStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.WaitingForStart => "Waiting for start",
                OrderStatus.Running => "Running",
                OrderStatus.Completed => "Completed",
                OrderStatus.Failure => "Failure",
                OrderStatus.Suspended => "Suspended",
                _ => ""
            };
        }

        public string FormatUsd(decimal amount)
        {
            throw new NotImplementedException();
        }

        public string FormatVram(int mb)
        {
            throw new NotImplementedException();
        }
    }
}

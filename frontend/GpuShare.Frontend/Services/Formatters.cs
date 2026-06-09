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
                OrderStatus.WAITING_FOR_START => "Waiting for start",
                OrderStatus.RUNNING => "Running",
                OrderStatus.COMPLETED => "Completed",
                OrderStatus.FAILURE => "Failure",
                OrderStatus.SUSPENDED => "Suspended",
                _ => ""
            };
        }

        public string FormatUsd(int amountInCents)
        {
            return $"${amountInCents / 100.0:0.00}";
        }

        public string FormatVram(int mb)
        {
            throw new NotImplementedException();
        }
    }
}

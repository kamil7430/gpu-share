using GpuShare.Frontend.Models;

namespace GpuShare.Frontend.Services.Interfaces;
public interface IFormatters
{
    string FormatUsd(decimal amount);

    string FormatDuration(int totalSeconds);

    string FormatDateTime(DateTime dateTime);

    string FormatVram(int mb);

    string FormatOrderStatus(OrderStatus status);
}

// FormatUsd(0.45m)
// → "$0.45"

// FormatDuration(9252)
// → "2h 34m 12s"

// FormatDateTime(date)
// → "6 Jan 2026, 12:34"

// FormatVram(24576)
// → "24 GB"
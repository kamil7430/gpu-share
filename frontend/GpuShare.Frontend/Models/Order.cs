public class Order
{
    public string Device { get; set; }
    public string Owner { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public int Cost { get; set; }
    public string Status { get; set; }

    public Order(string device, string owner, string startDate, string endDate, int cost, string status)
    {
        Device = device;
        Owner = owner;
        StartDate = startDate;
        EndDate = endDate;
        Cost = cost;
        Status = status;
    }
};
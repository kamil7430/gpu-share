using System.ComponentModel.DataAnnotations;

public class DisputeModel
{
    public string Reason { get; set; } = "";

    [MinLength(50)]
    public string Details { get; set; } = "";
}
namespace GpuShare.Frontend.Models.Dtos
{
    public class FileUploadResult
    {
        public string Url { get; set; } = "";
        public string FileName { get; set; } = "";
        public long Size { get; set; }

        // optional but useful later
        public string? ContentType { get; set; }
        public string? StorageKey { get; set; }
    }
}

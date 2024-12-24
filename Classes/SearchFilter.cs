namespace Avatar_Explorer.Classes
{
    public class SearchFilter
    {
        public string Author { get; set; } = "";
        public string Title { get; set; } = "";
        public string BoothId { get; set; } = "";
        public string[] SearchWords { get; set; } = Array.Empty<string>();
    }
}

namespace Avatar_Explorer.Classes
{
    public class Item
    {
        public string Title { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string AuthorId { get; set; } = "";
        public int BoothId { get; set; } = 0;
        public string ItemPath { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string AuthorImageUrl { get; set; } = "";
        public string AuthorImageFilePath { get; set; } = "";
        public ItemType Type { get; set; }
        public string[] SupportedAvatar { get; set; } = Array.Empty<string>();
    }
}

namespace Avatar_Explorer.Classes
{
    public class CurrentPath
    {
        public string? CurrentSelectedAvatar;
        public string? CurrentSelectedAvatarPath;
        public Author? CurrentSelectedAuthor;
        public ItemType CurrentSelectedCategory = ItemType.Unknown;
        public Item? CurrentSelectedItem;
        public string? CurrentSelectedItemCategory;
        public ItemFolderInfo CurrentSelectedItemFolderInfo = new();
    }
}

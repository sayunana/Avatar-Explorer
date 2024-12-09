namespace Avatar_Explorer.Classes
{
    public class CurrentPath
    {
        public string CurrentSelectedAvatar = "";
        public Author? CurrentSelectedAuthor;
        public ItemType CurrentSelectedCategory;
        public Item? CurrentSelectedItem;
        public string CurrentSelectedItemCategory = "";
        public ItemFolderInfo CurrentSelectedItemFolderInfo = new();
    }
}

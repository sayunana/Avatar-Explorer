namespace Avatar_Explorer.Classes
{
    public class SharedImages
    {
        private static readonly Image FileImage = Image.FromStream(new MemoryStream(Properties.Resources.FileIcon));
        private static readonly Image FolderImage = Image.FromStream(new MemoryStream(Properties.Resources.FolderIcon));
        private static readonly Image CopyImage = Image.FromStream(new MemoryStream(Properties.Resources.CopyIcon));
        private static readonly Image TrashImage = Image.FromStream(new MemoryStream(Properties.Resources.TrashIcon));
        private static readonly Image EditImage = Image.FromStream(new MemoryStream(Properties.Resources.EditIcon));
        private static readonly Image OpenImage = Image.FromStream(new MemoryStream(Properties.Resources.OpenIcon));

        public enum Images
        {
            FileIcon,
            FolderIcon,
            CopyIcon,
            TrashIcon,
            EditIcon,
            OpenIcon
        };

        public static Image GetImage(Images images)
        {
            Image sharedImage = images switch
            {
                Images.FileIcon => FileImage,
                Images.FolderIcon => FolderImage,
                Images.CopyIcon => CopyImage,
                Images.TrashIcon => TrashImage,
                Images.EditIcon => EditImage,
                Images.OpenIcon => OpenImage,
                _ => throw new ArgumentOutOfRangeException(nameof(images), images, "共有画像の定義がありません")
            };
            return sharedImage;
        }

        public static bool IsSharedImage(Image image)
        {
            if(image == FileImage
                || image == FolderImage 
                || image == CopyImage 
                || image == TrashImage 
                || image == EditImage 
                || image == OpenImage)
                return true;
            return false;
        }
    }
}

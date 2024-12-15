namespace Avatar_Explorer.Classes
{
    public class ItemFolderInfo
    {
        public FileData[] ModifyFiles { get; set; } = Array.Empty<FileData>();
        public FileData[] TextureFiles { get; set; } = Array.Empty<FileData>();
        public FileData[] DocumentFiles { get; set; } = Array.Empty<FileData>();
        public FileData[] UnityPackageFiles { get; set; } = Array.Empty<FileData>();
        public FileData[] UnkownFiles { get; set; } = Array.Empty<FileData>();

        public int GetItemCount(string type)
        {
            return type switch
            {
                "改変用データ" => ModifyFiles.Length,
                "テクスチャ" => TextureFiles.Length,
                "ドキュメント" => DocumentFiles.Length,
                "Unityパッケージ" => UnityPackageFiles.Length,
                "不明" => UnkownFiles.Length,
                _ => 0
            };
        }

        public FileData[] GetItems(string? type)
        {
            return type switch
            {
                "改変用データ" => ModifyFiles,
                "テクスチャ" => TextureFiles,
                "ドキュメント" => DocumentFiles,
                "Unityパッケージ" => UnityPackageFiles,
                "不明" => UnkownFiles,
                _ => Array.Empty<FileData>()
            };
        }
    }

    public class FileData
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileExtension => Path.GetExtension(FilePath);
    }
}

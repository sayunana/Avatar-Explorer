using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Avatar_Explorer.Classes
{
    public class Helper
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly Dictionary<string, Dictionary<string, string>> TranslateData = new();

        public static async Task<Item> GetBoothItemInfoAsync(string id)
        {
            var url = $"https://booth.pm/ja/items/{id}.json";
            var response = await HttpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            var title = json["name"]?.ToString() ?? "";
            var author = json["shop"]?["name"]?.ToString() ?? "";
            var authorUrl = json["shop"]?["url"]?.ToString() ?? "";
            var imageUrl = json["images"]?[0]?["original"]?.ToString() ?? "";
            var authorIcon = json["shop"]?["thumbnail_url"]?.ToString() ?? "";
            var authorId = GetAuthorId(authorUrl);
            var category = json["category"]?["name"]?.ToString() ?? "";
            var estimatedCategory = GetItemType(title, category);

            return new Item
            {
                Title = title,
                AuthorName = author,
                ThumbnailUrl = imageUrl,
                AuthorImageUrl = authorIcon,
                AuthorId = authorId,
                Type = estimatedCategory
            };
        }

        private static string GetAuthorId(string url)
        {
            var match = Regex.Match(url, @"https://(.*).booth.pm/");
            return match.Success ? match.Groups[1].Value : "";
        }

        public static string GetCategoryName(ItemType itemType, string lang)
        {
            return itemType switch
            {
                ItemType.Avatar => Translate("アバター", lang),
                ItemType.Clothing => Translate("衣装", lang),
                ItemType.Texture => Translate("テクスチャ", lang),
                ItemType.Gimick => Translate("ギミック", lang),
                ItemType.Accessary => Translate("アクセサリー", lang),
                ItemType.HairStyle => Translate("髪型", lang),
                ItemType.Animation => Translate("アニメーション", lang),
                ItemType.Tool => Translate("ツール", lang),
                ItemType.Shader => Translate("シェーダー", lang),
                _ => Translate("不明", lang)
            };
        }

        public static ItemFolderInfo GetItemFolderInfo(string path, string materialPath)
        {
            var itemFolderInfo = new ItemFolderInfo();
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };
                switch (extension)
                {
                    case ".psd":
                    case ".clip":
                    case ".blend":
                    case ".fbx":
                        itemFolderInfo.ModifyFiles = itemFolderInfo.ModifyFiles.Append(item).ToArray();
                        break;
                    case ".png":
                    case ".jpg":
                        itemFolderInfo.TextureFiles = itemFolderInfo.TextureFiles.Append(item).ToArray();
                        break;
                    case ".txt":
                    case ".md":
                    case ".pdf":
                        itemFolderInfo.DocumentFiles = itemFolderInfo.DocumentFiles.Append(item).ToArray();
                        break;
                    case ".unitypackage":
                        itemFolderInfo.UnityPackageFiles = itemFolderInfo.UnityPackageFiles.Append(item).ToArray();
                        break;
                    default:
                        itemFolderInfo.UnkownFiles = itemFolderInfo.UnkownFiles.Append(item).ToArray();
                        break;
                }
            }

            if (string.IsNullOrEmpty(materialPath)) return itemFolderInfo;

            var materialFiles = Directory.GetFiles(materialPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in materialFiles)
            {
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                itemFolderInfo.MaterialFiles = itemFolderInfo.MaterialFiles.Append(item).ToArray();
            }

            return itemFolderInfo;
        }

        public static Button CreateButton(string imagePath, string labelTitle, string? description, bool @short = false, string tooltip = "")
        {
            var buttonWidth = @short ? 303 : 874;
            CustomItemButton button = new CustomItemButton(false, buttonWidth);
            button.Picture = ResizeImage(File.Exists(imagePath) ? imagePath : "./Datas/FileIcon.png", 100, 100);
            button.TitleText = labelTitle;
            if (description != null)
                button.AuthorName = description;
            if (!string.IsNullOrEmpty(tooltip))
                button.ToolTipText = tooltip;

            return button;
        }

        public static ItemType GetItemType(string title, string type)
        {
            var avatarTitle = new[] { "オリジナル3Dモデル", "オリジナル", "Avatar", "Original" };
            var animationTitle = new[] { "アニメーション", "Animation" };
            var clothingTitle = new[] { "衣装", "Clothing" };
            var gimickTitle = new[] { "ギミック", "Gimick" };
            var accessaryTitle = new[] { "アクセサリ", "Accessary" };
            var hairStyleTitle = new[] { "髪", "Hair" };
            var textureTitle = new[] { "テクスチャ", "Eye", "Texture" };
            var toolTitle = new[] { "ツール", "システム", "Tool", "System" };
            var shaderTitle = new[] { "シェーダー", "Shader" };

            var suggestType = type switch
            {
                "3Dキャラクター" => ItemType.Avatar,
                "3Dモーション・アニメーション" => ItemType.Animation,
                "3D衣装" => ItemType.Clothing,
                "3D装飾品" => ItemType.Accessary,
                "3Dテクスチャ" => ItemType.Texture,
                "3Dツール・システム" => ItemType.Tool,
                _ => ItemType.Unknown,
            };

            if (avatarTitle.Any(title.Contains))
            {
                suggestType = ItemType.Avatar;
            }

            if (animationTitle.Any(title.Contains))
            {
                suggestType = ItemType.Animation;
            }

            if (clothingTitle.Any(title.Contains))
            {
                suggestType = ItemType.Clothing;
            }

            if (gimickTitle.Any(title.Contains))
            {
                suggestType = ItemType.Gimick;
            }

            if (accessaryTitle.Any(title.Contains))
            {
                suggestType = ItemType.Accessary;
            }

            if (hairStyleTitle.Any(title.Contains))
            {
                suggestType = ItemType.HairStyle;
            }

            if (textureTitle.Any(title.Contains))
            {
                suggestType = ItemType.Texture;
            }

            if (toolTitle.Any(title.Contains))
            {
                suggestType = ItemType.Tool;
            }

            if (shaderTitle.Any(title.Contains))
            {
                suggestType = ItemType.Shader;
            }

            return suggestType;
        }

        public static string RemoveFormat(string str) => str.Replace(' ', '_').Replace('/', '-');

        public static void SaveItemsData(Item[] items)
        {
            using var sw = new StreamWriter("./Datas/ItemsData.json");
            sw.Write(JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static Item[] LoadItemsData()
        {
            if (!File.Exists("./Datas/ItemsData.json")) return Array.Empty<Item>();
            using var sr = new StreamReader("./Datas/ItemsData.json");
            var data = JsonSerializer.Deserialize<Item[]>(sr.ReadToEnd());
            return data ?? Array.Empty<Item>();
        }

        public static string GetItemImagePath(FileInfo file)
        {
            switch (file.Extension)
            {
                case ".png":
                case ".jpg":
                    return file.FullName;
                case ".psd":
                    return "./Datas/PhotoshopIcon.png";
                case ".blend":
                    return "./Datas/BlenderIcon.png";
                case ".fbx":
                    return "./Datas/FbxIcon.png";
                case ".unitypackage":
                    return "./Datas/UnityIcon.png";
                case ".txt":
                    return "./Datas/TxtIcon.png";
                case ".md":
                    return "./Datas/MarkdownIcon.png";
                case ".pdf":
                    return "./Datas/PdfIcon.png";
                default:
                    return "./Datas/FileIcon.png";
            }
        }

        public static void DragEnter(object _, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private static Image ResizeImage(string imagePath, int width, int height)
        {
            if (!File.Exists(imagePath)) return new Bitmap(width, height);
            using var originalImage = Image.FromFile(imagePath);
            var resizedImage = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resizedImage);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, width, height);
            return resizedImage;
        }

        public static string Translate(string str, string to)
        {
            if (to == "ja-JP") return str;
            if (!File.Exists($"./Datas/Translate/{to}.json")) return str;
            var data = GetTranslateData(to);
            return data.TryGetValue(str, out var translated) ? translated : str;
        }

        private static Dictionary<string, string> GetTranslateData(string lang)
        {
            if (TranslateData.TryGetValue(lang, out var data)) return data;
            var json = File.ReadAllText(($"./Datas/Translate/{lang}.json"));
            var translateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translateData == null) return new Dictionary<string, string>();
            TranslateData.Add(lang, translateData);
            return translateData;
        }
    }
}
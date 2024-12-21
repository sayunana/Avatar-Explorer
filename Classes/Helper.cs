using System.Text.Json;
using System.Text.RegularExpressions;

namespace Avatar_Explorer.Classes
{
    public class Helper
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly Dictionary<string, Dictionary<string, string>> TranslateData = new();

        public static async Task<Item> GetBoothItemInfoAsync(string id)
        {
            var url = $"https://booth.pm/ja/items/{id}";
            var response = await HttpClient.GetStringAsync(url);
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(response);

            var title = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='font-bold leading-[32px] m-0 text-[24px]']")?.InnerText?.Trim();
            title ??= "";
            title = title.Replace("&amp;", "＆");

            var authorNode =
                htmlDoc.DocumentNode.SelectSingleNode(
                    "//a[@data-product-list='from market_show via market_item_detail to shop_index']");
            var author = authorNode?.InnerText?.Trim() ?? "";
            var authorUrl = authorNode?.GetAttributeValue("href", null) ?? "";

            var imageUrl = htmlDoc.DocumentNode
                .SelectSingleNode("//meta[@name='twitter:image']")
                ?.GetAttributeValue("content", null) ?? "";

            var authorIcon = htmlDoc.DocumentNode.SelectSingleNode($"//img[@alt='{author}']")?.GetAttributeValue("src", null) ?? "";

            var authorId = GetAuthorId(authorUrl);

            return new Item
            {
                Title = title,
                AuthorName = author,
                ThumbnailUrl = imageUrl,
                AuthorImageUrl = authorIcon,
                AuthorId = authorId
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

        public static Button CreateButton(string imagePath, string labelTitle, string? deskription, bool @short = false, string tooltip = "")
        {
            var buttonWidth = @short ? 303 : 874;
            Button button = new Button();
            button.Size = new Size(buttonWidth, 64);

            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(56, 56);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Image = ResizeImage(File.Exists(imagePath) ? imagePath : "./Datas/FileIcon.png", 100, 100);
            pictureBox.Location = new Point(4, 4);
            button.Controls.Add(pictureBox);

            Label title = new Label();
            title.Text = labelTitle;
            title.Location = new Point(60, 3);
            title.AutoSize = true;
            title.Font = new Font("Yu Gothic UI", 12F);
            button.Controls.Add(title);

            Label authorName = new Label();
            authorName.Text = deskription;
            authorName.Location = new Point(60, 25);
            authorName.Size = new Size(200, 20);
            button.Controls.Add(authorName);

            pictureBox.Click += (_, _) => button.PerformClick();
            title.Click += (_, _) => button.PerformClick();
            authorName.Click += (_, _) => button.PerformClick();

            if (!string.IsNullOrEmpty(tooltip))
                new ToolTip().SetToolTip(button, tooltip);

            return button;
        }

        public static ItemType GetItemType(string title)
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

            var suggestType = ItemType.Unknown;

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
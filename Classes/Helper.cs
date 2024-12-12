using System.Text.RegularExpressions;

namespace Avatar_Explorer.Classes
{
    public class Helper
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task<Item> GetBoothItemInfoAsync(string id)
        {
            var url = $"https://booth.pm/ja/items/{id}";
            var response = await HttpClient.GetStringAsync(url);
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(response);

            var title = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='font-bold leading-[32px] m-0 text-[24px]']")?.InnerText?.Trim();
            title ??= "タイトル不明";

            var authorNode =
                htmlDoc.DocumentNode.SelectSingleNode(
                    "//a[@data-product-list='from market_show via market_item_detail to shop_index']");
            var author = authorNode?.InnerText?.Trim() ?? "作者不明";
            var authorUrl = authorNode?.GetAttributeValue("href", null) ?? "Not Found";

            var imageUrl = htmlDoc.DocumentNode
                .SelectSingleNode("//meta[@name='twitter:image']")
                ?.GetAttributeValue("content", null) ?? "Not Found";

            var authorIcon = htmlDoc.DocumentNode.SelectSingleNode($"//img[@alt='{author}']")?.GetAttributeValue("src", null) ?? "Not Found";

            return new Item
            {
                Title = title.Replace("&amp;", "＆"),
                AuthorName = author,
                ThumbnailUrl = imageUrl,
                AuthorImageUrl = authorIcon,
                AuthorId = GetAuthorId(authorUrl)
            };
        }

        private static string GetAuthorId(string url)
        {
            var match = Regex.Match(url, @"https://(.*).booth.pm/");
            return match.Success ? match.Groups[1].Value : "Not Found";
        }

        public static string GetCategoryName(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Avatar => "アバター",
                ItemType.Clothing => "衣装",
                ItemType.Texture => "テクスチャ",
                ItemType.Gimick => "ギミック",
                ItemType.Accessary => "アクセサリー",
                ItemType.HairStyle => "髪型",
                ItemType.Animation => "アニメーション",
                ItemType.Tool => "ツール",
                ItemType.Shader => "シェーダー",
                _ => "不明"
            };
        }

        public static ItemFolderInfo GetItemFolderInfo(string path)
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
                }
            }

            return itemFolderInfo;
        }

        public static Button CreateButton(string imagePath, string labelTitle, string? deskription, bool @short = false)
        {
            var buttonWidth = @short ? 319 : 882;
            Button button = new Button();
            button.Size = new Size(buttonWidth, 64);

            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(56, 56);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Image = Image.FromFile(File.Exists(imagePath) ? imagePath : "./Datas/FileIcon.png");
            pictureBox.Location = new Point(3, 3);
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

            if (avatarTitle.Any(title.Contains))
            {
                return ItemType.Avatar;
            }

            if (animationTitle.Any(title.Contains))
            {
                return ItemType.Animation;
            }

            if (clothingTitle.Any(title.Contains))
            {
                return ItemType.Clothing;
            }

            if (gimickTitle.Any(title.Contains))
            {
                return ItemType.Gimick;
            }

            if (accessaryTitle.Any(title.Contains))
            {
                return ItemType.Accessary;
            }

            if (hairStyleTitle.Any(title.Contains))
            {
                return ItemType.HairStyle;
            }

            if (textureTitle.Any(title.Contains))
            {
                return ItemType.Texture;
            }

            if (toolTitle.Any(title.Contains))
            {
                return ItemType.Tool;
            }

            if (shaderTitle.Any(title.Contains))
            {
                return ItemType.Shader;
            }

            return ItemType.Avatar;
        }

        public static string RemoveFormat(string str)
        {
            return str.Replace(' ', '_').Replace('/', '-');
        }
    }
}
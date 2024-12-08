using System.Diagnostics;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        public Item[] Items = Array.Empty<Item>();

        public CurrentPath CurrentPath = new();

        private bool _authorMode;

        public Main()
        {
            InitializeComponent();

            //test
            Items = Items.Append(new Item
            {
                Title = "真冬 Mafuyu / オリジナル3Dモデル",
                AuthorName = "ぷらすわん",
                AuthorImageFilePath = "./Datas/AuthorImage/ぷらすわん.jpg",
                BoothId = 0,
                ItemPath = "D:\\VRChat Avatars\\アバター本体\\まふゆ\\Mafuyu_v1.0.4",
                ThumbnailUrl = "",
                ImagePath = "./Datas/Thumbnail/5007531.png",
                Type = ItemType.Avatar
            }).ToArray();
            GenerateAvatarList();
            GenerateAuthorList();
        }

        // Generate List (LEFT)
        public void GenerateAvatarList()
        {
            AvatarPage.Controls.Clear();
            var index = 0;
            foreach (Item item in Items.Where(item => item.Type == ItemType.Avatar))
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName, true);
                button.Location = new Point(0, (70 * index) + 7);
                button.Click += (sender, e) =>
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAuthor = null;
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = "";
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarPage.Controls.Add(button);
                index++;
            }
        }

        public void GenerateAuthorList()
        {
            AvatarAuthorPage.Controls.Clear();
            var index = 0;

            var authors = Items.Select(item => new Author { AuthorName = item.AuthorName, AuthorImagePath = item.AuthorImageFilePath }).Distinct().ToArray();

            foreach (var author in authors)
            {
                Button button = Helper.CreateButton(author.AuthorImagePath, author.AuthorName, Items.Count(item => item.AuthorName == author.AuthorName) + "個の項目", true);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (sender, e) =>
                {
                    CurrentPath.CurrentSelectedAuthor = author;
                    CurrentPath.CurrentSelectedAvatar = "";
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = "";
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = true;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarAuthorPage.Controls.Add(button);
                index++;
            }
        }

        // Generate List (RIGHT)
        private void GenerateCategoryList()
        {
            AvatarItemExplorer.Controls.Clear();
            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;
                Button button = Helper.CreateButton("./Datas/FolderIcon.png", Helper.GetCategoryName(itemType), Items.Count(item =>
                {
                    if (_authorMode)
                        return item.Type == itemType && item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName;
                    return item.Type == itemType;
                }) + "個の項目");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (sender, e) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems(CurrentPath.CurrentSelectedCategory);
                    PathTextBox.Text = GeneratePath();
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItems(ItemType itemType)
        {
            AvatarItemExplorer.Controls.Clear();

            var index = 0;
            foreach (Item item in Items.Where(item => item.Type == itemType && (item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatar) || item.SupportedAvatar.Length == 0)))
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title, "作者: " + item.AuthorName);
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (sender, e) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                //Context Menu
                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new("Boothリンクのコピー");
                toolStripMenuItem.Click += (sender, e) =>
                {
                    Clipboard.SetText("https://booth.pm/ja/items/" + item.BoothId);
                };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemCategoryList()
        {
            var types = new[] {
                "改変用データ",
                "テクスチャ",
                "ドキュメント",
                "Unityパッケージ"
            };
            ItemFolderInfo itemFolderInfo = Helper.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath);
            CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

            AvatarItemExplorer.Controls.Clear();
            var index = 0;
            foreach (var itemType in types)
            {
                Button button = Helper.CreateButton("./Datas/FolderIcon.png", itemType, itemFolderInfo.GetItemCount(itemType) + "個の項目");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (sender, e) =>
                {
                    CurrentPath.CurrentSelectedItemCategory = itemType;
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemFiles()
        {
            AvatarItemExplorer.Controls.Clear();

            var index = 0;
            foreach (var file in CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory))
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "./Datas/FileIcon.png";
                Button button = Helper.CreateButton(imagePath, file.FileName, file.FileExtension.Replace(".", "") + "ファイル");
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (sender, e) =>
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + file.FilePath));
                };
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        // Add Item Form
        private void AddItemButton_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory);
            addItem.ShowDialog();
            GenerateAvatarList();
        }

        // Generate Path
        private string GeneratePath()
        {
            if (!_authorMode)
            {
                if (CurrentPath.CurrentSelectedAvatar == "") return "";
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown) return RemoveFormat(CurrentPath.CurrentSelectedAvatar);
                if (CurrentPath.CurrentSelectedItem == null) return RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory);
                if (CurrentPath.CurrentSelectedItemCategory == "") return RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" + RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return RemoveFormat(CurrentPath.CurrentSelectedAvatar) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" + RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + "/" + RemoveFormat(CurrentPath.CurrentSelectedItemCategory);
            }

            if (CurrentPath.CurrentSelectedAuthor == null) return "";
            if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown) return RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName);
            if (CurrentPath.CurrentSelectedItem == null) return RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory);
            if (CurrentPath.CurrentSelectedItemCategory == "") return RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" + RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

            return RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + "/" + Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory) + "/" + RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + "/" + CurrentPath.CurrentSelectedItemCategory;
        }

        private static string RemoveFormat(string str)
        {
            return str.Replace(' ', '_').Replace('/', '-');
        }

        // Undo Button
        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (CurrentPath.CurrentSelectedItemCategory != "")
            {
                CurrentPath.CurrentSelectedItemCategory = "";
                GenerateItemCategoryList();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedItem != null)
            {
                CurrentPath.CurrentSelectedItem = null;
                GenerateItems(CurrentPath.CurrentSelectedCategory);
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
            {
                CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                GenerateCategoryList();
                PathTextBox.Text = GeneratePath();
            }
        }
    }
}

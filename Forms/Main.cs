using System.Diagnostics;
using System.Drawing.Text;
using System.IO.Compression;
using System.Text;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        // Current Version
        private const string CurrentVersion = "v1.0.1";

        // Items Data
        public Item[] Items;

        // Common Avatars
        public CommonAvatar[] CommonAvatars;

        // Current Path
        public CurrentPath CurrentPath = new();

        // Font
        private readonly PrivateFontCollection _fontCollection = new();
        private readonly Dictionary<string, FontFamily> _fontFamilies = new();
        public FontFamily? GuiFont;

        // Language
        public string CurrentLanguage = "ja-JP";

        // Search Mode
        private bool _authorMode;
        private bool _categoryMode;

        private Window _openingWindow = Window.Nothing;

        private readonly Dictionary<string, string> _controlNames = new();

        private readonly Dictionary<string, SizeF> _defaultControlSize = new();
        private readonly Dictionary<string, PointF> _defaultControlLocation = new();
        private readonly Size _initialFormSize;
        private readonly int _baseAvatarSearchFilterListWidth;
        private readonly int _baseAvatarItemExplorerListWidth;
        private Size _previousFormSize;

        private int GetAvatarListWidth() => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;
        private int GetItemExplorerListWidth() => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;

        public Main()
        {
            Items = Helper.LoadItemsData();
            CommonAvatars = Helper.LoadCommonAvatarData();

            // Fix Supported Avatar Path (Title => Path)
            Items = Helper.FixSupportedAvatarPath(Items);

            AddFontFile();
            InitializeComponent();

            // Save the default Size
            _initialFormSize = ClientSize;
            _previousFormSize = Size;
            _baseAvatarSearchFilterListWidth = AvatarSearchFilterList.Width;
            _baseAvatarItemExplorerListWidth = AvatarItemExplorer.Width;

            LanguageBox.SelectedIndex = 0;
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();

            Text = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";
        }

        private void AddFontFile()
        {
            _fontCollection.AddFontFile("./Datas/Fonts/NotoSansJP-Regular.ttf");
            _fontCollection.AddFontFile("./Datas/Fonts/NotoSans-Regular.ttf");
            _fontCollection.AddFontFile("./Datas/Fonts/NotoSansKR-Regular.ttf");

            foreach (var fontFamily in _fontCollection.Families)
            {
                switch (fontFamily.Name)
                {
                    case "Noto Sans JP":
                        _fontFamilies.Add("ja-JP", fontFamily);
                        break;
                    case "Noto Sans":
                        _fontFamilies.Add("en-US", fontFamily);
                        break;
                    case "Noto Sans KR":
                        _fontFamilies.Add("ko-KR", fontFamily);
                        break;
                }
            }

            GuiFont = _fontFamilies[CurrentLanguage];
        }

        // Generate List (LEFT)
        private void GenerateAvatarList()
        {
            AvatarPage.Controls.Clear();

            var items = Items.Where(item => item.Type == ItemType.Avatar).ToArray();
            if (items.Length == 0) return;
            items = items.OrderBy(item => item.Title).ToArray();

            var index = 0;
            foreach (Item item in items)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, true,
                    item.Title, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    CurrentPath.CurrentSelectedAuthor = null;
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = false;
                    _categoryMode = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start($"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) => { SearchBox.Text = $"Author=\"{item.AuthorName}\""; };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    if (_openingWindow == Window.ItemList) GenerateItems();
                    GenerateAvatarList();
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();
                    if (_openingWindow == Window.ItemList) GenerateItems();
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false;
                    if (CurrentPath.CurrentSelectedItem?.Title == item.Title)
                    {
                        CurrentPath.CurrentSelectedItemCategory = null;
                        CurrentPath.CurrentSelectedItem = null;
                        undo = true;
                        PathTextBox.Text = GeneratePath();
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (_openingWindow == Window.ItemList || undo) GenerateItems();
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;

                AvatarPage.Controls.Add(button);
                index++;
            }
        }

        private void GenerateAuthorList()
        {
            AvatarAuthorPage.Controls.Clear();
            var index = 0;

            var authors = Array.Empty<Author>();
            foreach (Item item in Items)
            {
                if (authors.Any(author => author.AuthorName == item.AuthorName)) continue;
                authors = authors.Append(new Author
                {
                    AuthorName = item.AuthorName,
                    AuthorImagePath = item.AuthorImageFilePath
                }).ToArray();
            }

            if (authors.Length == 0) return;
            authors = authors.OrderBy(author => author.AuthorName).ToArray();

            foreach (var author in authors)
            {
                Button button = Helper.CreateButton(author.AuthorImagePath, author.AuthorName,
                    Items.Count(item => item.AuthorName == author.AuthorName) +
                    Helper.Translate("個の項目", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedAuthor = author;
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedAvatarPath = null;
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = true;
                    _categoryMode = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    foreach (var item in Items.Where(item => item.AuthorName == author.AuthorName))
                    {
                        item.AuthorImageFilePath = ofd.FileName;
                    }

                    GenerateAuthorList();
                };

                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarAuthorPage.Controls.Add(button);
                index++;
            }
        }

        private void GenerateCategoryListLeft()
        {
            CategoryPage.Controls.Clear();
            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;

                var items = Items.Where(item => item.Type == itemType);
                var itemCount = items.Count();
                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedAuthor = null;
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedAvatarPath = null;
                    CurrentPath.CurrentSelectedCategory = itemType;
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    _authorMode = false;
                    _categoryMode = true;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };
                CategoryPage.Controls.Add(button);
                index++;
            }
        }

        // Generate List (RIGHT)
        private void GenerateCategoryList()
        {
            _openingWindow = Window.ItemCategoryList;
            ResetAvatarList();

            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;

                int itemCount = 0;
                if (_authorMode)
                {
                    itemCount = Items.Count(item =>
                        item.Type == itemType &&
                        item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                    );
                }
                else
                {
                    itemCount = Items.Count(item =>
                        item.Type == itemType &&
                        (
                            Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                                .IsSupportedOrCommon ||
                            item.SupportedAvatar.Length == 0
                        )
                    );
                }

                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItems()
        {
            _openingWindow = Window.ItemList;
            ResetAvatarList();

            var filteredItems = Items.AsEnumerable();

            if (_authorMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                );
            }
            else if (_categoryMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory
                );
            }
            else
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    (
                        Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                            .IsSupportedOrCommon ||
                        item.SupportedAvatar.Length == 0
                    )
                );
            }

            filteredItems = filteredItems.OrderBy(item => item.Title).ToList();
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                var authorText = Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName;

                var isSupportedOrCommon =
                    Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

                if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Length != 0 &&
                    !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
                {
                    var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                    if (commonAvatarName != "")
                    {
                        authorText += "\n" + Helper.Translate("共通素体: ", CurrentLanguage) + commonAvatarName;
                    }
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title, authorText, false, item.Title, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();
                        GenerateItems();
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                    }

                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Process.Start("explorer.exe", item.ItemPath);
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) => { SearchBox.Text = $"Author=\"{item.AuthorName}\""; };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    GenerateItems();
                    GenerateAvatarList();
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();
                    GenerateItems();
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GenerateItems();
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemCategoryList()
        {
            _openingWindow = Window.ItemFolderCategoryList;
            var types = new[]
            {
                "改変用データ",
                "テクスチャ",
                "ドキュメント",
                "Unityパッケージ",
                "マテリアル",
                "不明"
            };
            if (CurrentPath.CurrentSelectedItem == null) return;
            ItemFolderInfo itemFolderInfo = Helper.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath,
                CurrentPath.CurrentSelectedItem.MaterialPath);
            CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

            ResetAvatarList();

            var index = 0;
            foreach (var itemType in types)
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.Translate(itemType, CurrentLanguage), itemCount + Helper.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                button.Click += (_, _) =>
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
            _openingWindow = Window.ItemFolderItemsList;
            ResetAvatarList();

            var files = CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory);
            if (files.Length == 0) return;
            files = files.OrderBy(file => file.FileName).ToArray();

            var index = 0;
            foreach (var file in files)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("ファイル", CurrentLanguage), false,
                    Helper.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = file.FilePath,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        try
                        {
                            Process.Start("explorer.exe", "/select," + file.FilePath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("ファイルを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredItem(SearchFilter searchFilter)
        {
            ResetAvatarList();

            var filteredItems = Items.Where(item =>
            {
                if (!string.IsNullOrWhiteSpace(searchFilter.Author) && item.AuthorName != searchFilter.Author)
                    return false;
                if (!string.IsNullOrWhiteSpace(searchFilter.Title) && !item.Title.Contains(searchFilter.Title))
                    return false;
                if (!string.IsNullOrWhiteSpace(searchFilter.BoothId) && item.BoothId.ToString() != searchFilter.BoothId)
                    return false;

                return true;
            });

            filteredItems = filteredItems
                .Where(item =>
                    searchFilter.SearchWords.All(word =>
                        item.Title.ToLower().Contains(word.ToLower()) ||
                        item.AuthorName.ToLower().Contains(word.ToLower()) ||
                        item.SupportedAvatar.Any(avatar => avatar.ToLower().Contains(word.ToLower())) ||
                        item.BoothId.ToString().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(item =>
                {
                    var matchCount = 0;
                    foreach (var word in searchFilter.SearchWords)
                    {
                        if (item.Title.ToLower().Contains(word.ToLower())) matchCount++;
                        if (item.AuthorName.ToLower().Contains(word.ToLower())) matchCount++;
                    }

                    return matchCount;
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("検索結果: ", CurrentLanguage) + filteredItems.Count() +
                                     Helper.Translate("件", CurrentLanguage) + Helper.Translate(" (全", CurrentLanguage) +
                                     Items.Length + Helper.Translate("件)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, false,
                    item.Title, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();
                        GenerateFilteredItem(searchFilter);
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                    }

                    _authorMode = false;
                    GeneratePathFromItem(item);
                    SearchBox.Text = "";
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Process.Start("explorer.exe", item.ItemPath);
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage), "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{GetCurrentLanguageCode()}/items/" + item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) => { SearchBox.Text = $"Author=\"{item.AuthorName}\""; };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredFolderItems(SearchFilter searchWords)
        {
            ResetAvatarList();

            var fileDatas = _openingWindow switch
            {
                Window.ItemFolderItemsList => CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath
                    .CurrentSelectedItemCategory),
                Window.ItemFolderCategoryList => CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem(),
                _ => Array.Empty<FileData>()
            };

            var filteredItems = fileDatas
                .Where(file =>
                    searchWords.SearchWords.All(word =>
                        file.FileName.ToLower().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(file =>
                {
                    return searchWords.SearchWords.Count(word => file.FileName.ToLower().Contains(word.ToLower()));
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("フォルダー内検索結果: ", CurrentLanguage) + filteredItems.Count +
                                     Helper.Translate("件", CurrentLanguage) + Helper.Translate(" (全", CurrentLanguage) +
                                     fileDatas.Length + Helper.Translate("件)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (var file in filteredItems)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("ファイル", CurrentLanguage), false,
                    Helper.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = file.FilePath,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        try
                        {
                            Process.Start("explorer.exe", "/select," + file.FilePath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("ファイルを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        // Add Item Form
        private void AddItemButton_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory, false, null, null);
            addItem.ShowDialog();
            RefleshWindow();
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        // Generate Path
        private string GeneratePath()
        {
            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedAuthor == null)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (_categoryMode)
            {
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (CurrentPath.CurrentSelectedAvatar == null) return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
            if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar);
            if (CurrentPath.CurrentSelectedItem == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
            if (CurrentPath.CurrentSelectedItemCategory == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

            return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                   Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                   Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                   Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
        }

        private void GeneratePathFromItem(Item item)
        {
            var avatarName = Helper.GetAvatarName(Items, item.SupportedAvatar.FirstOrDefault());
            CurrentPath.CurrentSelectedAvatar = avatarName ?? "*";
            CurrentPath.CurrentSelectedAvatarPath = item.SupportedAvatar.FirstOrDefault();
            CurrentPath.CurrentSelectedCategory = item.Type;
            CurrentPath.CurrentSelectedItem = item;
        }

        // Undo Button
        private void UndoButton_Click(object sender, EventArgs e)
        {
            SearchBox.Text = "";

            if (CurrentPath.CurrentSelectedItemCategory != null)
            {
                CurrentPath.CurrentSelectedItemCategory = null;
                GenerateItemCategoryList();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedItem != null)
            {
                CurrentPath.CurrentSelectedItem = null;
                GenerateItems();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (_categoryMode) return;

            if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
            {
                CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                GenerateCategoryList();
                PathTextBox.Text = GeneratePath();
            }

            if (CurrentPath.CurrentSelectedAvatar == "*")
            {
                CurrentPath.CurrentSelectedAvatar = null;
                CurrentPath.CurrentSelectedAvatarPath = null;
                ResetAvatarList(true);
                PathTextBox.Text = GeneratePath();
            }
        }

        // Save Config
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helper.SaveItemsData(Items);
            Helper.SaveCommonAvatarData(CommonAvatars);
        }

        // Search Box
        private void SearchBox_TextChanged(object sender, EventArgs e) => SearchItems();

        private void SearchItems()
        {
            if (SearchBox.Text == "")
            {
                SearchResultLabel.Text = "";
                if (CurrentPath.CurrentSelectedItemCategory != null)
                {
                    GenerateItemFiles();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    GenerateItemCategoryList();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    GenerateItems();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != null || CurrentPath.CurrentSelectedAuthor != null)
                {
                    GenerateCategoryList();
                    return;
                }

                ResetAvatarList(true);
                return;
            }

            SearchFilter searchFilter = Helper.GetSearchFilter(SearchBox.Text);

            if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
            {
                GenerateFilteredFolderItems(searchFilter);
            }
            else
            {
                GenerateFilteredItem(searchFilter);
            }

            string[] pathTextArr = Array.Empty<string>();
            if (searchFilter.Author != "")
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("作者", CurrentLanguage) + ": " + searchFilter.Author)
                    .ToArray();
            }

            if (searchFilter.Title != "")
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("タイトル", CurrentLanguage) + ": " + searchFilter.Title)
                    .ToArray();
            }

            if (searchFilter.BoothId != "")
            {
                pathTextArr = pathTextArr.Append("BoothID: " + searchFilter.BoothId).ToArray();
            }

            pathTextArr = pathTextArr.Append(string.Join(",", searchFilter.SearchWords)).ToArray();

            PathTextBox.Text = Helper.Translate("検索中 - ", CurrentLanguage) + string.Join(" / ", pathTextArr);
        }

        // ResetAvatarList
        private void ResetAvatarList(bool startLabelVisible = false)
        {
            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    var control = AvatarItemExplorer.Controls[i];
                    AvatarItemExplorer.Controls.RemoveAt(i);
                    control.Dispose();
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = startLabelVisible;
                }
            }
        }

        // Drag and Drop Item Folder
        private void AvatarItemExplorer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (File.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, false, null, folderPath);
            addItem.ShowDialog();
            RefleshWindow();
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        private void AvatarPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (File.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, ItemType.Avatar, false, null, folderPath);
            addItem.ShowDialog();
            RefleshWindow();
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        // Export to CSV
        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                ExportButton.Enabled = false;
                if (!Directory.Exists("./Output"))
                {
                    Directory.CreateDirectory("./Output");
                }

                var fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";

                if (File.Exists("./Output/" + fileName))
                {
                    MessageBox.Show(Helper.Translate("ファイル名が重複しています。すこし時間を開けてから再度実行してください。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ExportButton.Enabled = true;
                    return;
                }

                using var sw = new StreamWriter("./Output/" + fileName, false, Encoding.UTF8);
                sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,SupportedAvatar,BoothId,ItemPath");
                foreach (var item in Items)
                {
                    string[] avatarNames = Array.Empty<string>();
                    foreach (var avatar in item.SupportedAvatar)
                    {
                        var avatarName = Helper.GetAvatarName(Items, avatar);
                        if (avatarName == null) continue;
                        avatarNames = avatarNames.Append(avatarName).ToArray();
                    }

                    sw.WriteLine(
                        $"{item.Title},{item.AuthorName},{item.AuthorImageFilePath},{item.ImagePath},{item.Type},{string.Join(";", avatarNames)},{item.BoothId},{item.ItemPath}");
                }

                MessageBox.Show(Helper.Translate("Outputフォルダにエクスポートが完了しました！\nファイル名: ", CurrentLanguage) + fileName,
                    Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ExportButton.Enabled = true;
            }
            catch
            {
                MessageBox.Show(Helper.Translate("エクスポートに失敗しました", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExportButton.Enabled = true;
            }
        }

        // Make Backup
        private void MakeBackupButton_Click(object sender, EventArgs e)
        {
            try
            {
                MakeBackupButton.Enabled = false;
                if (!Directory.Exists("./Backup"))
                {
                    Directory.CreateDirectory("./Backup");
                }

                var fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".zip";

                if (File.Exists("./Backup/" + fileName))
                {
                    MessageBox.Show(Helper.Translate("ファイル名が重複しています。すこし時間を開けてから再度実行してください。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MakeBackupButton.Enabled = true;
                    return;
                }

                ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

                MessageBox.Show(
                    Helper.Translate(
                        "Backupフォルダにバックアップが完了しました！\n\n復元したい場合はBackupフォルダの中身を解凍し、全てをDatasフォルダの中身と置き換えれば大丈夫です！\n※ソフトはその間起動しないようにしてください！\n\nファイル名: ",
                        CurrentLanguage) + fileName, Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MakeBackupButton.Enabled = true;
            }
            catch
            {
                MessageBox.Show(Helper.Translate("バックアップに失敗しました", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                MakeBackupButton.Enabled = true;
            }
        }

        private void LanguageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentLanguage = LanguageBox.SelectedIndex switch
            {
                0 => "ja-JP",
                1 => "ko-KR",
                2 => "en-US",
                _ => CurrentLanguage
            };

            GuiFont = _fontFamilies[CurrentLanguage];

            foreach (Control control in Controls)
            {
                if (control.Name == "LanguageBox") continue;
                if (control.Text == "") continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in AvatarSearchFilterList.Controls)
            {
                if (control.Text == "") continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in ExplorerList.Controls)
            {
                if (control.Text == "") continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in AvatarItemExplorer.Controls)
            {
                if (control.Text == "") continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
            PathTextBox.Text = GeneratePath();
            if (SearchBox.Text != "")
            {
                SearchItems();
            }
            else
            {
                RefleshWindow();
            }
        }

        private void RefleshWindow()
        {
            switch (_openingWindow)
            {
                case Window.ItemList:
                    GenerateItems();
                    break;
                case Window.ItemCategoryList:
                    GenerateCategoryList();
                    break;
                case Window.ItemFolderCategoryList:
                    GenerateItemCategoryList();
                    break;
                case Window.ItemFolderItemsList:
                    GenerateItemFiles();
                    break;
                case Window.Nothing:
                    break;
            }
        }

        private string GetCurrentLanguageCode()
        {
            return CurrentLanguage switch
            {
                "ja-JP" => "ja",
                "ko-KR" => "ko",
                "en-US" => "en",
                _ => "ja"
            };
        }

        private void ManageCommonAvatarButton_Click(object sender, EventArgs e)
        {
            ManageCommonAvatars manageCommonAvatar = new(this);
            manageCommonAvatar.ShowDialog();
            RefleshWindow();
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
            PathTextBox.Text = GeneratePath();
        }

        // 前のバージョン、もしくはバックアップからデータを読み込む
        private void LoadDataFromFolder()
        {
            FolderBrowserDialog fbd = new()
            {
                UseDescriptionForTitle = true,
                Description = Helper.Translate("以前のバージョンのDatasフォルダ、もしくは展開したバックアップフォルダを選択してください", CurrentLanguage),
                ShowNewFolderButton = false
            };
            if (fbd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var filePath = fbd.SelectedPath + "/ItemsData.json";
                if (!File.Exists(filePath))
                {
                    MessageBox.Show(Helper.Translate("アイテムファイルが見つかりませんでした。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Items = Helper.LoadItemsData(filePath);
                    Items = Helper.FixSupportedAvatarPath(Items);
                }

                var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                if (!File.Exists(filePath2))
                {
                    MessageBox.Show(Helper.Translate("共通素体ファイルが見つかりませんでした。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                }

                var result2 = MessageBox.Show(
                    Helper.Translate("Thumbnailフォルダ、AuthorImageフォルダもコピーしますか？", CurrentLanguage),
                    Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result2 != DialogResult.Yes)
                {
                    RefleshWindow();
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    return;
                }

                var thumbnailPath = fbd.SelectedPath + "/Thumbnail";
                var authorImagePath = fbd.SelectedPath + "/AuthorImage";

                if (Directory.Exists(thumbnailPath))
                {
                    Directory.CreateDirectory("./Datas/Thumbnail");
                    foreach (var file in Directory.GetFiles(thumbnailPath))
                    {
                        try
                        {
                            File.Copy(file, "./Datas/Thumbnail/" + Path.GetFileName(file), true);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to copy thumbnail image." + file);
                        }
                    }
                }

                if (Directory.Exists(authorImagePath))
                {
                    Directory.CreateDirectory("./Datas/AuthorImage");
                    foreach (var file in Directory.GetFiles(authorImagePath))
                    {
                        try
                        {
                            File.Copy(file, "./Datas/AuthorImage/" + Path.GetFileName(file), true);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to copy author image." + file);
                        }
                    }
                }

                MessageBox.Show(Helper.Translate("コピーが完了しました。", CurrentLanguage),
                    Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(Helper.Translate("データの読み込みに失敗しました。", CurrentLanguage) + "\n\n" + e.Message,
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefleshWindow();
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        private void LoadData_Click(object sender, EventArgs e) => LoadDataFromFolder();

        // フォームのリサイズ
        private void Main_Resize(object sender, EventArgs e) => ResizeControl();

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (Size == _previousFormSize) return;
            _previousFormSize = Size;
            ReRenderWindow();
        }

        private void ResizeControl()
        {
            var widthRatio = (float)ClientSize.Width / _initialFormSize.Width;
            var heightRatio = (float)ClientSize.Height / _initialFormSize.Height;

            foreach (Control control in Controls)
            {
                // サイズのスケーリング
                if (_defaultControlSize.TryGetValue(control.Name, out var defaultSize))
                {
                    var newWidth = (int)(defaultSize.Width * widthRatio);
                    var newHeight = (int)(defaultSize.Height * heightRatio);

                    // サイズがクライアント領域を超えないように制約
                    newWidth = Math.Min(newWidth, ClientSize.Width);
                    newHeight = Math.Min(newHeight, ClientSize.Height);

                    control.Size = new Size(newWidth, newHeight);
                }
                else
                {
                    _defaultControlSize.Add(control.Name, new SizeF(control.Size.Width, control.Size.Height));
                }

                // 位置のスケーリング
                if (_defaultControlLocation.TryGetValue(control.Name, out var defaultLocation))
                {
                    var newX = (int)(defaultLocation.X * widthRatio);
                    var newY = (int)(defaultLocation.Y * heightRatio);

                    // 位置がクライアント領域を超えないように制約
                    newX = Math.Max(0, Math.Min(newX, ClientSize.Width - control.Width));
                    newY = Math.Max(0, Math.Min(newY, ClientSize.Height - control.Height));

                    control.Location = new Point(newX, newY);
                }
                else
                {
                    _defaultControlLocation.Add(control.Name, new PointF(control.Location.X, control.Location.Y));
                }
            }

            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel") continue;
                var control = AvatarItemExplorer.Controls[i];
                control.Location = control.Location with
                {
                    X = (AvatarItemExplorer.Width - control.Width) / 2,
                    Y = (AvatarItemExplorer.Height - control.Height) / 2
                };
            }
        }

        private void ReRenderWindow()
        {
            if (SearchBox.Text != "")
            {
                SearchItems();
            }
            else
            {
                RefleshWindow();
            }

            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }
    }
}
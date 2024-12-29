using System.Diagnostics;
using System.Drawing.Text;
using System.Formats.Tar;
using System.IO.Compression;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using Avatar_Explorer.Classes;
using Timer = System.Windows.Forms.Timer;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        // Current Version
        private const string CurrentVersion = "v1.0.1";

        // Current Version Form Text
        private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by �Ղ����";

        // Min Resize Font Size
        private const float MinFontSize = 8f;

        // Backup Interval
        private const int BackupInterval = 300000; // 5 Minutes

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

        // Dictionary For Resize Controls
        private readonly Dictionary<string, string> _controlNames = new();
        private readonly Dictionary<string, SizeF> _defaultControlSize = new();
        private readonly Dictionary<string, PointF> _defaultControlLocation = new();
        private readonly Dictionary<string, float> _defaultFontSize = new();

        // Initial Form, AvatarSearchFilterList, AvatarItemExplorer Size
        private readonly Size _initialFormSize;
        private readonly int _baseAvatarSearchFilterListWidth;
        private readonly int _baseAvatarItemExplorerListWidth;

        // For Resize Button
        private int GetAvatarListWidth() => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;
        private int GetItemExplorerListWidth() => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;

        // Last Backup Time
        private DateTime _lastBackupTime;

        // Last Backup Error
        private bool _lastBackupError;

        // Searching Bool
        private bool _isSearching;

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
            _baseAvatarSearchFilterListWidth = AvatarSearchFilterList.Width;
            _baseAvatarItemExplorerListWidth = AvatarItemExplorer.Width;
            LanguageBox.SelectedIndex = 0;

            // Render Window
            RefleshWindow();

            // Start AutoBackup
            AutoBackup();

            // Set Backup Title Loop
            BackupTimeTitle();

            Text = $"VRChat Avatar Explorer {CurrentVersion} by �Ղ����";
        }

        private void AddFontFile()
        {
            string[] fontFiles = Directory.GetFiles("./Datas/Fonts", "*.ttf");
            foreach (var fontFile in fontFiles)
            {
                _fontCollection.AddFontFile(fontFile);
            }

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
                    Helper.Translate("���: ", CurrentLanguage) + item.AuthorName, true,
                    item.Title, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedAvatar = item.Title,
                        CurrentSelectedAvatarPath = item.ItemPath
                    };
                    _authorMode = false;
                    _categoryMode = false;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("�N���b�v�{�[�h�ɃR�s�[�ł��܂���ł���", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start($"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�����N���J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;

                    // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃T���l�C�����X�V���Ȃ��Ƃ����Ȃ����߁B
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //���������ƁA������ʂ��ēǍ����Ă�����
                    if (_isSearching) SearchItems();

                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();

                    // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃A�C�e���̏����X�V���Ȃ��Ƃ����Ȃ�����
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //���������ƁA������ʂ��ēǍ����Ă�����
                    if (_isSearching) SearchItems();

                    // �����A�C�e���ŕҏW���ꂽ�A�C�e�����J���Ă�����A�p�X�ȂǂɎg�p����镶������X�V���Ȃ��Ƃ����Ȃ�����
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath) CurrentPath.CurrentSelectedAvatar = item.Title;

                    // �������̕�����������Ȃ��悤�ɂ��邽�߂�_isSearching�Ń`�F�b�N���Ă���
                    if (!_isSearching) PathTextBox.Text = GeneratePath();

                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false;

                    // �����폜�����A�C�e�����J����Ă�����A��ʂ�߂��Ȃ��Ƃ����Ȃ����߁B
                    if (CurrentPath.CurrentSelectedItem?.ItemPath == item.ItemPath)
                    {
                        CurrentPath.CurrentSelectedItemCategory = null;
                        CurrentPath.CurrentSelectedItem = null;
                        undo = true;
                    }

                    var undo2 = false;

                    // �A�o�^�[���[�h�ł����폜�����A�o�^�[���烁�j���[���J����Ă�����A��ʂ�߂��Ȃ��Ƃ����Ȃ����߁B
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && !_authorMode && !_categoryMode)
                    {
                        CurrentPath = new CurrentPath();
                        undo2 = true;
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    // �A�o�^�[�̂Ƃ��͑Ή��A�o�^�[�폜�A���ʑf�̃O���[�v����폜�p�̏��������s����
                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath).ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath).ToArray();
                                }

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }

                    }

                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // �폜�����A�C�e�����J����Ă��������Ȃ�A�A�C�e���I����ʂ܂Ŗ߂��B
                    if (_openingWindow == Window.ItemList || undo)
                    {
                        if (_isSearching)
                        {
                            SearchItems();
                        }
                        else
                        {
                            GenerateItems();
                        }
                    }

                    // �A�o�^�[����폜�����A�C�e�����J����Ă�����A������ʂ܂Ŗ߂��B
                    if (undo2) ResetAvatarList(true);

                    // �������̕�����������Ȃ��悤�ɂ��邽�߂�_isSearching�Ń`�F�b�N���Ă���
                    if (!_isSearching) PathTextBox.Text = GeneratePath();

                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
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
                    Helper.Translate("�̍���", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedAuthor = author
                    };

                    _authorMode = true;
                    _categoryMode = false;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    foreach (var item in Items.Where(item => item.AuthorName == author.AuthorName))
                    {
                        item.AuthorImageFilePath = ofd.FileName;
                    }

                    GenerateAuthorList();
                    Helper.SaveItemsData(Items);
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
                    itemCount + Helper.Translate("�̍���", CurrentLanguage), true, "", GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedCategory = itemType
                    };

                    _authorMode = false;
                    _categoryMode = true;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
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
                            item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                        )
                    );
                }

                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth());
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
                        item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                    )
                );
            }

            filteredItems = filteredItems.OrderBy(item => item.Title).ToList();
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                var authorText = Helper.Translate("���: ", CurrentLanguage) + item.AuthorName;

                var isSupportedOrCommon =
                    Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

                if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Length != 0 &&
                    !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
                {
                    var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                    if (!string.IsNullOrEmpty(commonAvatarName))
                    {
                        authorText += "\n" + Helper.Translate("���ʑf��: ", CurrentLanguage) + commonAvatarName;
                    }
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title, authorText, false, item.Title, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();
                        RefleshWindow();
                        Helper.SaveItemsData(Items);
                    }

                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�H���_���J��", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            Process.Start("explorer.exe", item.ItemPath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_���J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("�N���b�v�{�[�h�ɃR�s�[�ł��܂���ł���", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�����N���J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;

                    if (_isSearching)
                    {
                        SearchItems();
                    }
                    else
                    {
                        GenerateItems();
                    }
                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath) CurrentPath.CurrentSelectedAvatar = item.Title;
                    if (!_isSearching) PathTextBox.Text = GeneratePath();
                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false;
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && !_authorMode && !_categoryMode)
                    {
                        CurrentPath = new CurrentPath();
                        undo = true;
                        PathTextBox.Text = GeneratePath();
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath).ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath).ToArray();
                                }
                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (undo) ResetAvatarList(true);
                    RefleshWindow();
                    Helper.SaveItemsData(Items);
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
                "���ϗp�f�[�^",
                "�e�N�X�`��",
                "�h�L�������g",
                "Unity�p�b�P�[�W",
                "�}�e���A��",
                "�s��"
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
                    Helper.Translate(itemType, CurrentLanguage), itemCount + Helper.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth());
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
                    file.FileExtension.Replace(".", "") + Helper.Translate("�t�@�C��", CurrentLanguage), false,
                    Helper.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�@�C���̃p�X���J��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {

                        if (file.FileExtension is ".unitypackage")
                        {
                            _ = ChangeUnityPackageFilePathAsync(file);
                        }
                        else
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = file.FilePath,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch
                    {
                        try
                        {
                            Process.Start("explorer.exe", "/select," + file.FilePath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�t�@�C�����J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
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
                if (searchFilter.Author.Length != 0 && !searchFilter.Author.Contains(item.AuthorName))
                    return false;
                if (searchFilter.Title.Length != 0 && !searchFilter.Title.Contains(item.Title))
                    return false;
                if (searchFilter.BoothId.Length != 0 && !searchFilter.BoothId.Contains(item.BoothId.ToString()))
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

            SearchResultLabel.Text = Helper.Translate("��������: ", CurrentLanguage) + filteredItems.Count() +
                                     Helper.Translate("��", CurrentLanguage) + Helper.Translate(" (�S", CurrentLanguage) +
                                     Items.Length + Helper.Translate("��)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("���: ", CurrentLanguage) + item.AuthorName, false,
                    item.Title, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();
                        GenerateFilteredItem(searchFilter);
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        Helper.SaveItemsData(Items);
                    }

                    _authorMode = false;
                    _categoryMode = false;
                    GeneratePathFromItem(item);
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�H���_���J��", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            Process.Start("explorer.exe", item.ItemPath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_���J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText($"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("�N���b�v�{�[�h�ɃR�s�[�ł��܂���ł���", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("�����N���J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath).ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath).ToArray();
                                }
                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
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

            SearchResultLabel.Text = Helper.Translate("�t�H���_�[����������: ", CurrentLanguage) + filteredItems.Count +
                                     Helper.Translate("��", CurrentLanguage) + Helper.Translate(" (�S", CurrentLanguage) +
                                     fileDatas.Length + Helper.Translate("��)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (var file in filteredItems)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("�t�@�C��", CurrentLanguage), false,
                    Helper.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�@�C���̃p�X���J��", CurrentLanguage),
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
                            MessageBox.Show(Helper.Translate("�t�@�C�����J���܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK,
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
            Helper.SaveItemsData(Items);
        }

        // Generate Path
        private string GeneratePath()
        {
            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedAuthor == null)
                    return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
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
                    return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (CurrentPath.CurrentSelectedAvatar == null) return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
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
            var avatarPath = item.SupportedAvatar.FirstOrDefault();
            var avatarName = Helper.GetAvatarName(Items, avatarPath);
            CurrentPath.CurrentSelectedAvatar = avatarName ?? "*";
            CurrentPath.CurrentSelectedAvatarPath = avatarPath;
            CurrentPath.CurrentSelectedCategory = item.Type;
            CurrentPath.CurrentSelectedItem = item;
        }

        // Undo Button
        private void UndoButton_Click(object sender, EventArgs e)
        {
            //�������������ꍇ�͑O�̉�ʂ܂łƂ肠�����߂��Ă�����
            if (_isSearching)
            {
                SearchBox.Text = "";
                SearchResultLabel.Text = "";
                _isSearching = false;
                if (CurrentPath.CurrentSelectedItemCategory != null)
                {
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != null || CurrentPath.CurrentSelectedAuthor != null)
                {
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                ResetAvatarList(true);
                PathTextBox.Text = GeneratePath();
                return;
            }

            SearchBox.Text = "";
            SearchResultLabel.Text = "";
            _isSearching = false;

            if (CurrentPath.IsEmpty())
            {
                //�G���[�����Đ�
                SystemSounds.Hand.Play();
                return;
            }

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

            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }
            }
            else if (!_categoryMode)
            {
                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar == "*")
                {
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedAvatarPath = null;
                    ResetAvatarList(true);
                    PathTextBox.Text = GeneratePath();
                    return;
                }
            }

            ResetAvatarList(true);
            PathTextBox.Text = GeneratePath();
        }

        // Search Box
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                SearchItems();
            }
        }

        private void SearchItems()
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
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

            _isSearching = true;
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
            if (searchFilter.Author.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("���", CurrentLanguage) + ": " + string.Join(", ", searchFilter.Author))
                    .ToArray();
            }

            if (searchFilter.Title.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�^�C�g��", CurrentLanguage) + ": " + string.Join(", ", searchFilter.Title))
                    .ToArray();
            }

            if (searchFilter.BoothId.Length != 0)
            {
                pathTextArr = pathTextArr.Append("BoothID: " + string.Join(", ", searchFilter.BoothId)).ToArray();
            }

            pathTextArr = pathTextArr.Append(string.Join(", ", searchFilter.SearchWords)).ToArray();

            PathTextBox.Text = Helper.Translate("������... - ", CurrentLanguage) + string.Join(" / ", pathTextArr);
        }

        // ResetAvatarList
        private void ResetAvatarList(bool startLabelVisible = false)
        {
            if (startLabelVisible) CurrentPath = new CurrentPath();
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

            if (File.Exists(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("�t�H���_��I�����Ă�������", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, false, null, folderPath);
            addItem.ShowDialog();
            RefleshWindow();
            Helper.SaveItemsData(Items);
        }

        private void AvatarPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (File.Exists(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("�t�H���_��I�����Ă�������", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, ItemType.Avatar, false, null, folderPath);
            addItem.ShowDialog();
            RefleshWindow();
            Helper.SaveItemsData(Items);
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
                    MessageBox.Show(Helper.Translate("�t�@�C�������d�����Ă��܂��B���������Ԃ��J���Ă���ēx���s���Ă��������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                MessageBox.Show(Helper.Translate("Output�t�H���_�ɃG�N�X�|�[�g���������܂����I\n�t�@�C����: ", CurrentLanguage) + fileName,
                    Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ExportButton.Enabled = true;
            }
            catch
            {
                MessageBox.Show(Helper.Translate("�G�N�X�|�[�g�Ɏ��s���܂���", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(Helper.Translate("�t�@�C�������d�����Ă��܂��B���������Ԃ��J���Ă���ēx���s���Ă��������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MakeBackupButton.Enabled = true;
                    return;
                }

                ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

                MessageBox.Show(
                    Helper.Translate(
                        "Backup�t�H���_�Ƀo�b�N�A�b�v���������܂����I\n\n�����������ꍇ�́A\"�f�[�^��ǂݍ���\"�{�^���Ō��ݍ쐬���ꂽ�t�@�C����W�J�������̂�I�����Ă��������B\n\n�t�@�C����: ",
                        CurrentLanguage) + fileName, Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MakeBackupButton.Enabled = true;
            }
            catch
            {
                MessageBox.Show(Helper.Translate("�o�b�N�A�b�v�Ɏ��s���܂���", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                MakeBackupButton.Enabled = true;
            }
        }

        // Change Language
        private void LanguageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentLanguage = LanguageBox.SelectedIndex switch
            {
                0 => "ja-JP",
                1 => "ko-KR",
                2 => "en-US",
                _ => CurrentLanguage
            };

            var newFont = _fontFamilies.TryGetValue(CurrentLanguage, out var family) ? family : _fontFamilies["ja-JP"];
            GuiFont = newFont;

            foreach (Control control in Controls)
            {
                if (control.Name == "LanguageBox") continue;
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in AvatarSearchFilterList.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in ExplorerList.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            foreach (Control control in AvatarItemExplorer.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
            }

            RefleshWindow();
            PathTextBox.Text = GeneratePath();
        }

        private void RefleshWindow(bool reloadLeft = true)
        {
            if (_isSearching)
            {
                SearchItems();
                GenerateAvatarList();
                GenerateAuthorList();
                GenerateCategoryListLeft();
                return;
            }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!reloadLeft) return;
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        private void ManageCommonAvatarButton_Click(object sender, EventArgs e)
        {
            ManageCommonAvatars manageCommonAvatar = new(this);
            manageCommonAvatar.ShowDialog();
            RefleshWindow();
            PathTextBox.Text = GeneratePath();
            Helper.SaveCommonAvatarData(CommonAvatars);
        }

        // Load Data Button
        private void LoadData_Click(object sender, EventArgs e) => LoadDataFromFolder();

        // Load Data From Folder
        private void LoadDataFromFolder()
        {
            //�����o�b�N�A�b�v�t�H���_���畜�����邩����
            var result = MessageBox.Show(Helper.Translate("�����o�b�N�A�b�v�t�H���_���畜�����܂����H", CurrentLanguage),
                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //�o�b�N�A�b�v��̃t�H���_
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var backupPath = Path.Combine(appDataPath, "Avatar Explorer", "Backup");

                //�o�b�N�A�b�v�t�H���_�����݂��Ȃ��ꍇ
                if (!Directory.Exists(backupPath))
                {
                    MessageBox.Show(Helper.Translate("�o�b�N�A�b�v�t�H���_��������܂���ł����B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //�ŏ��̃t�H���_
                var firstFolder = Directory.GetDirectories(backupPath).MaxBy(d => new DirectoryInfo(d).CreationTime) ?? backupPath;

                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("�o�b�N�A�b�v���鎞�Ԃ̃t�H���_��I�����Ă�������", CurrentLanguage),
                    ShowNewFolderButton = false,
                    SelectedPath = firstFolder
                };

                if (fbd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var filePath = fbd.SelectedPath + "/ItemsData.json";
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(Helper.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Items = Helper.FixSupportedAvatarPath(Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    MessageBox.Show(Helper.Translate("�������������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
                    MessageBox.Show(Helper.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("�ȑO�̃o�[�W������Datas�t�H���_�A�������͓W�J�����o�b�N�A�b�v�t�H���_��I�����Ă�������", CurrentLanguage),
                    ShowNewFolderButton = false
                };
                if (fbd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var filePath = fbd.SelectedPath + "/ItemsData.json";
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(Helper.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Items = Helper.FixSupportedAvatarPath(Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    var result2 = MessageBox.Show(
                        Helper.Translate("Thumbnail�t�H���_�AAuthorImage�t�H���_���R�s�[���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result2 != DialogResult.Yes)
                    {
                        SearchBox.Text = "";
                        SearchResultLabel.Text = "";
                        _isSearching = false;
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        ResetAvatarList(true);
                        PathTextBox.Text = GeneratePath();
                        MessageBox.Show(Helper.Translate("�R�s�[���������܂����B", CurrentLanguage),
                            Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            catch (Exception ex)
                            {
                                Helper.ErrorLogger("�T���l�C���̃R�s�[�Ɏ��s���܂����B", ex);
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
                            catch (Exception ex)
                            {
                                Helper.ErrorLogger("��҉摜�̃R�s�[�Ɏ��s���܂����B", ex);
                            }
                        }
                    }

                    MessageBox.Show(Helper.Translate("�R�s�[���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
                    MessageBox.Show(Helper.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            SearchBox.Text = "";
            SearchResultLabel.Text = "";
            _isSearching = false;
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
            ResetAvatarList(true);
            PathTextBox.Text = GeneratePath();
        }

        // Resize Form
        private void Main_Resize(object sender, EventArgs e) => ResizeControl();

        private void ResizeControl()
        {
            var widthRatio = (float)ClientSize.Width / _initialFormSize.Width;
            var heightRatio = (float)ClientSize.Height / _initialFormSize.Height;

            foreach (Control control in Controls)
            {
                // �T�C�Y�̃X�P�[�����O
                if (!_defaultControlSize.TryGetValue(control.Name, out var defaultSize))
                {
                    defaultSize = new SizeF(control.Size.Width, control.Size.Height);
                    _defaultControlSize.Add(control.Name, defaultSize);
                }

                var newWidth = (int)(defaultSize.Width * widthRatio);
                var newHeight = (int)(defaultSize.Height * heightRatio);

                // �T�C�Y���N���C�A���g�̈�𒴂��Ȃ��悤�ɐ���
                newWidth = Math.Min(newWidth, ClientSize.Width);
                newHeight = Math.Min(newHeight, ClientSize.Height);

                control.Size = new Size(newWidth, newHeight);

                // �ʒu�̃X�P�[�����O
                if (!_defaultControlLocation.TryGetValue(control.Name, out var defaultLocation))
                {
                    defaultLocation = new PointF(control.Location.X, control.Location.Y);
                    _defaultControlLocation.Add(control.Name, defaultLocation);
                }

                var newX = (int)(defaultLocation.X * widthRatio);
                var newY = (int)(defaultLocation.Y * heightRatio);

                // �ʒu���N���C�A���g�̈�𒴂��Ȃ��悤�ɐ���
                newX = Math.Max(0, Math.Min(newX, ClientSize.Width - control.Width));
                newY = Math.Max(0, Math.Min(newY, ClientSize.Height - control.Height));

                control.Location = new Point(newX, newY);

                switch (control)
                {
                    // ���x���A�e�L�X�g�{�b�N�X�̃t�H���g�T�C�Y�̃X�P�[�����O
                    case Label { Name: "SearchResultLabel" } label:
                        {
                            if (!_defaultFontSize.TryGetValue(label.Name, out var defaultFontSize))
                            {
                                defaultFontSize = label.Font.Size;
                                _defaultFontSize.Add(label.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;

                            // �������Ȃ�ꍇ�̂݃t�H���g�T�C�Y��ύX
                            if (!(newFontSize < defaultFontSize)) continue;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            label.Font = new Font(label.Font.FontFamily, newFontSize, label.Font.Style);
                            break;
                        }
                    // SearchResultLabel�ȊO
                    case Label label:
                        {
                            if (!_defaultFontSize.TryGetValue(label.Name, out var defaultFontSize))
                            {
                                defaultFontSize = label.Font.Size;
                                _defaultFontSize.Add(label.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            label.Font = new Font(label.Font.FontFamily, newFontSize, label.Font.Style);
                            break;
                        }
                    case TextBox:
                        {
                            if (!_defaultFontSize.TryGetValue(control.Name, out var defaultFontSize))
                            {
                                defaultFontSize = control.Font.Size;
                                _defaultFontSize.Add(control.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            control.Font = new Font(control.Font.FontFamily, newFontSize, control.Font.Style);
                            break;
                        }
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

            ScaleItemButtons();
        }

        private void ScaleItemButtons()
        {
            const int avatarItemExplorerBaseWidth = 874;
            const int avatarItemListBaseWidth = 303;

            var avatarItemExplorerWidth = avatarItemExplorerBaseWidth + GetItemExplorerListWidth();
            var avatarItemListWidth = avatarItemListBaseWidth + GetAvatarListWidth();

            foreach (Control control in AvatarItemExplorer.Controls)
            {
                if (control is Button button)
                {
                    button.Size = button.Size with { Width = avatarItemExplorerWidth };
                }
            }

            var controls = new Control[]
            {
                AvatarPage,
                AvatarAuthorPage,
                CategoryPage
            };

            foreach (var control in controls)
            {
                foreach (Control control1 in control.Controls)
                {
                    if (control1 is Button button)
                    {
                        button.Size = button.Size with { Width = avatarItemListWidth };
                    }
                }
            }
        }

        // Backup
        private void AutoBackup()
        {
            BackupFile();
            Timer timer = new()
            {
                Interval = BackupInterval
            };

            timer.Tick += (_, _) => BackupFile();
            timer.Start();
        }

        private void BackupTimeTitle()
        {
            Timer timer = new()
            {
                Interval = 1000
            };

            timer.Tick += (_, _) =>
            {
                if (_lastBackupTime == DateTime.MinValue)
                {
                    if (_lastBackupError) Text = CurrentVersionFormText + " - " + Helper.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
                    return;
                }
                var timeSpan = DateTime.Now - _lastBackupTime;
                var minutes = timeSpan.Minutes;
                Text = CurrentVersionFormText +
                       $" - {Helper.Translate("�ŏI�����o�b�N�A�b�v: ", CurrentLanguage) + minutes + Helper.Translate("���O", CurrentLanguage)}";

                if (_lastBackupError) Text += " - " + Helper.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
            };
            timer.Start();
        }

        private void BackupFile()
        {
            try
            {
                var backupFilesArray = new[]
                {
                    "./Datas/ItemsData.json",
                    "./Datas/CommonAvatar.json"
                };

                Helper.Backup(backupFilesArray);
                _lastBackupError = false;
                _lastBackupTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _lastBackupError = true;
                Helper.ErrorLogger("�����o�b�N�A�b�v�Ɏ��s���܂����B", ex);
            }
        }


        /// <summary>
        /// UnityPackage�̓W�J��f�B���N�g����ύX����
        /// </summary>
        private async Task ChangeUnityPackageFilePathAsync(FileData file)
        {
            // Temp Unity Package�̕ۑ���f�B���N�g�����쐬
            string authorName = CurrentPath.CurrentSelectedItem.AuthorName;
            string itemTitle = CurrentPath.CurrentSelectedItem.Title;

            // null�`�F�b�N
            if (String.IsNullOrEmpty(authorName)) { authorName = "Unknown"; }
            if (String.IsNullOrEmpty(itemTitle)) { itemTitle = "Unknown"; }

            authorName = CheckFilePath(authorName);
            itemTitle = CheckFilePath(itemTitle);

            string saveFolder = @$".\\Datas\\Temp\\{authorName}\\{itemTitle}\\";
            string saveFilePath = @$"{saveFolder}\\{Path.GetFileNameWithoutExtension(file.FileName)}_export.unitypackage";

            if (Directory.Exists(saveFolder) is false)
            {
                Directory.CreateDirectory(saveFolder);
            }

            // �W�J
            List<StreamReader> disposeStreamReaderList = new();
            // UnityPackage�̓ǂݍ���
            using var f = File.OpenRead(file.FilePath);
            using var fgz = new GZipStream(f, CompressionMode.Decompress);
            using var tr = new TarReader(fgz);

            // �V����UnityPacakage�̍쐬
            using var f2 = File.Create(saveFilePath);

            try
            {
                // �t�@�C���̏�������
                using (TarWriter tarWriter = new TarWriter(f2))
                {
                    TarEntry entry;

                    while ((entry = await tr.GetNextEntryAsync()) is not null)
                    {
                        // �A�Z�b�g�p�X��ҏW
                        if (Path.GetFileName(entry.Name) is "pathname")
                        {
                            using StreamReader reader = new StreamReader(entry.DataStream);

                            string assetPath = reader.ReadToEnd();

                            // �t�@�C���p�X��ҏW
                            assetPath = assetPath.Insert(7, $"{GetCategoryPath(CurrentPath.CurrentSelectedCategory)}/");

                            entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(assetPath));

                        }
                        //Debug.WriteLine(entry.Name);
                        await tarWriter.WriteEntryAsync(entry);
                    }
                }
                f2.Dispose();
                tr.Dispose();
                fgz.Dispose();
                f.Dispose();
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger("UnityPackage�̓W�J�Ɏ��s���܂����B", ex);
                MessageBox.Show(Helper.Translate("UnityPackage�̓W�J�Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Process.Start(new ProcessStartInfo
            {
                FileName = saveFilePath,
                UseShellExecute = true
            });
        }


        private string CheckFilePath(string s)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(s.Where(c => !invalidChars.Contains(c)));
        }

        private string GetCategoryPath(ItemType category)
        {
            return category switch
            {
                ItemType.Avatar => "Avatar",
                ItemType.Clothing => "Clothing",
                ItemType.Texture => "Texture",
                ItemType.Gimmick => "Gimmick",
                ItemType.Accessory => "Accessory",
                ItemType.HairStyle => "HairStyle",
                ItemType.Animation => "Animation",
                ItemType.Tool => "Tool",
                ItemType.Shader => "Shader",
                ItemType.Unknown => "Unknown",
                _ => "Unknown"
            };
        }
    }
}
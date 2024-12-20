using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class AddItem : Form
    {
        private readonly Main _mainForm;
        private readonly bool _edit;
        private static readonly HttpClient HttpClient = new();

        public Item Item = new();

        public string[] SupportedAvatar = Array.Empty<string>();

        public AddItem(Main mainForm, ItemType type, bool edit, Item? item, string? folderPath)
        {
            _edit = edit;
            _mainForm = mainForm;
            InitializeComponent();

            if (folderPath != null) FolderTextBox.Text = folderPath;

            TypeComboBox.SelectedIndex = (int)type == 9 ? 0 : (int)type;
            Text = Helper.Translate("アイテムの追加", _mainForm.CurrentLanguage);

            if (!(edit && item != null)) return;
            Item = item;
            Text = Helper.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            label3.Text = Helper.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            AddButton.Text = Helper.Translate("編集", _mainForm.CurrentLanguage);

            BoothURLTextBox.Text = $"https://booth.pm/ja/items/{item.BoothId}";
            FolderTextBox.Text = item.ItemPath;
            SupportedAvatar = item.SupportedAvatar;
            TitleTextBox.Text = item.Title;
            AuthorTextBox.Text = item.AuthorName;
            SelectAvatar.Text = Helper.Translate("選択中: ", _mainForm.CurrentLanguage) + SupportedAvatar.Length + Helper.Translate("個", _mainForm.CurrentLanguage);

            FolderTextBox.Enabled = false;
            AddButton.Enabled = true;
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
            CustomButton.Enabled = false;
        }

        private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;

            if (File.Exists(dragFilePathArr[0]))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", _mainForm.CurrentLanguage), Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FolderTextBox.Text = dragFilePathArr[0];
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            Item.Title = TitleTextBox.Text;
            Item.AuthorName = AuthorTextBox.Text;
            Item.Type = (ItemType)TypeComboBox.SelectedIndex;
            Item.ItemPath = FolderTextBox.Text;
            Item.SupportedAvatar = SupportedAvatar;

            if (Item.Title == "" || Item.AuthorName == "" || Item.ItemPath == "")
            {
                MessageBox.Show(Helper.Translate("タイトル、作者、フォルダパスのどれかが入力されていません", _mainForm.CurrentLanguage), Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddButton.Enabled = false;

            if (Item.BoothId != -1)
            {
                var thumbnailPath = Path.Combine("./Datas", "Thumbnail", $"{Item.BoothId}.png");
                if (!File.Exists(thumbnailPath))
                {
                    if (!string.IsNullOrEmpty(Item.ThumbnailUrl))
                    {
                        try
                        {
                            var thumbnailData = await HttpClient.GetByteArrayAsync(Item.ThumbnailUrl);
                            await File.WriteAllBytesAsync(thumbnailPath, thumbnailData);
                            Item.ImagePath = thumbnailPath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Helper.Translate("サムネイルのダウンロードに失敗しました: ", _mainForm.CurrentLanguage) + ex.Message,
                                Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Item.ImagePath = thumbnailPath;
                }
            }

            if (!string.IsNullOrEmpty(Item.AuthorId))
            {
                var authorImagePath = Path.Combine("./Datas", "AuthorImage", $"{Item.AuthorId}.png");
                if (!File.Exists(authorImagePath))
                {
                    if (!string.IsNullOrEmpty(Item.AuthorImageUrl))
                    {
                        try
                        {
                            var authorImageData = await HttpClient.GetByteArrayAsync(Item.AuthorImageUrl);
                            await File.WriteAllBytesAsync(authorImagePath, authorImageData);
                            Item.AuthorImageFilePath = authorImagePath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Helper.Translate("作者の画像のダウンロードに失敗しました: ", _mainForm.CurrentLanguage) + ex.Message,
                                Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    Item.AuthorImageFilePath = authorImagePath;
                }
            }

            if (_edit)
            {
                // 同じパスのものを削除してから追加
                MessageBox.Show(Helper.Translate("Boothのアイテムを編集しました!", _mainForm.CurrentLanguage) + "\n" + Helper.Translate("アイテム名: ", _mainForm.CurrentLanguage) + Item.Title + "\n" + Helper.Translate("作者: ", _mainForm.CurrentLanguage) + Item.AuthorName, Helper.Translate("編集完了", _mainForm.CurrentLanguage),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Where(i => i.ItemPath != Item.ItemPath).ToArray();
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }
            else
            {
                MessageBox.Show(Helper.Translate("Boothのアイテムを追加しました!", _mainForm.CurrentLanguage) + "\n" + Helper.Translate("アイテム名: ", _mainForm.CurrentLanguage) + Item.Title + "\n" + Helper.Translate("作者: ", _mainForm.CurrentLanguage) + Item.AuthorName, Helper.Translate("追加完了", _mainForm.CurrentLanguage),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }

            Close();
        }

        private async void GetButton_Click(object sender, EventArgs e)
        {
            var boothId = BoothURLTextBox.Text.Split('/').Last();
            if (!int.TryParse(boothId, out _))
            {
                MessageBox.Show(Helper.Translate("Booth URLが正しくありません", _mainForm.CurrentLanguage), Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                GetButton.Enabled = false;
                GetButton.Text = Helper.Translate("取得中...", _mainForm.CurrentLanguage);
                Item = await Helper.GetBoothItemInfoAsync(boothId);
                GetButton.Text = Helper.Translate("情報を取得", _mainForm.CurrentLanguage);
                GetButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("Boothのアイテム情報を取得できませんでした", _mainForm.CurrentLanguage) + "\n" + ex.Message, Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                TitleTextBox.Enabled = true;
                AuthorTextBox.Enabled = true;
                GetButton.Enabled = true;
                Item = new Item();
            }

            Item.BoothId = int.Parse(boothId);

            AddButton.Enabled = true;
            TitleTextBox.Text = Item.Title;
            AuthorTextBox.Text = Item.AuthorName;

            var suggestedType = Helper.GetItemType(Item.Title);
            if (suggestedType != ItemType.Unknown) TypeComboBox.SelectedIndex = (int)suggestedType;
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
        }

        private void SelectAvatar_Click(object sender, EventArgs e)
        {
            SelectSupportedAvatar selectSupportedAvatar = new(_mainForm, this);
            selectSupportedAvatar.ShowDialog();
            SelectAvatar.Text = Helper.Translate("選択中: ", _mainForm.CurrentLanguage) + SupportedAvatar.Length + Helper.Translate("個", _mainForm.CurrentLanguage);
        }

        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectAvatar.Enabled = TypeComboBox.SelectedIndex != (int)ItemType.Avatar;
        }

        private void TitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!_edit && _mainForm.Items.Any(i => i.Title == TitleTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: 同じタイトルのアイテムが既に存在します", _mainForm.CurrentLanguage));
            }
            else if (string.IsNullOrEmpty(TitleTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: タイトルが入力されていません", _mainForm.CurrentLanguage));
            }
            else if (TitleTextBox.Text == "*")
            {
                SetErrorState(Helper.Translate("エラー: タイトルを*にすることはできません", _mainForm.CurrentLanguage));
            }
            else
            {
                ClearErrorState();
            }
        }

        private void CustomButton_Click(object sender, EventArgs e)
        {
            TitleTextBox.Text = "";
            AuthorTextBox.Text = "";
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
        }

        private void FolderTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(FolderTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスが存在しません", _mainForm.CurrentLanguage));
            }
            else if (File.Exists(FolderTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスがファイルです", _mainForm.CurrentLanguage));
            }
            else if (string.IsNullOrEmpty(FolderTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスが入力されていません", _mainForm.CurrentLanguage));
            }
            else if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && !_edit)
            {
                SetErrorState(Helper.Translate("エラー: 同じパスのアイテムが既に存在します", _mainForm.CurrentLanguage));
            }
            else
            {
                ClearErrorState();
            }
        }

        private void SetErrorState(string errorMessage)
        {
            AddButton.Enabled = false;
            ErrorLabel.Text = errorMessage;
        }

        private void ClearErrorState()
        {
            AddButton.Enabled = true;
            ErrorLabel.Text = "";
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}

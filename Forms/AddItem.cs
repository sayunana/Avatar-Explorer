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
            // アイテム編集モードかどうか
            _edit = edit;

            // メインフォームの参照を持っておく
            _mainForm = mainForm;
            InitializeComponent();

            // フォルダパスが渡されたら表示してあげる
            if (folderPath != null) FolderTextBox.Text = folderPath;

            // typeでデフォルトの値を変えてあげる
            TypeComboBox.SelectedIndex = (int)type == 9 ? 0 : (int)type;
            Text = "アイテムの追加";

            // 編集用
            if (!(edit && item != null)) return;
            Item = item;
            Text = "アイテムの編集";
            label3.Text = "アイテムの編集";
            AddButton.Text = "編集";

            // 元のデータを表示
            BoothURLTextBox.Text = $"https://booth.pm/ja/items/{item.BoothId}";
            FolderTextBox.Text = item.ItemPath;
            SupportedAvatar = item.SupportedAvatar;
            TitleTextBox.Text = item.Title;
            AuthorTextBox.Text = item.AuthorName;
            SelectAvatar.Text = $"選択中: {SupportedAvatar.Length}個";

            // ボタンの有効化、無効化
            FolderTextBox.Enabled = false;
            AddButton.Enabled = true;
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
            CustomButton.Enabled = false;
        }

        private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        {
            // フォルダが投げられたときにフォルダパスを取得して、表示してあげる
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            FolderTextBox.Text = dragFilePathArr[0];
        }

        private void FolderTextBox_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private async void AddButton_Click(object sender, EventArgs e)
        {
            Item.Title = TitleTextBox.Text;
            Item.AuthorName = AuthorTextBox.Text;
            Item.Type = (ItemType)TypeComboBox.SelectedIndex;
            Item.ItemPath = FolderTextBox.Text;
            Item.SupportedAvatar = SupportedAvatar;

            if (Item.Title == "" || Item.AuthorName == "" || Item.ItemPath == "")
            {
                MessageBox.Show("タイトル、作者、フォルダパスのどれかが入力されていません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Item.BoothId != -1)
            {
                var thumbnailPath = Path.Combine("./Datas", "Thumbnail", $"{Item.BoothId}.png");
                if (!File.Exists(thumbnailPath))
                {
                    if (string.IsNullOrEmpty(Item.ThumbnailUrl)) return;

                    try
                    {
                        var thumbnailData = await HttpClient.GetByteArrayAsync(Item.ThumbnailUrl);
                        await File.WriteAllBytesAsync(thumbnailPath, thumbnailData);
                        Item.ImagePath = thumbnailPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("サムネイルのダウンロードに失敗しました: " + ex.Message,
                            "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Item.AuthorId))
            {
                var authorImagePath = Path.Combine("./Datas", "AuthorImage", $"{Item.AuthorId}.png");
                if (!File.Exists(authorImagePath))
                {
                    if (string.IsNullOrEmpty(Item.AuthorImageUrl)) return;

                    try
                    {
                        var authorImageData = await HttpClient.GetByteArrayAsync(Item.AuthorImageUrl);
                        await File.WriteAllBytesAsync(authorImagePath, authorImageData);
                        Item.AuthorImageFilePath = authorImagePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("作者の画像のダウンロードに失敗しました: " + ex.Message,
                            "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

            }

            if (_edit)
            {
                // 同じパスのものを削除してから追加
                MessageBox.Show("Boothのアイテムを編集しました!\nアイテム名: " + Item.Title + "\n作者: " + Item.AuthorName, "編集完了",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Where(i => i.ItemPath != Item.ItemPath).ToArray();
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }
            else
            {
                MessageBox.Show("Boothのアイテムを追加しました!\nアイテム名: " + Item.Title + "\n作者: " + Item.AuthorName, "追加完了",
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
                MessageBox.Show("Booth URLが正しくありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && !_edit)
            {
                MessageBox.Show("同じパスのアイテムが既に存在します");
                return;
            }

            try
            {
                GetButton.Enabled = false;
                GetButton.Text = "取得中...";
                Item = await Helper.GetBoothItemInfoAsync(boothId);
                GetButton.Text = "情報を取得";
                GetButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Boothのアイテム情報を取得できませんでした\n" + ex.Message, "エラー", MessageBoxButtons.OK,
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
            SelectAvatar.Text = $"選択中: {SupportedAvatar.Length}個";
        }

        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectAvatar.Enabled = TypeComboBox.SelectedIndex != (int)ItemType.Avatar;
        }

        private void TitleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!_edit && _mainForm.Items.Any(i => i.Title == TitleTextBox.Text))
            {
                SetErrorState("エラー: 同じタイトルのアイテムが既に存在します");
            }
            else if (string.IsNullOrEmpty(TitleTextBox.Text))
            {
                SetErrorState("エラー: タイトルが入力されていません");
            }
            else if (TitleTextBox.Text == "*")
            {
                SetErrorState("エラー: タイトルを*にすることはできません");
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
                SetErrorState("エラー: フォルダパスが存在しません");
            }
            else if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && !_edit)
            {
                SetErrorState("エラー: 同じパスのアイテムが既に存在します");
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
    }
}

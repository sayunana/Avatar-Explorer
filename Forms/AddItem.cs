using System.Net;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class AddItem : Form
    {
        private readonly Main _mainForm;
        public string[] SupportedAvatar = Array.Empty<string>();
        private readonly bool _edit;
        public Item Item = new();

        public AddItem(Main mainForm, ItemType type, bool edit, Item? item)
        {
            _edit = edit;
            _mainForm = mainForm;
            InitializeComponent();
            TypeComboBox.SelectedIndex = (int)type >= 6 ? 0 : (int)type;

            if (!edit) return;
            Text = "アイテムの編集";
            label3.Text = "アイテムの編集";
            BoothURLTextBox.Text = $"https://booth.pm/ja/items/{item!.BoothId}";
            FolderTextBox.Enabled = false;
            FolderTextBox.Text = item.ItemPath;
            SupportedAvatar = item.SupportedAvatar;
            SelectAvatar.Text = $"選択中: {SupportedAvatar.Length}個";
            AddButton.Text = "編集";
        }

        private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            FolderTextBox.Text = dragFilePathArr[0];
        }

        private void FolderTextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        [Obsolete("Obsolete")]
        private async void AddButton_Click(object sender, EventArgs e)
        {
            var boothId = BoothURLTextBox.Text.Split('/').Last();
            if (!int.TryParse(boothId, out _))
            {
                MessageBox.Show("Booth URLが正しくありません");
                return;
            }

            if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && !_edit)
            {
                MessageBox.Show("同じパスのアイテムが既に存在します");
                return;
            }

            if (_edit)
            {
                Item = new Item
                {
                    Title = Item.Title,
                    AuthorName = Item.AuthorName,
                    ThumbnailUrl = Item.ThumbnailUrl,
                    AuthorImageUrl = Item.AuthorImageUrl,
                    ItemPath = FolderTextBox.Text,
                    BoothId = int.Parse(boothId),
                    Type = (ItemType)TypeComboBox.SelectedIndex,
                    SupportedAvatar = SupportedAvatar
                };

                EditItemMeta editItemMeta = new(this, Item);
                editItemMeta.ShowDialog();
            }
            else
            {
                Item = await Helper.GetBoothItemInfoAsync(boothId);
                var index = 1;
                while (_mainForm.Items.Any(i => i.Title == Item.Title))
                {
                    Item.Title = $"{Item.Title} - {index}";
                    index++;
                }

                Item.ItemPath = FolderTextBox.Text;
                Item.BoothId = int.Parse(boothId);
                Item.Type = (ItemType)TypeComboBox.SelectedIndex;
                Item.SupportedAvatar = SupportedAvatar;

                EditItemMeta editItemMeta = new(this, Item);
                editItemMeta.ShowDialog();
            }

            var thumbnailPath = Path.Combine("./Datas", "Thumbnail", $"{Item.BoothId}.png");
            if (!File.Exists(thumbnailPath))
            {
                if (Item.ThumbnailUrl == "Not Found") return;
                using var wc = new WebClient();
                wc.DownloadFile(Item.ThumbnailUrl, thumbnailPath);
            }
            Item.ImagePath = thumbnailPath;

            var authorImagePath = Path.Combine("./Datas", "AuthorImage", $"{Item.AuthorId}.png");
            if (!File.Exists(authorImagePath))
            {
                if (Item.AuthorImageUrl == "Not Found") return;
                using var wc = new WebClient();
                wc.DownloadFile(Item.AuthorImageUrl, authorImagePath);
            }
            Item.AuthorImageFilePath = authorImagePath;

            if (_edit)
            {
                MessageBox.Show("Boothのアイテムを編集しました!\nアイテム名: " + Item.Title + "\n作者: " + Item.AuthorName, "編集完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Where(i => i.ItemPath != Item.ItemPath).ToArray();
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
                Close();
            }
            else
            {
                MessageBox.Show("Boothのアイテムを追加しました!\nアイテム名: " + Item.Title + "\n作者: " + Item.AuthorName, "追加完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }
            Close();
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
    }
}

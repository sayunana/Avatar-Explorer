using System.Net;
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class AddItem : Form
    {
        private readonly Main _mainForm;
        public string[] SupportedAvatar = Array.Empty<string>();

        public AddItem(Main mainForm, ItemType type)
        {
            _mainForm = mainForm;
            InitializeComponent();
            TypeComboBox.SelectedIndex = (int)type is 6 or 7 or 8 ? 0 : (int)type;
        }

        private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] dragFilePathArr = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            FolderTextBox.Text = dragFilePathArr[0];
        }

        private void FolderTextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            var boothId = BoothURLTextBox.Text.Split('/').Last();
            if (!int.TryParse(boothId, out _))
            {
                MessageBox.Show("Booth URLが正しくありません");
                return;
            }

            var item = await Helper.GetBoothItemInfoAsync(boothId);
            item.ItemPath = FolderTextBox.Text;
            item.BoothId = int.Parse(boothId);
            item.Type = (ItemType)TypeComboBox.SelectedIndex;
            item.SupportedAvatar = SupportedAvatar;

            var thumbnailPath = Path.Combine("./Datas", "Thumbnail", $"{item.BoothId}.png");
            using var wc = new WebClient();
            wc.DownloadFile(item.ThumbnailUrl, thumbnailPath);
            item.ImagePath = thumbnailPath;

            var authorImagePath = Path.Combine("./Datas", "AuthorImage", $"{item.AuthorName}.png");
            wc.DownloadFile(item.AuthorImageUrl, authorImagePath);
            item.AuthorImageFilePath = authorImagePath;

            MessageBox.Show("Boothのアイテムを追加しました!\nアイテム名: " + item.Title + "\n作者: " + item.AuthorName, "追加完了", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _mainForm.Items = _mainForm.Items.Append(item).ToArray();
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

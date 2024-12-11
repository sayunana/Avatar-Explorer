using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class EditItemMeta : Form
    {
        private readonly AddItem _addItem;

        public EditItemMeta(AddItem addItem, Item item)
        {
            _addItem = addItem;
            InitializeComponent();
            TitleTextBox.Text = item.Title;
            AuthorTextBox.Text = item.AuthorName;
            AuthorLink.Text = "Author Link: https://" + item.AuthorId + ".booth.pm/";
            BoothLinkLabel.Text = "BoothLink: https://booth.pm/ja/items/" + item.BoothId;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _addItem.Item.Title = TitleTextBox.Text;
            _addItem.Item.AuthorName = AuthorTextBox.Text;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

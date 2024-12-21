using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Avatar_Explorer.Classes
{
    internal class CustomItemButton : Button
    {
        private PictureBox _pictureBox;
        private Label _title;
        private Label _authorName;
        private string _toolTipText;
        private ToolTip _toolTip;

        public Image Picture
        {
            get => _pictureBox.Image;
            set => _pictureBox.Image = value;
        }

        public string TitleText
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        public string AuthorName
        {
            get => _authorName.Text;
            set => _authorName.Text = value;
        }

        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                _toolTipText = value;
                _toolTip.SetToolTip(this, _toolTipText);
            }
        }

        public CustomItemButton(bool isAvatarSelectButton, int buttonWidth)
        {
            Size = new Size(buttonWidth, 64);

            _pictureBox = new PictureBox();
            if (isAvatarSelectButton)
            {
                // アバター選択画面で画像の位置が異なる
                _pictureBox.Location = new Point(3, 3);
            } else {
                _pictureBox.Location = new Point(4, 4);
            }
            _pictureBox.Size = new Size(56, 56);
            _pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(_pictureBox);

            // タイトルが長すぎる場合にボタンの幅を超えないようにする
            // ボタンの幅 - ラベルのX位置 - 余裕を持たせて数px引く
            var labelWidth = buttonWidth - 60 - 5;

            _title = new Label();
            _title.Location = new Point(60, 3);
            _title.Size = new Size(labelWidth, 20);
            _title.Font = new Font("Yu Gothic UI", 12F);
            Controls.Add(_title);

            _authorName = new Label();
            _authorName.Location = new Point(60, 25);
            _authorName.Size = new Size(labelWidth, 20);
            Controls.Add(_authorName);

            _toolTipText = "";
            _toolTip = new ToolTip();

            // 画像とラベルのイベントが発生した際にボタンのイベントを呼び出す
            foreach (Control control in Controls)
            {
                control.MouseEnter += (s, e) => OnMouseEnter(e);
                control.MouseLeave += (s, e) => OnMouseLeave(e);
                control.MouseMove += (s, e) => OnMouseMove(e);
                control.MouseDown += (s, e) => OnMouseDown(e);
                control.MouseClick += (s, e) => OnMouseClick(e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            // ここではクリックされたボタンの種類を取得できないため、何もしない（OnMouseClickで行う）
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            // 左クリックされた場合
            if (e.Button == MouseButtons.Left)
                ProcessClick();
            base.OnMouseClick(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            // エンターキーまたはスペースキーを押下された場合
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                ProcessClick();
            base.OnPreviewKeyDown(e);
        }

        private void ProcessClick()
        {
            // 画像・ラベルをクリックされた場合に備えてフォーカスをボタンに移動させる
            Focus();
            // button.Click += で追加されたイベントを実行する
            base.OnClick(EventArgs.Empty);
        }
    }
}

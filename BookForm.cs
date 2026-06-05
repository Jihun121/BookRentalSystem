using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BookRentalSystem
{
    public partial class BookForm : Form
    {
        TextBox tCode, tCategory, tTitle, tAuthor, tTranslator, tPublisher;
        DateTimePicker dpPublish;
        Label lblCount;
        DataGridView grid;

        public BookForm()
        {
            Text = "도서 정보"; Width = 720; Height = 600;

            UI.Lbl(this, "도서 코드", 20, 20); tCode = UI.Txt(this, 110, 20);
            UI.Lbl(this, "분류", 270, 20); tCategory = UI.Txt(this, 360, 20);
            UI.Lbl(this, "제목", 20, 55); tTitle = UI.Txt(this, 110, 55, 400);
            UI.Lbl(this, "저자", 20, 90); tAuthor = UI.Txt(this, 110, 90);
            UI.Lbl(this, "역자", 270, 90); tTranslator = UI.Txt(this, 360, 90);
            UI.Lbl(this, "출판사", 20, 125); tPublisher = UI.Txt(this, 110, 125);
            UI.Lbl(this, "출판일", 270, 125);
            dpPublish = new DateTimePicker { Left = 360, Top = 125, Width = 150, Format = DateTimePickerFormat.Short };
            Controls.Add(dpPublish);

            UI.Btn(this, "추가", 20, 165, (s, e) => NewMode());
            UI.Btn(this, "저장", 110, 165, (s, e) => Save());
            UI.Btn(this, "삭제", 200, 165, (s, e) => Delete());
            UI.Btn(this, "취소", 290, 165, (s, e) => Load());
            UI.Btn(this, "나가기", 540, 165, (s, e) => Close());

            UI.Lbl(this, "전체 도서 수 :", 20, 205, 110);
            lblCount = new Label { Left = 135, Top = 209, Width = 80, Text = "0권" };
            Controls.Add(lblCount);

            grid = new DataGridView
            {
                Left = 20,
                Top = 235,
                Width = 660,
                Height = 320,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            grid.CellClick += (s, e) => { if (e.RowIndex >= 0) LoadRow(e.RowIndex); };
            Controls.Add(grid);

            Load();
        }

        void Load()
        {
            grid.DataSource = BookDAO.GetAll();
            lblCount.Text = BookDAO.Count() + "권";
        }

        void NewMode()
        {
            tCode.Text = tCategory.Text = tTitle.Text = tAuthor.Text =
                tTranslator.Text = tPublisher.Text = "";
            dpPublish.Value = DateTime.Today;
            tCode.Focus();
        }

        // 그리드 한 줄 클릭 → 입력칸에 채우기
        void LoadRow(int row)
        {
            string code = grid.Rows[row].Cells["코드"].Value.ToString();
            var b = BookDAO.GetByCode(code);
            if (b == null) return;
            tCode.Text = b.BookCode; tCategory.Text = b.Category; tTitle.Text = b.Title;
            tAuthor.Text = b.Author; tTranslator.Text = b.Translator; tPublisher.Text = b.Publisher;
            dpPublish.Value = b.PublishDate;
        }

        // 코드가 있으면 UPDATE, 없으면 INSERT (저장 버튼 하나로 처리)
        void Save()
        {
            if (tCode.Text.Trim() == "")
            { MessageBox.Show("도서 코드를 입력하세요."); return; }

            var b = new Book
            {
                BookCode = tCode.Text.Trim(),
                Category = tCategory.Text.Trim(),
                Title = tTitle.Text.Trim(),
                Author = tAuthor.Text.Trim(),
                Translator = tTranslator.Text.Trim(),
                Publisher = tPublisher.Text.Trim(),
                PublishDate = dpPublish.Value.Date
            };
            try
            {
                if (BookDAO.Exists(b.BookCode)) BookDAO.Update(b);
                else BookDAO.Insert(b);
                Load();
                MessageBox.Show("저장되었습니다.");
            }
            catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
        }

        void Delete()
        {
            if (tCode.Text.Trim() == "") return;
            if (MessageBox.Show("삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { BookDAO.Delete(tCode.Text.Trim()); Load(); NewMode(); }
            catch (Exception ex) { MessageBox.Show("대여 이력이 있으면 삭제 불가.\n" + ex.Message); }
        }
    }
}
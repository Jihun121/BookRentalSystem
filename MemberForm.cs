using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BookRentalSystem
{
    public partial class MemberForm : Form
    {
        TextBox tNo, tName, tJumin, tPhone, tMobile, tZip, tAddr;
        ComboBox cbGrade, cbGender;
        Label lblCount;
        DataGridView grid;

        public MemberForm()
        {
            Text = "회원 정보"; Width = 720; Height = 620;

            UI.Lbl(this, "회원번호", 20, 20); tNo = UI.Txt(this, 110, 20);
            UI.Lbl(this, "주민등록번호", 290, 20); tJumin = UI.Txt(this, 380, 20);
            UI.Lbl(this, "회원명", 20, 55); tName = UI.Txt(this, 110, 55);
            UI.Lbl(this, "회원 등급", 20, 90);
            cbGrade = new ComboBox { Left = 110, Top = 90, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGrade.Items.AddRange(new[] { "일반", "학생" }); cbGrade.SelectedIndex = 0; Controls.Add(cbGrade);
            UI.Lbl(this, "성별", 290, 90);
            cbGender = new ComboBox { Left = 380, Top = 90, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGender.Items.AddRange(new[] { "남자", "여자" }); cbGender.SelectedIndex = 0; Controls.Add(cbGender);
            UI.Lbl(this, "전화번호", 20, 125); tPhone = UI.Txt(this, 110, 125);
            UI.Lbl(this, "휴대폰", 290, 125); tMobile = UI.Txt(this, 380, 125);
            UI.Lbl(this, "우편번호", 20, 160); tZip = UI.Txt(this, 110, 160);
            UI.Lbl(this, "주소", 20, 195); tAddr = UI.Txt(this, 110, 195, 420);

            UI.Btn(this, "추가", 20, 230, (s, e) => NewMode());
            UI.Btn(this, "저장", 110, 230, (s, e) => Save());
            UI.Btn(this, "삭제", 200, 230, (s, e) => Delete());
            UI.Btn(this, "취소", 290, 230, (s, e) => Load());
            UI.Btn(this, "카드 관리", 400, 230, (s, e) => IssueCard(), 90);
            UI.Btn(this, "나가기", 540, 230, (s, e) => Close());

            UI.Lbl(this, "현재 회원 수 :", 20, 270, 100);
            lblCount = new Label { Left = 125, Top = 274, Width = 80, Text = "0명" };
            Controls.Add(lblCount);

            grid = new DataGridView
            {
                Left = 20,
                Top = 300,
                Width = 660,
                Height = 270,
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
            grid.DataSource = MemberDAO.GetAll();
            lblCount.Text = MemberDAO.Count() + "명";
        }

        void NewMode()
        {
            tNo.Text = tName.Text = tJumin.Text = tPhone.Text =
                tMobile.Text = tZip.Text = tAddr.Text = "";
            cbGrade.SelectedIndex = 0; cbGender.SelectedIndex = 0; tNo.Focus();
        }

        void LoadRow(int row)
        {
            int no = Convert.ToInt32(grid.Rows[row].Cells["코드"].Value);
            var m = MemberDAO.GetByNo(no);
            if (m == null) return;
            tNo.Text = m.MemberNo.ToString(); tName.Text = m.Name; tJumin.Text = m.Jumin;
            cbGrade.Text = m.Grade; cbGender.Text = m.Gender;
            tPhone.Text = m.Phone; tMobile.Text = m.Mobile; tZip.Text = m.ZipCode; tAddr.Text = m.Address;
        }

        Member Read()
        {
            int no;
            if (!int.TryParse(tNo.Text.Trim(), out no))
            { MessageBox.Show("회원번호는 숫자입니다."); return null; }
            return new Member
            {
                MemberNo = no,
                Name = tName.Text.Trim(),
                Jumin = tJumin.Text.Trim(),
                Grade = cbGrade.Text,
                Gender = cbGender.Text,
                Phone = tPhone.Text.Trim(),
                Mobile = tMobile.Text.Trim(),
                ZipCode = tZip.Text.Trim(),
                Address = tAddr.Text.Trim()
            };
        }

        void Save()
        {
            var m = Read(); if (m == null) return;
            try
            {
                if (MemberDAO.Exists(m.MemberNo)) MemberDAO.Update(m);
                else MemberDAO.Insert(m);
                Load(); MessageBox.Show("저장되었습니다.");
            }
            catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
        }

        void Delete()
        {
            int no;
            if (!int.TryParse(tNo.Text.Trim(), out no)) return;
            if (MessageBox.Show("삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { MemberDAO.Delete(no); Load(); NewMode(); }
            catch (Exception ex) { MessageBox.Show("대여 이력이 있으면 삭제 불가.\n" + ex.Message); }
        }

        // RFID 카드 발급
        void IssueCard()
        {
            int no;
            if (!int.TryParse(tNo.Text.Trim(), out no) || !MemberDAO.Exists(no))
            { MessageBox.Show("저장된 회원을 먼저 선택하세요."); return; }
            string card = MemberDAO.IssueCard(no);
            MessageBox.Show("RFID 카드 발급 완료\n카드번호: " + card);
        }
    }
}
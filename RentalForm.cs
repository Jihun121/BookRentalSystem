using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BookRentalSystem
{
    public partial class RentalForm : Form
    {
        TabControl tab;
        TextBox tInName, tInCode, tInPhone, tInMobile;                      // 회원 입력 탭
        DataGridView gridSelect;                                            // 회원 선택 탭(동명이인)
        TextBox iCode, iName, iJumin, iGrade, iPhone, iMobile, iZip, iAddr; // 회원 정보 표시
        TextBox tBCode, tBTitle, tRentFee, tOverdue, tDue;
        DateTimePicker dpRent;
        Label lblTotCnt, lblTotFee, lblTotOver;
        DataGridView grid;
        int curMember = -1;   // 현재 선택된 회원번호

        public RentalForm()
        {
            Text = "대여 관리"; Width = 980; Height = 700;

            // --- 회원 검색 탭 ---
            tab = new TabControl { Left = 15, Top = 15, Width = 430, Height = 220 };
            var p1 = new TabPage("회원 입력");
            var p2 = new TabPage("회원 선택");
            tab.TabPages.Add(p1); tab.TabPages.Add(p2); Controls.Add(tab);

            UI.Lbl(p1, "회원명", 10, 15); tInName = UI.Txt(p1, 90, 15);
            UI.Lbl(p1, "회원코드", 10, 50); tInCode = UI.Txt(p1, 90, 50);
            UI.Lbl(p1, "전화번호", 10, 85); tInPhone = UI.Txt(p1, 90, 85);
            UI.Lbl(p1, "휴대폰", 10, 120); tInMobile = UI.Txt(p1, 90, 120);
            UI.Btn(p1, "카드 읽기", 30, 155, (s, e) => ReadCard(), 90);
            UI.Btn(p1, "찾기", 140, 155, (s, e) => Find());

            gridSelect = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            gridSelect.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int no = Convert.ToInt32(gridSelect.Rows[e.RowIndex].Cells["코드"].Value);
                ShowMember(MemberDAO.GetByNo(no));
            };
            p2.Controls.Add(gridSelect);

            // --- 회원 정보 표시 ---
            int bx = 470;
            UI.Lbl(this, "회원코드", bx, 20); iCode = UI.Txt(this, bx + 80, 20, 140, true);
            UI.Lbl(this, "회원명", bx + 240, 20); iName = UI.Txt(this, bx + 320, 20, 140, true);
            UI.Lbl(this, "주민번호", bx, 55); iJumin = UI.Txt(this, bx + 80, 55, 140, true);
            UI.Lbl(this, "회원등급", bx + 240, 55); iGrade = UI.Txt(this, bx + 320, 55, 140, true);
            UI.Lbl(this, "전화번호", bx, 90); iPhone = UI.Txt(this, bx + 80, 90, 140, true);
            UI.Lbl(this, "휴대폰", bx + 240, 90); iMobile = UI.Txt(this, bx + 320, 90, 140, true);
            UI.Lbl(this, "우편번호", bx, 125); iZip = UI.Txt(this, bx + 80, 125, 140, true);
            UI.Lbl(this, "주소", bx, 160); iAddr = UI.Txt(this, bx + 80, 160, 380, true);

            // --- 대여 도서 입력 ---
            UI.Lbl(this, "도서 코드", 15, 260, 90); tBCode = UI.Txt(this, 110, 260, 120);
            UI.Lbl(this, "도서 제목", 250, 260, 90); tBTitle = UI.Txt(this, 345, 260, 300, true);
            UI.Btn(this, "등록", 670, 258, (s, e) => RegisterRental());

            UI.Lbl(this, "대여료", 15, 300, 60); tRentFee = UI.Txt(this, 80, 300, 90, true);
            UI.Lbl(this, "연체료", 190, 300, 60); tOverdue = UI.Txt(this, 255, 300, 90, true);
            UI.Lbl(this, "대여일", 360, 300, 50);
            dpRent = new DateTimePicker { Left = 415, Top = 300, Width = 130, Format = DateTimePickerFormat.Short };
            Controls.Add(dpRent);
            UI.Lbl(this, "반납 예정일", 560, 300, 80); tDue = UI.Txt(this, 645, 300, 130, true);

            UI.Lbl(this, "총 대여 권수", 15, 340, 90);
            lblTotCnt = new Label { Left = 110, Top = 344, Width = 60, Text = "0권" }; Controls.Add(lblTotCnt);
            UI.Lbl(this, "총 대여료", 200, 340, 70);
            lblTotFee = new Label { Left = 275, Top = 344, Width = 90, Text = "0원" }; Controls.Add(lblTotFee);
            UI.Lbl(this, "총 연체료", 400, 340, 70);
            lblTotOver = new Label { Left = 475, Top = 344, Width = 90, Text = "0원" }; Controls.Add(lblTotOver);
            UI.Btn(this, "도서 반납", 670, 338, (s, e) => ReturnBook(), 100);

            grid = new DataGridView
            {
                Left = 15,
                Top = 380,
                Width = 940,
                Height = 270,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(grid);

            // 바코드 스캐너 입력 후 Enter → 바로 등록
            tBCode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) RegisterRental(); };
        }

        // 카드(RFID) 읽기 → 카드번호로 회원 조회
        void ReadCard()
        {
            string card = UI.Prompt("카드 읽기", "RFID 카드번호를 입력(스캔)하세요.");
            if (string.IsNullOrEmpty(card)) return;
            var m = MemberDAO.GetByCard(card);
            if (m == null) MessageBox.Show("해당 카드 회원이 없습니다.");
            else ShowMember(m);
        }

        // 코드 우선, 없으면 이름으로 검색. 동명이인이면 선택 탭으로
        void Find()
        {
            if (tInCode.Text.Trim() != "")
            {
                int no;
                if (int.TryParse(tInCode.Text.Trim(), out no))
                {
                    var m = MemberDAO.GetByNo(no);
                    if (m == null) MessageBox.Show("회원이 없습니다."); else ShowMember(m);
                }
                return;
            }
            if (tInName.Text.Trim() == "") { MessageBox.Show("회원명 또는 코드를 입력하세요."); return; }

            var dt = MemberDAO.GetByName(tInName.Text.Trim());
            if (dt.Rows.Count == 0) { MessageBox.Show("회원이 없습니다."); return; }
            if (dt.Rows.Count == 1)
            {
                ShowMember(MemberDAO.GetByNo(Convert.ToInt32(dt.Rows[0]["코드"])));
            }
            else
            {
                gridSelect.DataSource = dt;
                tab.SelectedIndex = 1;
                MessageBox.Show("동명이인이 있습니다. '회원 선택' 탭에서 더블클릭하세요.");
            }
        }

        void ShowMember(Member m)
        {
            if (m == null) return;
            curMember = m.MemberNo;
            iCode.Text = m.MemberNo.ToString(); iName.Text = m.Name; iJumin.Text = m.Jumin;
            iGrade.Text = m.Grade; iPhone.Text = m.Phone; iMobile.Text = m.Mobile;
            iZip.Text = m.ZipCode; iAddr.Text = m.Address;
            LoadRentals();
        }

        // 도서 대여 등록 (신간/구간 자동 판정 + 요금 자동 적용)
        void RegisterRental()
        {
            if (curMember < 0) { MessageBox.Show("회원을 먼저 선택하세요."); return; }
            string code = tBCode.Text.Trim();
            if (code == "") return;

            var b = BookDAO.GetByCode(code);
            if (b == null) { MessageBox.Show("도서가 없습니다."); return; }
            if (RentalDAO.IsActive(code)) { MessageBox.Show("이미 대여 중인 도서입니다."); return; }

            var s = SettingDAO.Get();
            bool isNew = (DateTime.Today - b.PublishDate).TotalDays <= s.SwitchPeriod;
            int days = isNew ? s.NewRentDays : s.OldRentDays;
            int fee = isNew ? s.NewRentFee : s.OldRentFee;
            int rate = isNew ? s.NewOverdueFee : s.OldOverdueFee;
            DateTime rent = dpRent.Value.Date;
            DateTime due = rent.AddDays(days);

            RentalDAO.Rent(curMember, code, rent, due, fee, rate);

            tBTitle.Text = b.Title;
            tRentFee.Text = fee + "원";
            tOverdue.Text = rate + "원/일";
            tDue.Text = due.ToString("yyyy-MM-dd");
            tBCode.Text = "";
            LoadRentals();
            tBCode.Focus();
        }

        // 현재 회원이 대여중인 목록 + 합계
        void LoadRentals()
        {
            var raw = RentalDAO.GetActiveRaw(curMember);
            var dt = new DataTable();
            dt.Columns.Add("번호"); dt.Columns.Add("도서코드"); dt.Columns.Add("제목");
            dt.Columns.Add("대여일"); dt.Columns.Add("반납예정일");
            dt.Columns.Add("대여료"); dt.Columns.Add("연체료(예상)"); dt.Columns.Add("상태");

            int totFee = 0, totOver = 0;
            foreach (DataRow r in raw.Rows)
            {
                DateTime due = Convert.ToDateTime(r["DueDate"]);
                int rate = Convert.ToInt32(r["OverdueRate"]);
                int fee = Convert.ToInt32(r["RentFee"]);
                int late = Math.Max(0, (DateTime.Today - due).Days);
                int over = late * rate;
                dt.Rows.Add(r["RentalId"], r["BookCode"], r["Title"],
                    Convert.ToDateTime(r["RentDate"]).ToString("yyyy-MM-dd"),
                    due.ToString("yyyy-MM-dd"), fee, over, late > 0 ? "연체" : "대여중");
                totFee += fee; totOver += over;
            }
            grid.DataSource = dt;
            lblTotCnt.Text = dt.Rows.Count + "권";
            lblTotFee.Text = totFee + "원";
            lblTotOver.Text = totOver + "원";
        }

        // 선택한 도서 반납
        void ReturnBook()
        {
            if (grid.CurrentRow == null) { MessageBox.Show("반납할 도서를 선택하세요."); return; }
            int id = Convert.ToInt32(grid.CurrentRow.Cells["번호"].Value);
            RentalDAO.Return(id, DateTime.Today);
            LoadRentals();
            MessageBox.Show("반납 처리되었습니다.");
        }
    }
}
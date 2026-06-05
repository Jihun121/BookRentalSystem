using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BookRentalSystem
{
    public partial class SettingForm : Form
    {
        TextBox tSwitch, tNewDays, tNewFee, tNewOver, tOldDays, tOldFee, tOldOver;

        public SettingForm()
        {
            Text = "환경 설정 / 도서 대여 설정"; Width = 620; Height = 480;

            var gNew = new GroupBox { Text = "신간", Left = 20, Top = 20, Width = 560, Height = 110 };
            Controls.Add(gNew);
            UI.Lbl(gNew, "전환 기간(일)", 15, 25, 90); tSwitch = UI.Txt(gNew, 110, 25, 60);
            UI.Lbl(gNew, "대여료", 300, 25, 50); tNewFee = UI.Txt(gNew, 355, 25, 80);
            UI.Lbl(gNew, "대여 기간(일)", 15, 60, 90); tNewDays = UI.Txt(gNew, 110, 60, 60);
            UI.Lbl(gNew, "연체료(/일)", 300, 60, 70); tNewOver = UI.Txt(gNew, 375, 60, 80);

            var gOld = new GroupBox { Text = "구간", Left = 20, Top = 140, Width = 560, Height = 110 };
            Controls.Add(gOld);
            UI.Lbl(gOld, "대여 기간(일)", 15, 25, 90); tOldDays = UI.Txt(gOld, 110, 25, 60);
            UI.Lbl(gOld, "대여료", 300, 25, 50); tOldFee = UI.Txt(gOld, 355, 25, 80);
            UI.Lbl(gOld, "연체료(/일)", 300, 60, 70); tOldOver = UI.Txt(gOld, 375, 60, 80);

            UI.Btn(this, "수정 저장", 20, 265, (s, e) => SaveSetting(), 90);
            UI.Btn(this, "나가기", 490, 265, (s, e) => Close());

            var gExcel = new GroupBox { Text = "엑셀로 출력", Left = 20, Top = 310, Width = 560, Height = 100 };
            Controls.Add(gExcel);
            UI.Btn(gExcel, "도서 목록", 30, 35, (s, e) => ExportBookList(), 120);
            UI.Btn(gExcel, "대여 현황", 180, 35, (s, e) => ExportRentalStatus(), 120);

            LoadSetting();
        }

        void LoadSetting()
        {
            var s = SettingDAO.Get();
            tSwitch.Text = s.SwitchPeriod.ToString();
            tNewDays.Text = s.NewRentDays.ToString(); tNewFee.Text = s.NewRentFee.ToString();
            tNewOver.Text = s.NewOverdueFee.ToString();
            tOldDays.Text = s.OldRentDays.ToString(); tOldFee.Text = s.OldRentFee.ToString();
            tOldOver.Text = s.OldOverdueFee.ToString();
        }

        void SaveSetting()
        {
            try
            {
                var s = new Setting
                {
                    SwitchPeriod = int.Parse(tSwitch.Text),
                    NewRentDays = int.Parse(tNewDays.Text),
                    NewRentFee = int.Parse(tNewFee.Text),
                    NewOverdueFee = int.Parse(tNewOver.Text),
                    OldRentDays = int.Parse(tOldDays.Text),
                    OldRentFee = int.Parse(tOldFee.Text),
                    OldOverdueFee = int.Parse(tOldOver.Text)
                };
                SettingDAO.Save(s);
                MessageBox.Show("설정이 저장되었습니다.");
            }
            catch { MessageBox.Show("숫자만 입력하세요."); }
        }

        void ExportBookList() => ToCsv(BookDAO.GetAll(), "도서목록");

        void ExportRentalStatus()
        {
            var dt = DBManager.Query(
                @"SELECT r.BookCode AS 코드, b.Title AS 제목, m.Name AS 회원명,
                         CONVERT(varchar(10),r.RentDate,23) AS 대여일,
                         CONVERT(varchar(10),r.DueDate,23) AS 반납예정일,
                         CASE WHEN r.IsReturned=1 THEN '반납' ELSE '대여중' END AS 상태,
                         r.RentFee AS 대여료, r.OverdueFee AS 연체료
                  FROM Rental r JOIN Book b ON r.BookCode=b.BookCode
                                JOIN Member m ON r.MemberNo=m.MemberNo
                  ORDER BY r.RentDate DESC");
            ToCsv(dt, "대여현황");
        }

        // 외부 라이브러리 없이 CSV(UTF-8 BOM)로 저장 → 엑셀에서 바로 열림.
        // 진짜 .xlsx가 필요하면 ClosedXML NuGet으로 교체.
        void ToCsv(DataTable dt, string name)
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV 파일|*.csv", FileName = name + ".csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                for (int i = 0; i < dt.Columns.Count; i++)
                    sb.Append(dt.Columns[i].ColumnName + (i < dt.Columns.Count - 1 ? "," : "\r\n"));
                foreach (DataRow r in dt.Rows)
                    for (int i = 0; i < dt.Columns.Count; i++)
                        sb.Append("\"" + r[i].ToString().Replace("\"", "\"\"") + "\""
                                  + (i < dt.Columns.Count - 1 ? "," : "\r\n"));
                File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("저장 완료: " + sfd.FileName);
            }
        }
    }
}
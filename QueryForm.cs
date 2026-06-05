using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BookRentalSystem
{
    public partial class QueryForm : Form
    {
        RadioButton rbBookRank, rbActive, rbMemberRank;
        ComboBox cbCategory, cbGrade;
        DataGridView grid;

        public QueryForm()
        {
            Text = "도서 조회 관리"; Width = 900; Height = 640;

            rbBookRank = new RadioButton { Text = "도서 대여 순위", Left = 20, Top = 20, Width = 140, Checked = true };
            rbActive = new RadioButton { Text = "대여중인 도서", Left = 180, Top = 20, Width = 140 };
            rbMemberRank = new RadioButton { Text = "회원 대여 순위", Left = 20, Top = 50, Width = 140 };
            Controls.AddRange(new Control[] { rbBookRank, rbActive, rbMemberRank });

            UI.Lbl(this, "분류", 360, 20, 50);
            cbCategory = new ComboBox { Left = 415, Top = 20, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategory.Items.Add("전체");
            foreach (System.Data.DataRow r in BookDAO.Categories().Rows) cbCategory.Items.Add(r[0].ToString());
            cbCategory.SelectedIndex = 0; Controls.Add(cbCategory);

            UI.Lbl(this, "회원 등급", 350, 55, 60);
            cbGrade = new ComboBox { Left = 415, Top = 55, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGrade.Items.Add("전체");
            foreach (System.Data.DataRow r in MemberDAO.Grades().Rows) cbGrade.Items.Add(r[0].ToString());
            cbGrade.SelectedIndex = 0; Controls.Add(cbGrade);

            UI.Btn(this, "검색", 600, 30, (s, e) => Search());
            UI.Btn(this, "나가기", 700, 30, (s, e) => Close());

            grid = new DataGridView
            {
                Left = 20,
                Top = 100,
                Width = 850,
                Height = 480,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(grid);
        }

        void Search()
        {
            var ps = new List<SqlParameter>();
            string sql;

            if (rbBookRank.Checked)   // 도서 대여 순위
            {
                sql = @"SELECT COUNT(*) AS 대여횟수, b.BookCode AS 코드, b.Title AS 제목,
                               b.Category AS 분류, b.Author AS 저자
                        FROM Rental r JOIN Book b ON r.BookCode=b.BookCode";
                if (cbCategory.Text != "전체") { sql += " WHERE b.Category=@c"; ps.Add(new SqlParameter("@c", cbCategory.Text)); }
                sql += " GROUP BY b.BookCode,b.Title,b.Category,b.Author ORDER BY 대여횟수 DESC";
            }
            else if (rbMemberRank.Checked)  // 회원 대여 순위
            {
                sql = @"SELECT COUNT(*) AS 대여횟수, m.Name AS [회원 이름], m.Grade AS 등급,
                               m.Gender AS 성별, m.Phone AS 연락처, m.Mobile AS 휴대폰, m.Address AS 주소
                        FROM Rental r JOIN Member m ON r.MemberNo=m.MemberNo";
                if (cbGrade.Text != "전체") { sql += " WHERE m.Grade=@gr"; ps.Add(new SqlParameter("@gr", cbGrade.Text)); }
                sql += " GROUP BY m.Name,m.Grade,m.Gender,m.Phone,m.Mobile,m.Address ORDER BY 대여횟수 DESC";
            }
            else  // 대여중인 도서
            {
                sql = @"SELECT b.BookCode AS 코드, b.Title AS 제목, b.Category AS 분류,
                               m.Name AS 회원명, m.Grade AS 등급,
                               CONVERT(varchar(10),r.RentDate,23) AS 대여일,
                               CONVERT(varchar(10),r.DueDate,23) AS 반납예정일
                        FROM Rental r JOIN Book b ON r.BookCode=b.BookCode
                                      JOIN Member m ON r.MemberNo=m.MemberNo
                        WHERE r.IsReturned=0";
                if (cbCategory.Text != "전체") { sql += " AND b.Category=@c"; ps.Add(new SqlParameter("@c", cbCategory.Text)); }
                if (cbGrade.Text != "전체") { sql += " AND m.Grade=@gr"; ps.Add(new SqlParameter("@gr", cbGrade.Text)); }
                sql += " ORDER BY r.DueDate";
            }

            grid.DataSource = DBManager.Query(sql, ps.ToArray());
        }
    }
}
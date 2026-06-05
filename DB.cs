using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BookRentalSystem
{
    public static class DBManager
    {
        // ★ 환경에 맞게 수정하는 곳 ★
        // LocalDB:   Server=(localdb)\MSSQLLocalDB;Database=BookRentalDB;Integrated Security=True;
        // Express:   Server=.\SQLEXPRESS;Database=BookRentalDB;Integrated Security=True;
        // 계정 로그인: Server=localhost;Database=BookRentalDB;User Id=sa;Password=암호;
        public static string ConnectionString =
            @"Server=PC100\SQLEXPRESS01;Database=BookRentalDB;Uid=sa;Pwd=std001;TrustServerCertificate=True;";

        // SELECT → DataTable 반환
        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }

        // INSERT / UPDATE / DELETE → 영향받은 행 수
        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // COUNT 등 단일 값
        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }
    }

    // 디자이너 없이 코드로 화면을 짜기 위한 보조 함수 모음
    public static class UI
    {
        public static Label Lbl(Control p, string t, int x, int y, int w = 80)
        {
            var l = new Label
            {
                Text = t,
                Left = x,
                Top = y + 4,
                Width = w,
                TextAlign = ContentAlignment.MiddleRight
            };
            p.Controls.Add(l); return l;
        }
        public static TextBox Txt(Control p, int x, int y, int w = 150, bool ro = false)
        {
            var t = new TextBox { Left = x, Top = y, Width = w, ReadOnly = ro };
            p.Controls.Add(t); return t;
        }
        public static Button Btn(Control p, string t, int x, int y, EventHandler click, int w = 80)
        {
            var b = new Button { Text = t, Left = x, Top = y, Width = w, Height = 28 };
            b.Click += click; p.Controls.Add(b); return b;
        }
        // 간단한 입력 대화상자 (카드번호 입력 등에 사용)
        public static string Prompt(string title, string label, string def = "")
        {
            using (var f = new Form
            {
                Width = 340,
                Height = 150,
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var l = new Label { Left = 12, Top = 15, Width = 300, Text = label };
                var t = new TextBox { Left = 12, Top = 40, Width = 300, Text = def };
                var ok = new Button { Text = "확인", Left = 150, Top = 75, DialogResult = DialogResult.OK };
                f.Controls.AddRange(new Control[] { l, t, ok }); f.AcceptButton = ok;
                return f.ShowDialog() == DialogResult.OK ? t.Text.Trim() : null;
            }
        }
    }
}

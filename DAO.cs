using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BookRentalSystem
{
    // ===== 도서 =====
    public static class BookDAO
    {
        public static DataTable GetAll() => DBManager.Query(
            @"SELECT BookCode AS 코드, Category AS 분류, Title AS 제목, Author AS 저자,
                     Translator AS 역자, Publisher AS 출판사,
                     CONVERT(varchar(10),PublishDate,23) AS 출판일
              FROM Book ORDER BY BookCode");

        public static int Count() => (int)DBManager.Scalar("SELECT COUNT(*) FROM Book");

        public static bool Exists(string code) =>
            (int)DBManager.Scalar("SELECT COUNT(*) FROM Book WHERE BookCode=@c",
                new SqlParameter("@c", code)) > 0;

        public static Book GetByCode(string code)
        {
            var dt = DBManager.Query("SELECT * FROM Book WHERE BookCode=@c",
                new SqlParameter("@c", code));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Book
            {
                BookCode = r["BookCode"].ToString(),
                Category = r["Category"].ToString(),
                Title = r["Title"].ToString(),
                Author = r["Author"].ToString(),
                Translator = r["Translator"].ToString(),
                Publisher = r["Publisher"].ToString(),
                PublishDate = r["PublishDate"] == DBNull.Value
                    ? DateTime.Today : Convert.ToDateTime(r["PublishDate"])
            };
        }

        public static void Insert(Book b) => DBManager.Execute(
            @"INSERT INTO Book(BookCode,Category,Title,Author,Translator,Publisher,PublishDate)
              VALUES(@code,@c,@t,@a,@tr,@p,@d)", P(b));

        public static void Update(Book b) => DBManager.Execute(
            @"UPDATE Book SET Category=@c,Title=@t,Author=@a,Translator=@tr,Publisher=@p,PublishDate=@d
              WHERE BookCode=@code", P(b));

        public static void Delete(string code) => DBManager.Execute(
            "DELETE FROM Book WHERE BookCode=@c", new SqlParameter("@c", code));

        public static DataTable Categories() => DBManager.Query(
            "SELECT DISTINCT Category FROM Book WHERE Category IS NOT NULL ORDER BY Category");

        static SqlParameter[] P(Book b) => new[]
        {
            new SqlParameter("@code", b.BookCode),
            new SqlParameter("@c", (object)b.Category ?? DBNull.Value),
            new SqlParameter("@t", (object)b.Title ?? DBNull.Value),
            new SqlParameter("@a", (object)b.Author ?? DBNull.Value),
            new SqlParameter("@tr", (object)b.Translator ?? DBNull.Value),
            new SqlParameter("@p", (object)b.Publisher ?? DBNull.Value),
            new SqlParameter("@d", b.PublishDate)
        };
    }

    // ===== 회원 =====
    public static class MemberDAO
    {
        public static DataTable GetAll() => DBManager.Query(
            @"SELECT MemberNo AS 코드, Name AS 성명, Jumin AS 주민등록번,
                     Grade AS 등급, Gender AS 성별, Phone AS 연락처, Mobile AS 휴대폰
              FROM Member ORDER BY MemberNo");

        public static int Count() => (int)DBManager.Scalar("SELECT COUNT(*) FROM Member");

        public static bool Exists(int no) =>
            (int)DBManager.Scalar("SELECT COUNT(*) FROM Member WHERE MemberNo=@n",
                new SqlParameter("@n", no)) > 0;

        public static Member GetByNo(int no) => Map(
            DBManager.Query("SELECT * FROM Member WHERE MemberNo=@n", new SqlParameter("@n", no)));

        public static Member GetByCard(string card) => Map(
            DBManager.Query("SELECT * FROM Member WHERE CardId=@c", new SqlParameter("@c", card)));

        public static DataTable GetByName(string name) => DBManager.Query(
            @"SELECT MemberNo AS 코드, Name AS 성명, Jumin AS 주민번호,
                     Grade AS 등급, Phone AS 연락처, Mobile AS 휴대폰
              FROM Member WHERE Name=@nm ORDER BY MemberNo",
            new SqlParameter("@nm", name));

        public static void Insert(Member m) => DBManager.Execute(
            @"INSERT INTO Member(MemberNo,Name,Jumin,Grade,Gender,Phone,Mobile,ZipCode,Address,CardId)
              VALUES(@no,@nm,@j,@g,@s,@p,@m,@z,@a,@cd)", P(m));

        public static void Update(Member m) => DBManager.Execute(
            @"UPDATE Member SET Name=@nm,Jumin=@j,Grade=@g,Gender=@s,Phone=@p,
                     Mobile=@m,ZipCode=@z,Address=@a WHERE MemberNo=@no", P(m));

        public static void Delete(int no) => DBManager.Execute(
            "DELETE FROM Member WHERE MemberNo=@n", new SqlParameter("@n", no));

        // RFID 카드 발급 (실하드웨어 대신 카드번호 생성·저장)
        public static string IssueCard(int no)
        {
            string card = "RFID-" + no + "-" + new Random().Next(1000, 9999);
            DBManager.Execute("UPDATE Member SET CardId=@c WHERE MemberNo=@n",
                new SqlParameter("@c", card), new SqlParameter("@n", no));
            return card;
        }

        public static DataTable Grades() => DBManager.Query(
            "SELECT DISTINCT Grade FROM Member WHERE Grade IS NOT NULL ORDER BY Grade");

        static Member Map(DataTable dt)
        {
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new Member
            {
                MemberNo = Convert.ToInt32(r["MemberNo"]),
                Name = r["Name"].ToString(),
                Jumin = r["Jumin"].ToString(),
                Grade = r["Grade"].ToString(),
                Gender = r["Gender"].ToString(),
                Phone = r["Phone"].ToString(),
                Mobile = r["Mobile"].ToString(),
                ZipCode = r["ZipCode"].ToString(),
                Address = r["Address"].ToString(),
                CardId = r["CardId"].ToString()
            };
        }

        static SqlParameter[] P(Member m) => new[]
        {
            new SqlParameter("@no", m.MemberNo),
            new SqlParameter("@nm", (object)m.Name ?? DBNull.Value),
            new SqlParameter("@j", (object)m.Jumin ?? DBNull.Value),
            new SqlParameter("@g", (object)m.Grade ?? DBNull.Value),
            new SqlParameter("@s", (object)m.Gender ?? DBNull.Value),
            new SqlParameter("@p", (object)m.Phone ?? DBNull.Value),
            new SqlParameter("@m", (object)m.Mobile ?? DBNull.Value),
            new SqlParameter("@z", (object)m.ZipCode ?? DBNull.Value),
            new SqlParameter("@a", (object)m.Address ?? DBNull.Value),
            new SqlParameter("@cd", (object)m.CardId ?? DBNull.Value)
        };
    }

    // ===== 대여 =====
    public static class RentalDAO
    {
        public static bool IsActive(string bookCode) =>
            (int)DBManager.Scalar(
                "SELECT COUNT(*) FROM Rental WHERE BookCode=@b AND IsReturned=0",
                new SqlParameter("@b", bookCode)) > 0;

        public static DataTable GetActiveRaw(int memberNo) => DBManager.Query(
            @"SELECT r.RentalId, r.BookCode, b.Title, r.RentDate, r.DueDate,
                     r.RentFee, r.OverdueRate
              FROM Rental r JOIN Book b ON r.BookCode=b.BookCode
              WHERE r.MemberNo=@no AND r.IsReturned=0
              ORDER BY r.RentDate",
            new SqlParameter("@no", memberNo));

        public static void Rent(int memberNo, string bookCode, DateTime rent,
                                DateTime due, int rentFee, int overdueRate) =>
            DBManager.Execute(
                @"INSERT INTO Rental(MemberNo,BookCode,RentDate,DueDate,RentFee,OverdueRate)
                  VALUES(@c,@b,@rd,@dd,@rf,@or)",
                new SqlParameter("@c", memberNo), new SqlParameter("@b", bookCode),
                new SqlParameter("@rd", rent), new SqlParameter("@dd", due),
                new SqlParameter("@rf", rentFee), new SqlParameter("@or", overdueRate));

        // 반납: 반납 처리 + 연체일수 × 연체단가로 연체료 확정
        public static void Return(int rentalId, DateTime returnDate) =>
            DBManager.Execute(
                @"UPDATE Rental
                  SET IsReturned=1, ReturnDate=@d,
                      OverdueFee = CASE WHEN DATEDIFF(day,DueDate,@d) > 0
                                        THEN DATEDIFF(day,DueDate,@d)*OverdueRate ELSE 0 END
                  WHERE RentalId=@id",
                new SqlParameter("@d", returnDate), new SqlParameter("@id", rentalId));
    }

    // ===== 설정 =====
    public static class SettingDAO
    {
        public static Setting Get()
        {
            var r = DBManager.Query("SELECT * FROM RentalSetting WHERE Id=1").Rows[0];
            return new Setting
            {
                SwitchPeriod = (int)r["SwitchPeriod"],
                NewRentDays = (int)r["NewRentDays"],
                NewRentFee = (int)r["NewRentFee"],
                NewOverdueFee = (int)r["NewOverdueFee"],
                OldRentDays = (int)r["OldRentDays"],
                OldRentFee = (int)r["OldRentFee"],
                OldOverdueFee = (int)r["OldOverdueFee"]
            };
        }

        public static void Save(Setting s) => DBManager.Execute(
            @"UPDATE RentalSetting SET SwitchPeriod=@sp,NewRentDays=@nd,NewRentFee=@nf,
                     NewOverdueFee=@no,OldRentDays=@od,OldRentFee=@of,OldOverdueFee=@oo
              WHERE Id=1",
            new SqlParameter("@sp", s.SwitchPeriod),
            new SqlParameter("@nd", s.NewRentDays), new SqlParameter("@nf", s.NewRentFee),
            new SqlParameter("@no", s.NewOverdueFee),
            new SqlParameter("@od", s.OldRentDays), new SqlParameter("@of", s.OldRentFee),
            new SqlParameter("@oo", s.OldOverdueFee));
    }
}
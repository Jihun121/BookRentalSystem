using System;
using System.Collections.Generic;
using System.Text;

namespace BookRentalSystem
{
    public class Book
    {
        public string BookCode, Category, Title, Author, Translator, Publisher;
        public DateTime PublishDate = DateTime.Today;
    }

    public class Member
    {
        public int MemberNo;
        public string Name, Jumin, Grade, Gender, Phone, Mobile, ZipCode, Address, CardId;
    }

    public class Setting
    {
        public int SwitchPeriod, NewRentDays, NewRentFee, NewOverdueFee,
                   OldRentDays, OldRentFee, OldOverdueFee;
    }
}

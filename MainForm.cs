namespace BookRentalSystem
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Text = "도서 관리 프로그램";
            IsMdiContainer = true;                 // 자식 창을 품는 컨테이너
            WindowState = FormWindowState.Maximized;

            var menu = new MenuStrip();

            var mFile = new ToolStripMenuItem("파일");
            mFile.DropDownItems.Add("종료", null, (s, e) => Close());

            var mRent = new ToolStripMenuItem("도서 대여/반납");
            mRent.DropDownItems.Add("대여 관리", null, (s, e) => Open(new RentalForm()));

            var mBook = new ToolStripMenuItem("도서 관리");
            mBook.DropDownItems.Add("도서 정보", null, (s, e) => Open(new BookForm()));

            var mMember = new ToolStripMenuItem("회원 관리");
            mMember.DropDownItems.Add("회원 정보", null, (s, e) => Open(new MemberForm()));

            var mQuery = new ToolStripMenuItem("정보 조회");
            mQuery.DropDownItems.Add("정보 조회", null, (s, e) => Open(new QueryForm()));

            var mEnv = new ToolStripMenuItem("환경설정");
            mEnv.DropDownItems.Add("환경 설정", null, (s, e) => Open(new SettingForm()));

            menu.Items.AddRange(new ToolStripItem[] { mFile, mRent, mBook, mMember, mQuery, mEnv });
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        // 같은 종류 창이 이미 떠 있으면 새로 열지 않고 활성화만
        void Open(Form f)
        {
            foreach (var c in MdiChildren)
                if (c.GetType() == f.GetType()) { c.Activate(); f.Dispose(); return; }
            f.MdiParent = this;
            f.Show();
        }
    }
}
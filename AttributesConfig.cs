namespace BookLendingSystem
{
    internal static class StaffAttributes
    {
        public static readonly string Staff = "职员信息";
        public static readonly string StaffId = "职员编号";
        public static readonly string StaffName = "用户名";
        public static readonly string Password = "密码";
        public static readonly string ChrName = "姓名";
        public static readonly string Gender = "性别";
        public static readonly string HireDate = "入职时间";
        public static readonly string Salary = "工资";
        public static readonly string Administrator = "权限";
    }

    internal static class UserAttributes
    {
        public static readonly string User = "用户信息";
        public static readonly string UserId = "用户编号";
        public static readonly string UserName = "用户名";
        public static readonly string Password = "密码";
        public static readonly string ChrName = "姓名";
        public static readonly string Gender = "性别";
        public static readonly string RegistrationDate = "注册时间";
        public static readonly string NumberOfBooksAvailableBorrowing = "可借书总数";
        public static readonly string NumberOfBooksBorrowing = "已借书数量";
        public static readonly string NumberOfOverdue = "逾期次数";
    }

    internal static class BookAttributes
    {
        public static readonly string Book = "图书信息";
        public static readonly string BookId = "图书编号";
        public static readonly string Isbn = "ISBN号";
        public static readonly string Title = "书名";
        public static readonly string Author = "作者";
        public static readonly string Publisher = "出版单位";
        public static readonly string PublicationDate = "出版时间";
        public static readonly string Price = "价格（元）";
        public static readonly string Borrowed = "借阅状态";
    }

    internal static class BorrowAttributes
    {
        public static readonly string Borrow = "借阅信息";
        public static readonly string BorrowId = "借阅编号";
        public static readonly string StaffId = "职员编号";
        public static readonly string UserId = "用户编号";
        public static readonly string BookId = "图书编号";
        public static readonly string BorrowDate = "借阅时间";
        public static readonly string ReturnDate = "规定最迟归还时间";
        public static readonly string BorrowedDate = "已借阅时间（日）";
        public static readonly string Overdue = "逾期状态";
        public static readonly string OverdueDate = "逾期时间（日）";
        public static readonly string OverdueCost = "逾期费用（元）";
    }
}

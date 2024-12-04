using System;
using System.Globalization;

namespace BookLendingSystem
{
    internal class Config
    {
        public static string InitialStaffId() // 初始化职员编号
        {
            return GenerateId.GenerateStaffId();
        }

        public static string InitialUserId() // 初始化用户编号
        {
            return GenerateId.GenerateUserId();
        }

        public static string InitialBookId() // 初始化图书编号
        {
            return GenerateId.GenerateBookId();
        }

        public static string InitialBorrowId() // 初始化借阅编号
        {
            return GenerateId.GenerateBorrowId();
        }

        public static string InitialPassword() // 初始化密码
        {
            return "123456";
            //string passwordStr = "123456";
            //return Encrypt.MD5Hash(passwordStr);
        }

        public static string ChangePassword(string passwordStr) // 修改密码
        {
            return Encrypt.MD5Hash(passwordStr);
        }

        public static string InitialDate() // 初始化日期
        {
            DateTime currentTime = DateTime.Now; // 获取系统当前时间
            return currentTime.ToString("yyyy-MM-dd");
        }

        public static string InitialAdministrator() // 初始化权限
        {
            return "0";
        }

        public static int InitialNumberOfBooksAvailableBorrowing() // 初始化可借书总数
        {
            return 10;
        }

        public static int InitialNumberOfBooksBorrowing() // 初始化已借书数量
        {
            return 0;
        }

        public static int InitialNumberOfOverdue() // 初始化逾期次数
        {
            return 0;
        }

        public static int InitialBorrowed() // 初始化图书借阅状态
        {
            return 0;
        }

        public static string InitialReturnDate() // 初始化还书时间
        {
            DateTime currentTime = DateTime.Now; // 获取系统当前时间
            DateTime returnTime = currentTime.AddDays(30); // 天数加30
            return returnTime.ToString("yyyy-MM-dd"); // 格式控制
        }

        public static string ExtensionReturnDate(string returnDate, int extensionDays) // 设置延期时间
        {
            DateTime date = DateTime.ParseExact(returnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture); // 将字符串转为日期对象
            date = date.AddDays(extensionDays); // 使用AddDays添加指定的天数
            return date.ToString("yyyy-MM-dd"); // 将日期对象转换回字符串并返回
        }

        public static int BorrowedDate(string borrowDate) // 设置已借阅时间
        {
            DateTime date = DateTime.ParseExact(borrowDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime currentTime = DateTime.Now; // 获取系统当前时间
            TimeSpan diff = currentTime - date; // 计算时间差值
            if (diff.Days <= 0)
                return 0;
            else
                return diff.Days;
        }

        public static int InitialOverdue() // 初始化逾期状态
        {
            return 0;
        }

        public static int Overdue(string returnDate) // 获取逾期状态
        {
            DateTime date = DateTime.ParseExact(returnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime currentTime = DateTime.Now; // 获取系统当前时间
            TimeSpan diff = currentTime - date; // 计算时间差值
            if (diff.Days <= 0)
                return 0;
            else
                return 1;
        }

        public static int InitialOverdueDate() // 初始化逾期时间
        {
            return 0;
        }

        public static int OverdueDate(string returnDate) // 获取逾期时间
        {
            DateTime date = DateTime.ParseExact(returnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime currentTime = DateTime.Now; // 获取系统当前时间
            TimeSpan diff = currentTime - date; // 计算时间差值
            if (diff.Days <= 0)
                return 0;
            else
                return diff.Days;
        }

        public static double InitialOverdueCost() // 初始化逾期费用
        {
            return 0.00;
        }

        public static double OverdueCost(int overdueDate) // 获取逾期费用
        {
            double cost = 0.1 * overdueDate;
            return Math.Round(cost, 2);
        }

        public static bool IsDate(string attribute) // 是日期信息
        {
            if ((attribute == "hireDate") || (attribute == "registrationDate") || (attribute == "publicationDate")
                || (attribute == "borrowDate") || (attribute == "returnDate") || (attribute == StaffAttributes.HireDate)
                || (attribute == UserAttributes.RegistrationDate) || (attribute == BookAttributes.PublicationDate)
                || (attribute == BorrowAttributes.BorrowDate) || (attribute == BorrowAttributes.ReturnDate))
                return true;
            else
                return false;
        }
        
        public static bool Editable(int flag, string tableName, string attributes) // 编辑权限
        {
            if (flag == 0) // 用户
            {
                if (tableName == "user")
                {
                    if (attributes == "userId")
                        return false;
                    else if (attributes == "userName")
                        return false;
                    else if (attributes == "registrationDate")
                        return false;
                    else if (attributes == "numberOfBooksAvailableBorrowing")
                        return false;
                    else if (attributes == "numberOfBooksBorrowing")
                        return false;
                    else if (attributes == "numberOfOverdue")
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            else if (flag == 1) // 职员
            {
                if (tableName == "staff")
                {
                    if (attributes == "staffId")
                        return false;
                    else if (attributes == "staffName")
                        return false;
                    else if (attributes == "hireDate")
                        return false;
                    else if (attributes == "salary")
                        return false;
                    else if (attributes == "administrator")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "user")
                {
                    if (attributes == "userId")
                        return false;
                    else if (attributes == "userName")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "book")
                {
                    if (attributes == "bookId")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "borrow")
                {
                    if (attributes == "borrowId")
                        return false;
                    else
                        return true;
                }
            }
            else if (flag == 2) // 管理员
            {
                if (tableName == "staff")
                {
                    if (attributes == "staffId")
                        return false;
                    else if (attributes == "staffName")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "user")
                {
                    if (attributes == "userId")
                        return false;
                    else if (attributes == "userName")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "book")
                {
                    if (attributes == "bookId")
                        return false;
                    else
                        return true;
                }
                else if (tableName == "borrow")
                {
                    if (attributes == "borrowId")
                        return false;
                    else
                        return true;
                }
            }
            return false;
        }
    }
}

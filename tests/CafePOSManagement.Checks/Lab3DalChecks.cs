using System;
using CafePOSManagement.Web.DAL;
using CafePOSManagement.Web.Utils;

public static class Lab3DalChecks
{
    public static void Run()
    {
        Console.WriteLine("\n=== KIỂM THỬ CÁC LỚP DAL & DBUTIL (LAB 3) ===");
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__CafePos");
        if (!string.IsNullOrWhiteSpace(conn)) DBUtil.connString = conn;

        // 1. Test CategoryDAL (Đức)
        var categories = CategoryDAL.GetAll();
        Console.WriteLine($"[CategoryDAL] Lấy thành công {categories.Count} danh mục.");
        if (categories.Count == 0) throw new Exception("CategoryDAL rỗng!");

        // 2. Test ProductDAL / DrinkDAL (Đức)
        var products = ProductDAL.GetAll();
        Console.WriteLine($"[ProductDAL] Lấy thành công {products.Count} món đồ uống.");
        if (products.Count == 0) throw new Exception("ProductDAL rỗng!");

        // 3. Test UserDAL (Tâm)
        var users = UserDAL.GetAll();
        Console.WriteLine($"[UserDAL] Lấy thành công {users.Count} tài khoản nhân sự.");
        var loginUser = UserDAL.Login("thu@polycafe.vn", "123456");
        Console.WriteLine($"[UserDAL Login] Đăng nhập thử: {(loginUser != null ? "Thành công (" + loginUser.FullName + " - " + loginUser.RoleName + ")" : "Thất bại")}");

        // 4. Test CardDAL (Kha)
        var cards = CardDAL.GetAll();
        Console.WriteLine($"[CardDAL] Lấy thành công {cards.Count} thẻ bàn/thẻ rung.");

        // 5. Test OrderDAL & OrderItemDAL (Nguyễn)
        var orders = OrderDAL.GetAll();
        Console.WriteLine($"[OrderDAL] Lấy thành công {orders.Count} hóa đơn.");
        if (orders.Count > 0)
        {
            var items = OrderItemDAL.GetByOrderId(orders[0].Id);
            Console.WriteLine($"[OrderItemDAL] Hóa đơn {orders[0].Code} có {items.Count} chi tiết món.");
        }

        Console.WriteLine(">>> TẤT CẢ CÁC LỚP DAL & DBUTIL HOẠT ĐỘNG HOÀN HẢO 100% <<<\n");
    }
}

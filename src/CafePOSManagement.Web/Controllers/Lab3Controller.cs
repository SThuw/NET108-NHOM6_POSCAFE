using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CafePOSManagement.Web.DAL;
using Microsoft.AspNetCore.Authorization;

namespace CafePOSManagement.Web.Controllers;

public class Lab3ViewModel
{
    public List<CategoryDAL> Categories { get; set; } = new();
    public List<ProductDAL> Products { get; set; } = new();
    public List<UserDAL> Users { get; set; } = new();
    public List<CardDAL> Cards { get; set; } = new();
    public List<OrderDAL> Orders { get; set; } = new();
    public List<AiSessionDTO> AiSessions { get; set; } = new();
}

[Authorize(Policy = "ManagerOnly")]
public class Lab3Controller : Controller
{
    [HttpGet]
    public IActionResult Index(int aiPage = 1)
    {
        ViewData["Title"] = "Quản lý Hệ thống · PolyCafe";
        var model = new Lab3ViewModel();
        try
        {
            model.Categories = CategoryDAL.GetAll();
            model.Products = ProductDAL.GetAll();
            model.Users = UserDAL.GetAll();
            model.Cards = CardDAL.GetAll();
            model.Orders = OrderDAL.GetAll();
            
            // Lấy TOÀN BỘ sessions (để View tự phân trang)
            var allSessions = AiStateStoreDAL.GetAllSessions();
            
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)allSessions.Count / pageSize);
            
            ViewBag.AiPage = aiPage;
            ViewBag.TotalAiPages = totalPages > 0 ? totalPages : 1;
            
            // Chỉ lấy 5 cái cho trang hiện tại
            model.AiSessions = allSessions.Skip((aiPage - 1) * pageSize).Take(pageSize).ToList();
            
            // Gán tổng số phiên thực tế để View hiển thị đúng
            ViewBag.TotalAiSessions = allSessions.Count;
        }
        catch (Exception ex)
        {
            ViewBag.DbError = ex.Message;
        }
        return View(model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
public IActionResult UpdateOrderStatus(long id, string newStatus)
{
    try
    {
        // Lấy đơn hàng hiện tại từ DB
        var order = OrderDAL.GetById(id);
        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

        // Cập nhật trạng thái mới
        order.Status = newStatus;
        
        // Gọi hàm Update có sẵn trong DAL (cập nhật Status và Note)
        OrderDAL.Update(order);

        return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi: " + ex.Message });
    }
}
        [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SendAiMessage(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Vui lòng nhập câu hỏi." });

            string sessionId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "guest";
            
            // 1. Tạo phiên chat
            var sessionSql = "INSERT INTO AiSessions (Id, UserId, Title, IsActive, CreatedAt) VALUES (@1, @2, @3, 1, SYSDATETIMEOFFSET())";
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(sessionSql, new List<object> { sessionId, userId, "Tư vấn: " + message });

            // 2. Lưu tin nhắn user
            var userMsgSql = "INSERT INTO AiMessages (SessionId, Role, Content, CreatedAt) VALUES (@1, 'user', @2, SYSDATETIMEOFFSET())";
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(userMsgSql, new List<object> { sessionId, message });

            // 3. Xử lý logic AI trả lời thông minh & an toàn (Null-safe)
            var products = ProductDAL.GetAll();
            string reply = "";
            string lowerMsg = message.ToLower().Trim();

            // Nhóm chào hỏi
            if (lowerMsg.Contains("chào") || lowerMsg.Contains("chao") || lowerMsg.Contains("hello") || lowerMsg.Contains("hi") || lowerMsg.Contains("hí") || lowerMsg.Contains("hế lô"))
            {
                reply = "Chào bạn! Mình là Trợ lý AI PolyCafe. Bạn muốn tìm đồ uống thanh mát, cà phê đậm đà hay các món bánh ăn kèm?";
            }
            // Nhóm hỏi BÁNH NGỌT / ĐỒ ĂN
            else if (lowerMsg.Contains("bánh") || lowerMsg.Contains("banh") || lowerMsg.Contains("đồ ăn") || lowerMsg.Contains("do an") || lowerMsg.Contains("thức ăn") || lowerMsg.Contains("ăn kèm"))
            {
                var pastryItems = products.Where(p => 
                    (p.CategoryName ?? "").ToLower().Contains("bánh") || 
                    (p.CategoryName ?? "").ToLower().Contains("ăn") || 
                    (p.Name ?? "").ToLower().Contains("bánh") || 
                    (p.Name ?? "").ToLower().Contains("croissant") ||
                    (p.Name ?? "").ToLower().Contains("quy")).Take(3).ToList();

                if (pastryItems.Any())
                    reply = "Dạ, quán có các món bánh thơm ngon ăn kèm sau đây ạ:\n" + string.Join("\n", pastryItems.Select(x => $"- {x.Name} ({x.Price:N0} đ): {x.Description}"));
                else
                    reply = "Dạ, hiện tại thực đơn bánh hôm nay đang cập nhật, bạn có muốn thử các món đồ uống của quán không?";
            }
            // Nhóm hỏi ĐỒ UỐNG / NƯỚC UỐNG (Lọc bỏ triệt để các món bánh)
            else if (lowerMsg.Contains("đồ uống") || lowerMsg.Contains("do uong") || lowerMsg.Contains("thức uống") || lowerMsg.Contains("thuc uong") || 
                     lowerMsg.Contains("nước") || lowerMsg.Contains("nuoc") || lowerMsg.Contains("uống gì") || lowerMsg.Contains("uống"))
            {
                var drinkItems = products.Where(p => 
                    !(p.CategoryName ?? "").ToLower().Contains("bánh") && 
                    !(p.Name ?? "").ToLower().Contains("bánh") && 
                    !(p.Name ?? "").ToLower().Contains("croissant") &&
                    !(p.Name ?? "").ToLower().Contains("quy")).ToList();

                if (lowerMsg.Contains("chua") || lowerMsg.Contains("ngọt") || lowerMsg.Contains("ngot") || lowerMsg.Contains("mát") || lowerMsg.Contains("mat") || lowerMsg.Contains("trà") || lowerMsg.Contains("tra"))
                {
                    var specificDrinks = drinkItems.Where(p => 
                        (p.Name ?? "").ToLower().Contains("trà") || 
                        (p.Name ?? "").ToLower().Contains("cam") || 
                        (p.Name ?? "").ToLower().Contains("chanh") || 
                        (p.Description ?? "").ToLower().Contains("chua") || 
                        (p.Description ?? "").ToLower().Contains("mát")).Take(3).ToList();
                    
                    if (specificDrinks.Any()) drinkItems = specificDrinks;
                }
                else if (lowerMsg.Contains("cà phê") || lowerMsg.Contains("ca phe") || lowerMsg.Contains("cafe") || lowerMsg.Contains("coffee") || lowerMsg.Contains("đắng") || lowerMsg.Contains("tỉnh"))
                {
                    var coffeeDrinks = drinkItems.Where(p => 
                        (p.CategoryName ?? "").ToLower().Contains("cà phê") || 
                        (p.Name ?? "").ToLower().Contains("cà phê") || 
                        (p.Name ?? "").ToLower().Contains("coffee")).Take(3).ToList();

                    if (coffeeDrinks.Any()) drinkItems = coffeeDrinks;
                }

                reply = "Dạ, các món đồ uống bán chạy được yêu thích nhất tại PolyCafe gồm có:\n" + 
                        string.Join("\n", drinkItems.Take(3).Select(x => $"- {x.Name} ({x.Price:N0} đ): {x.Description}"));
            }
            // Tìm kiếm linh hoạt theo từ khóa chấm điểm (Keyword Scoring)
            else
            {
                var stopWords = new HashSet<string> { "tôi", "mình", "muốn", "món", "nào", "cho", "có", "gì", "và", "là", "của", "những", "các", "một", "được", "hãy", "giúp", "với", "thì", "mà", "đang", "bạn", "ơi", "nhé", "ạ", "vâng", "dạ", "tao" };
                var keywords = lowerMsg.Split(new[] { ' ', ',', '.', '?', '!', '-', '/', '\\', '"', '\'', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 1 && !stopWords.Contains(w)).Distinct().ToList();

                if (keywords.Any())
                {
                    var scoredProducts = products.Select(p => {
                        int score = 0;
                        string pName = (p.Name ?? "").ToLower();
                        string pCat = (p.CategoryName ?? "").ToLower();
                        string pDesc = (p.Description ?? "").ToLower();

                        foreach (var kw in keywords) {
                            if (pName.Contains(kw)) score += 3;
                            if (pCat.Contains(kw)) score += 2;
                            if (pDesc.Contains(kw)) score += 1;
                        }
                        return new { Product = p, Score = score };
                    }).Where(x => x.Score > 0).OrderByDescending(x => x.Score).Take(3).ToList();

                    if (scoredProducts.Any())
                    {
                        reply = "Dạ, dựa trên yêu cầu của bạn, em xin gợi ý các món sau:\n" + 
                                string.Join("\n", scoredProducts.Select(x => $"- {x.Product.Name} ({x.Product.Price:N0} đ): {x.Product.Description}"));
                    }
                    else
                    {
                        reply = "Dạ, hiện tại em chưa tìm thấy món thật sự khớp. Quán đang có các món đồ uống rất được yêu thích sau:\n" + 
                                string.Join("\n", products.Where(p => !(p.CategoryName ?? "").ToLower().Contains("bánh")).Take(3).Select(p => $"- {p.Name} ({p.Price:N0} đ)"));
                    }
                }
                else
                {
                    reply = "Chào bạn! Mình là Trợ lý AI của PolyCafe. Bạn muốn tìm đồ uống thanh mát, đậm vị cà phê hay bánh ngọt ăn kèm?";
                }
            }

            // 4. LƯU TRẠNG THÁI NGỮ CẢNH (STATE STORE)
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery("DELETE FROM AiStateStores WHERE SessionId = @1", new List<object> { sessionId });

            string intent = "recommend_drink";
            string preference = "drinks";

            if (lowerMsg.Contains("bánh") || lowerMsg.Contains("banh") || lowerMsg.Contains("ăn")) { intent = "recommend_pastry"; preference = "pastry"; }
            else if (lowerMsg.Contains("cà phê") || lowerMsg.Contains("ca phe") || lowerMsg.Contains("cafe")) { preference = "coffee"; }
            else if (lowerMsg.Contains("chua") || lowerMsg.Contains("mát")) { preference = "refreshing"; }
            else if (lowerMsg.Contains("combo")) { preference = "combo"; }

            var stateSql = "INSERT INTO AiStateStores (SessionId, StateKey, StateValue, UpdatedAt) VALUES (@1, @2, @3, SYSDATETIMEOFFSET())";
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(stateSql, new List<object> { sessionId, "intent", intent });
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(stateSql, new List<object> { sessionId, "user_preference", preference });
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(stateSql, new List<object> { sessionId, "last_message_length", message.Length.ToString() });

            // 5. Lưu tin nhắn AI trả lời
            var aiMsgSql = "INSERT INTO AiMessages (SessionId, Role, Content, CreatedAt) VALUES (@1, 'assistant', @2, SYSDATETIMEOFFSET())";
            CafePOSManagement.Web.Utils.DBUtil.executeNonQuery(aiMsgSql, new List<object> { sessionId, reply });

            return Json(new { success = true, reply = reply });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }
[HttpPost]
[IgnoreAntiforgeryToken]
public IActionResult DeleteAiHistory()
{
    try
    {
        // Xóa tất cả tin nhắn (AiMessages) trước do có khóa ngoại
        CafePOSManagement.Web.Utils.DBUtil.executeNonQuery("DELETE FROM AiMessages");
        // Xóa tất cả trạng thái ngữ cảnh (AiStateStores)
        CafePOSManagement.Web.Utils.DBUtil.executeNonQuery("DELETE FROM AiStateStores");
        // Cuối cùng xóa các phiên chat (AiSessions)
        CafePOSManagement.Web.Utils.DBUtil.executeNonQuery("DELETE FROM AiSessions");

        return Json(new { success = true, message = "Đã xóa toàn bộ lịch sử trò chuyện." });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
    }
}
    [HttpGet]
    public IActionResult GetOrderDetail(long id)
    {
        try
        {
            var order = OrderDAL.GetById(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });
            }

            var items = OrderItemDAL.GetByOrderId(id);

            return Json(new
            {
                success = true,
                order = new
                {
                    id = order.Id,
                    code = order.Code,
                    orderType = order.OrderType,
                    createdAt = order.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    paymentMethod = order.PaymentMethod,
                    status = order.Status,
                    employeeName = order.EmployeeName,
                    note = string.IsNullOrEmpty(order.Note) ? "Không có" : order.Note,
                    subtotal = order.Subtotal,
                    discountAmount = order.DiscountAmount,
                    taxAmount = order.TaxAmount,
                    total = order.Total
                },
                items = items.Select(i => new
                {
                    productName = i.ProductName,
                    unitPrice = i.UnitPrice,
                    quantity = i.Quantity,
                    lineTotal = i.LineTotal
                })
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi lấy chi tiết: " + ex.Message });
        }
    }
}

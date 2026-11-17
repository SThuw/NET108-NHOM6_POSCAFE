using System;
using Microsoft.AspNetCore.Mvc;
using CafePOSManagement.Web.DAL;
using CafePOSManagement.Web.Utils;
using Microsoft.AspNetCore.Authorization;

namespace CafePOSManagement.Web.Controllers;

[Authorize(Policy = "ManagerOnly")]
public class CardsController : Controller
{
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Create(string? id, string code, string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ Mã và Tên thẻ!" });

            code = code.Trim();
            name = name.Trim();

            // Ràng buộc: Kiểm tra mã thẻ đã tồn tại trong CSDL chưa (kể cả thẻ đã xóa mềm)
            string checkSql = "SELECT COUNT(1) FROM Cards WHERE UPPER(Code) = UPPER(@1)";
            var countObj = DBUtil.executeScalar(checkSql, new List<object> { code });
            int existsCount = countObj != null && countObj != DBNull.Value ? Convert.ToInt32(countObj) : 0;
            if (existsCount > 0)
            {
                return Json(new { success = false, message = $"Mã thẻ '{code}' đã tồn tại trong hệ thống!" });
            }

            // Tự động sinh Id duy nhất (VD: CARD_GUID)
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }

            var card = new CardDAL { Id = id, Code = code, Name = name, Status = false, Active = true };
            CardDAL.Insert(card);
            return Json(new { success = true, message = "Thêm thẻ thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Update(string id, string code, string name, bool status)
    {
        try
        {
            var card = CardDAL.GetById(id);
            if (card == null) return Json(new { success = false, message = "Không tìm thấy thẻ." });
            
            // Ràng buộc: Mã thẻ không được trùng với thẻ khác
            var existing = CardDAL.GetAll().FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && c.Id != id);
            if (existing != null) return Json(new { success = false, message = "Mã thẻ này đã được dùng cho thẻ khác!" });

            card.Code = code;
            card.Name = name;
            card.Status = status;
            CardDAL.Update(card);
            return Json(new { success = true, message = "Cập nhật thẻ thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Delete(string id)
    {
        try
        {
            var card = CardDAL.GetById(id);
            // Ràng buộc: Không cho xóa thẻ đang phục vụ
            if (card != null && card.Status)
                return Json(new { success = false, message = "Không thể xóa thẻ đang phục vụ khách!" });

            CardDAL.Delete(id);
            return Json(new { success = true, message = "Đã xóa thẻ!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Hóa đơn / Đơn hàng (Bài 4 - LAB 3)
/// Phụ trách: Nguyễn Trí Nguyễn (TB02277)
/// </summary>
public class OrderDAL
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty; // OrderNumber (#001)
    public string OrderType { get; set; } = string.Empty; // Tại bàn, Mang đi, Giao hàng
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Hoàn thành";
    public DateTime CreatedAt { get; set; }
    public string Note { get; set; } = string.Empty;

    public static List<OrderDAL> GetAll()
    {
        var list = new List<OrderDAL>();
        string sql = "SELECT Id, OrderNumber, OrderType, Subtotal, DiscountAmount, TaxAmount, TotalAmount AS Total, PaymentMethod, Status, CreatedAt, Note FROM Orders ORDER BY Id DESC";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new OrderDAL
            {
                Id = Convert.ToInt64(row["Id"]),
                Code = row["OrderNumber"].ToString() ?? "",
                OrderType = row["OrderType"].ToString() ?? "",
                Subtotal = Convert.ToDecimal(row["Subtotal"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                Total = Convert.ToDecimal(row["Total"]),
                PaymentMethod = row["PaymentMethod"].ToString() ?? "",
                Status = row["Status"].ToString() ?? "",
                CreatedAt = row["CreatedAt"] is DateTimeOffset dto ? dto.DateTime : Convert.ToDateTime(row["CreatedAt"]),
                Note = row["Note"]?.ToString() ?? ""
            });
        }
        return list;
    }

    public static OrderDAL? GetById(long id)
    {
        string sql = "SELECT Id, OrderNumber, OrderType, Subtotal, DiscountAmount, TaxAmount, TotalAmount AS Total, PaymentMethod, Status, CreatedAt, Note FROM Orders WHERE Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new OrderDAL
        {
            Id = Convert.ToInt64(row["Id"]),
            Code = row["OrderNumber"].ToString() ?? "",
            OrderType = row["OrderType"].ToString() ?? "",
            Subtotal = Convert.ToDecimal(row["Subtotal"]),
            DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
            TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
            Total = Convert.ToDecimal(row["Total"]),
            PaymentMethod = row["PaymentMethod"].ToString() ?? "",
            Status = row["Status"].ToString() ?? "",
            CreatedAt = row["CreatedAt"] is DateTimeOffset dto ? dto.DateTime : Convert.ToDateTime(row["CreatedAt"]),
            Note = row["Note"]?.ToString() ?? ""
        };
    }

    public static int Insert(OrderDAL entity)
    {
        string sql = @"
            INSERT INTO Orders (RequestId, OrderNumber, OrderType, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaymentMethod, Status, CreatedAt, Note)
            VALUES (NEWID(), @1, @2, @3, @4, @5, @6, @7, @8, SYSDATETIMEOFFSET(), @9)";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Code, entity.OrderType, entity.Subtotal, entity.DiscountAmount,
            entity.TaxAmount, entity.Total, entity.PaymentMethod, entity.Status, entity.Note
        });
    }

    public static int Update(OrderDAL entity)
    {
        string sql = @"
            UPDATE Orders 
            SET Status = @1, Note = @2
            WHERE Id = @3";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Status, entity.Note, entity.Id
        });
    }

    public static int Delete(long id)
    {
        string sql = "DELETE FROM Orders WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

// Bí danh BillDAL để khớp 100% tài liệu đề bài mẫu LAB 3
public class BillDAL : OrderDAL { }

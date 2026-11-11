using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Chi tiết đơn hàng (Bài 4 - LAB 3)
/// Phụ trách: Nguyễn Trí Nguyễn (TB02277)
/// </summary>
public class OrderItemDAL
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public static List<OrderItemDAL> GetAll()
    {
        var list = new List<OrderItemDAL>();
        string sql = "SELECT Id, OrderId, ProductId, ProductName, UnitPrice, Quantity, LineTotal FROM OrderItems ORDER BY Id DESC";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new OrderItemDAL
            {
                Id = Convert.ToInt64(row["Id"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                ProductId = row["ProductId"].ToString() ?? "",
                ProductName = row["ProductName"].ToString() ?? "",
                UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                LineTotal = Convert.ToDecimal(row["LineTotal"])
            });
        }
        return list;
    }

    public static List<OrderItemDAL> GetByOrderId(long orderId)
    {
        var list = new List<OrderItemDAL>();
        string sql = "SELECT Id, OrderId, ProductId, ProductName, UnitPrice, Quantity, LineTotal FROM OrderItems WHERE OrderId = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { orderId });
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new OrderItemDAL
            {
                Id = Convert.ToInt64(row["Id"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                ProductId = row["ProductId"].ToString() ?? "",
                ProductName = row["ProductName"].ToString() ?? "",
                UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                LineTotal = Convert.ToDecimal(row["LineTotal"])
            });
        }
        return list;
    }

    public static OrderItemDAL? GetById(long id)
    {
        string sql = "SELECT Id, OrderId, ProductId, ProductName, UnitPrice, Quantity, LineTotal FROM OrderItems WHERE Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new OrderItemDAL
        {
            Id = Convert.ToInt64(row["Id"]),
            OrderId = Convert.ToInt64(row["OrderId"]),
            ProductId = row["ProductId"].ToString() ?? "",
            ProductName = row["ProductName"].ToString() ?? "",
            UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
            Quantity = Convert.ToInt32(row["Quantity"]),
            LineTotal = Convert.ToDecimal(row["LineTotal"])
        };
    }

    public static int Insert(OrderItemDAL entity)
    {
        string sql = @"
            INSERT INTO OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
            VALUES (@1, @2, @3, @4, @5)";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.OrderId, entity.ProductId, entity.ProductName, entity.UnitPrice, entity.Quantity
        });
    }

    public static int Delete(long id)
    {
        string sql = "DELETE FROM OrderItems WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

// Bí danh BillDetailDAL để khớp 100% tài liệu đề bài mẫu LAB 3
public class BillDetailDAL : OrderItemDAL { }

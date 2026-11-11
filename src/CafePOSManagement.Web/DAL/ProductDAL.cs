using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Sản phẩm / Đồ uống (Bài 4 - LAB 3)
/// Phụ trách: Ninh Văn Đức (TB02291)
/// </summary>
public class ProductDAL
{
    public string Id { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public static List<ProductDAL> GetAll()
    {
        var list = new List<ProductDAL>();
        string sql = @"
            SELECT p.Id, p.CategoryId, c.Name AS CategoryName, p.Name, p.Price, p.OriginalPrice, 
                   p.ImageUrl, p.Description, p.IsAvailable 
            FROM Products p
            JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.IsAvailable = 1
            ORDER BY c.DisplayOrder, p.Name";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new ProductDAL
            {
                Id = row["Id"].ToString() ?? "",
                CategoryId = row["CategoryId"].ToString() ?? "",
                CategoryName = row["CategoryName"].ToString() ?? "",
                Name = row["Name"].ToString() ?? "",
                Price = Convert.ToDecimal(row["Price"]),
                OriginalPrice = row["OriginalPrice"] == DBNull.Value ? null : Convert.ToDecimal(row["OriginalPrice"]),
                ImageUrl = row["ImageUrl"].ToString() ?? "",
                Description = row["Description"]?.ToString() ?? "",
                Active = Convert.ToBoolean(row["IsAvailable"])
            });
        }
        return list;
    }

    public static ProductDAL? GetById(string id)
    {
        string sql = @"
            SELECT p.Id, p.CategoryId, c.Name AS CategoryName, p.Name, p.Price, p.OriginalPrice, 
                   p.ImageUrl, p.Description, p.IsAvailable 
            FROM Products p
            JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new ProductDAL
        {
            Id = row["Id"].ToString() ?? "",
            CategoryId = row["CategoryId"].ToString() ?? "",
            CategoryName = row["CategoryName"].ToString() ?? "",
            Name = row["Name"].ToString() ?? "",
            Price = Convert.ToDecimal(row["Price"]),
            OriginalPrice = row["OriginalPrice"] == DBNull.Value ? null : Convert.ToDecimal(row["OriginalPrice"]),
            ImageUrl = row["ImageUrl"].ToString() ?? "",
            Description = row["Description"]?.ToString() ?? "",
            Active = Convert.ToBoolean(row["IsAvailable"])
        };
    }

    public static int Insert(ProductDAL entity)
    {
        string sql = @"
            INSERT INTO Products (Id, Name, CategoryId, Price, OriginalPrice, ImageUrl, Description, IsAvailable, CreatedAt)
            VALUES (@1, @2, @3, @4, @5, @6, @7, @8, SYSDATETIMEOFFSET())";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Id, entity.Name, entity.CategoryId, entity.Price,
            (object?)entity.OriginalPrice ?? DBNull.Value,
            entity.ImageUrl, entity.Description, entity.Active ? 1 : 0
        });
    }

    public static int Update(ProductDAL entity)
    {
        string sql = @"
            UPDATE Products 
            SET Name = @1, CategoryId = @2, Price = @3, OriginalPrice = @4, ImageUrl = @5, Description = @6, IsAvailable = @7
            WHERE Id = @8";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Name, entity.CategoryId, entity.Price,
            (object?)entity.OriginalPrice ?? DBNull.Value,
            entity.ImageUrl, entity.Description, entity.Active ? 1 : 0, entity.Id
        });
    }

    public static int Delete(string id)
    {
        string sql = "UPDATE Products SET IsAvailable = 0 WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

// Bí danh DrinkDAL để khớp 100% tài liệu đề bài mẫu LAB 3
public class DrinkDAL : ProductDAL { }

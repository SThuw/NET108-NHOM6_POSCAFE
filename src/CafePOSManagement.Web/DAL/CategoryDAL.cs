using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Danh mục (Bài 4 - LAB 3)
/// Phụ trách: Ninh Văn Đức (TB02291)
/// </summary>
public class CategoryDAL
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool Active { get; set; } = true;

    public static List<CategoryDAL> GetAll()
    {
        var list = new List<CategoryDAL>();
        string sql = "SELECT Id, Name, Code, DisplayOrder, IsActive FROM Categories WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new CategoryDAL
            {
                Id = row["Id"].ToString() ?? "",
                Name = row["Name"].ToString() ?? "",
                Code = row["Code"].ToString() ?? "",
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                Active = Convert.ToBoolean(row["IsActive"])
            });
        }
        return list;
    }

    public static CategoryDAL? GetById(string id)
    {
        string sql = "SELECT Id, Name, Code, DisplayOrder, IsActive FROM Categories WHERE Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new CategoryDAL
        {
            Id = row["Id"].ToString() ?? "",
            Name = row["Name"].ToString() ?? "",
            Code = row["Code"].ToString() ?? "",
            DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
            Active = Convert.ToBoolean(row["IsActive"])
        };
    }

    public static int Insert(CategoryDAL entity)
    {
        string sql = "INSERT INTO Categories (Id, Name, Code, DisplayOrder, IsActive, CreatedAt) VALUES (@1, @2, @3, @4, @5, SYSDATETIMEOFFSET())";
        return DBUtil.executeNonQuery(sql, new List<object> { entity.Id, entity.Name, entity.Code, entity.DisplayOrder, entity.Active ? 1 : 0 });
    }

    public static int Update(CategoryDAL entity)
    {
        string sql = "UPDATE Categories SET Name = @1, Code = @2, DisplayOrder = @3, IsActive = @4 WHERE Id = @5";
        return DBUtil.executeNonQuery(sql, new List<object> { entity.Name, entity.Code, entity.DisplayOrder, entity.Active ? 1 : 0, entity.Id });
    }

    public static int Delete(string id)
    {
        string sql = "UPDATE Categories SET IsActive = 0 WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

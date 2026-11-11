using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Thẻ bàn / Thẻ rung (Bài 4 - LAB 3)
/// Phụ trách: Trần Thị Tuyết Kha (TB02214)
/// </summary>
public class CardDAL
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } // 0: Trống, 1: Đang phục vụ
    public bool Active { get; set; } = true;

    public string StatusText => Status ? "Đang phục vụ" : "Bàn trống";

    public static List<CardDAL> GetAll()
    {
        var list = new List<CardDAL>();
        string sql = "SELECT Id, Code, Name, IsOccupied, IsActive FROM Cards WHERE IsActive = 1 ORDER BY Code";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new CardDAL
            {
                Id = row["Id"].ToString() ?? "",
                Code = row["Code"].ToString() ?? "",
                Name = row["Name"].ToString() ?? "",
                Status = Convert.ToBoolean(row["IsOccupied"]),
                Active = Convert.ToBoolean(row["IsActive"])
            });
        }
        return list;
    }

    public static CardDAL? GetById(string id)
    {
        string sql = "SELECT Id, Code, Name, IsOccupied, IsActive FROM Cards WHERE Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new CardDAL
        {
            Id = row["Id"].ToString() ?? "",
            Code = row["Code"].ToString() ?? "",
            Name = row["Name"].ToString() ?? "",
            Status = Convert.ToBoolean(row["IsOccupied"]),
            Active = Convert.ToBoolean(row["IsActive"])
        };
    }

    public static int Insert(CardDAL entity)
    {
        string sql = @"
            INSERT INTO Cards (Id, Code, Name, IsOccupied, IsActive, CreatedAt)
            VALUES (@1, @2, @3, @4, @5, SYSDATETIMEOFFSET())";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Id, entity.Code, entity.Name, entity.Status ? 1 : 0, entity.Active ? 1 : 0
        });
    }

    public static int Update(CardDAL entity)
    {
        string sql = @"
            UPDATE Cards 
            SET Code = @1, Name = @2, IsOccupied = @3, IsActive = @4
            WHERE Id = @5";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Code, entity.Name, entity.Status ? 1 : 0, entity.Active ? 1 : 0, entity.Id
        });
    }

    public static int Delete(string id)
    {
        string sql = "UPDATE Cards SET IsActive = 0 WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

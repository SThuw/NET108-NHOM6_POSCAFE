using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;
using Microsoft.AspNetCore.Identity;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp thực thể và truy cập dữ liệu Tài khoản / Nhân viên (Bài 4 - LAB 3)
/// Phụ trách: Nguyễn Lâm Chí Tâm (TB02234)
/// </summary>
public class UserDAL
{
    private static readonly PasswordHasher<UserDAL> PasswordHasher = new();
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool Role { get; set; } // 0 = Nhân viên, 1 = Quản lý
    public bool Active { get; set; } = true;

    public string RoleName => Role ? "Quản lý" : "Nhân viên";

    public static List<UserDAL> GetAll()
    {
        var list = new List<UserDAL>();
        string sql = "SELECT Id, Email, Password, FullName, Phone, Role, IsActive FROM Users WHERE IsActive = 1 ORDER BY FullName";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new UserDAL
            {
                Id = row["Id"].ToString() ?? "",
                Email = row["Email"].ToString() ?? "",
                Password = row["Password"].ToString() ?? "",
                FullName = row["FullName"].ToString() ?? "",
                Phone = row["Phone"].ToString() ?? "",
                Role = Convert.ToBoolean(row["Role"]),
                Active = Convert.ToBoolean(row["IsActive"])
            });
        }
        return list;
    }

    public static UserDAL? GetById(string id)
    {
        string sql = "SELECT Id, Email, Password, FullName, Phone, Role, IsActive FROM Users WHERE Id = @1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { id });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        return new UserDAL
        {
            Id = row["Id"].ToString() ?? "",
            Email = row["Email"].ToString() ?? "",
            Password = row["Password"].ToString() ?? "",
            FullName = row["FullName"].ToString() ?? "",
            Phone = row["Phone"].ToString() ?? "",
            Role = Convert.ToBoolean(row["Role"]),
            Active = Convert.ToBoolean(row["IsActive"])
        };
    }

    public static UserDAL? Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) return null;
        string sql = "SELECT Id, Email, Password, FullName, Phone, Role, IsActive FROM Users WHERE Email = @1 AND IsActive = 1";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { email });
        if (dt.Rows.Count == 0) return null;
        DataRow row = dt.Rows[0];
        var user = new UserDAL
        {
            Id = row["Id"].ToString() ?? "",
            Email = row["Email"].ToString() ?? "",
            Password = row["Password"].ToString() ?? "",
            FullName = row["FullName"].ToString() ?? "",
            Phone = row["Phone"].ToString() ?? "",
            Role = Convert.ToBoolean(row["Role"]),
            Active = Convert.ToBoolean(row["IsActive"])
        };
        var hashed = user.Password.StartsWith("AQAAAA", StringComparison.Ordinal);
        var valid = hashed
            ? PasswordHasher.VerifyHashedPassword(user, user.Password, password) != PasswordVerificationResult.Failed
            : user.Password == password; // Tương thích dữ liệu mẫu LAB 3; nâng cấp hash sau lần đăng nhập đầu.
        if (!valid) return null;
        if (!hashed)
        {
            DBUtil.executeNonQuery("UPDATE Users SET Password = @1 WHERE Id = @2 AND Password = @3",
                new List<object> { PasswordHasher.HashPassword(user, password), user.Id, user.Password });
        }
        user.Password = "";
        return user;
    }

    public static int Insert(UserDAL entity)
    {
        string sql = @"
            INSERT INTO Users (Id, Email, Password, FullName, Phone, Role, IsActive, CreatedAt)
            VALUES (@1, @2, @3, @4, @5, @6, @7, SYSDATETIMEOFFSET())";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Id, entity.Email, PasswordHasher.HashPassword(entity, entity.Password), entity.FullName, entity.Phone,
            entity.Role ? 1 : 0, entity.Active ? 1 : 0
        });
    }

    public static int Update(UserDAL entity)
    {
        string sql = @"
            UPDATE Users 
            SET Email = @1, Password = @2, FullName = @3, Phone = @4, Role = @5, IsActive = @6
            WHERE Id = @7";
        return DBUtil.executeNonQuery(sql, new List<object>
        {
            entity.Email, PasswordHasher.HashPassword(entity, entity.Password), entity.FullName, entity.Phone,
            entity.Role ? 1 : 0, entity.Active ? 1 : 0, entity.Id
        });
    }

    public static int Delete(string id)
    {
        string sql = "UPDATE Users SET IsActive = 0 WHERE Id = @1";
        return DBUtil.executeNonQuery(sql, new List<object> { id });
    }
}

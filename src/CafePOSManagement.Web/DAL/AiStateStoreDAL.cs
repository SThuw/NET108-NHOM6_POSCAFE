using System;
using System.Collections.Generic;
using System.Data;
using CafePOSManagement.Web.Utils;

namespace CafePOSManagement.Web.DAL;

/// <summary>
/// Lớp truy cập dữ liệu Cụm bảng AI State Store (Lời dặn giảng viên Lab 2)
/// Phụ trách: Nguyễn Kim Song Thư (Nhóm trưởng)
/// </summary>
public class AiSessionDTO
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Context { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class AiMessageDTO
{
    public long Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AiStateDTO
{
    public long Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string StateKey { get; set; } = string.Empty;
    public string StateValue { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public static class AiStateStoreDAL
{
    public static List<AiSessionDTO> GetAllSessions()
    {
        var list = new List<AiSessionDTO>();
        string sql = @"
            SELECT s.Id, s.UserId, u.FullName AS UserFullName, s.Title, s.Context, s.IsActive, s.CreatedAt
            FROM AiSessions s
            LEFT JOIN Users u ON s.UserId = u.Id
            WHERE s.IsActive = 1
            ORDER BY s.CreatedAt DESC";
        DataTable dt = DBUtil.queryDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new AiSessionDTO
            {
                Id = row["Id"].ToString() ?? "",
                UserId = row["UserId"]?.ToString(),
                UserFullName = row["UserFullName"]?.ToString() ?? "Khách vãng lai",
                Title = row["Title"].ToString() ?? "",
                Context = row["Context"]?.ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = row["CreatedAt"] is DateTimeOffset dto ? dto.DateTime : Convert.ToDateTime(row["CreatedAt"])
            });
        }
        return list;
    }

    public static List<AiMessageDTO> GetMessages(string sessionId)
    {
        var list = new List<AiMessageDTO>();
        string sql = "SELECT Id, SessionId, Role, Content, CreatedAt FROM AiMessages WHERE SessionId = @1 ORDER BY Id ASC";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { sessionId });
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new AiMessageDTO
            {
                Id = Convert.ToInt64(row["Id"]),
                SessionId = row["SessionId"].ToString() ?? "",
                Role = row["Role"].ToString() ?? "",
                Content = row["Content"].ToString() ?? "",
                CreatedAt = row["CreatedAt"] is DateTimeOffset dto ? dto.DateTime : Convert.ToDateTime(row["CreatedAt"])
            });
        }
        return list;
    }

    public static List<AiStateDTO> GetStates(string sessionId)
    {
        var list = new List<AiStateDTO>();
        string sql = "SELECT Id, SessionId, StateKey, StateValue, UpdatedAt FROM AiStateStores WHERE SessionId = @1 ORDER BY StateKey ASC";
        DataTable dt = DBUtil.queryDataTable(sql, new List<object> { sessionId });
        foreach (DataRow row in dt.Rows)
        {
            list.Add(new AiStateDTO
            {
                Id = Convert.ToInt64(row["Id"]),
                SessionId = row["SessionId"].ToString() ?? "",
                StateKey = row["StateKey"].ToString() ?? "",
                StateValue = row["StateValue"].ToString() ?? "",
                UpdatedAt = row["UpdatedAt"] is DateTimeOffset dto ? dto.DateTime : Convert.ToDateTime(row["UpdatedAt"])
            });
        }
        return list;
    }
}

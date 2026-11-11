using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOSManagement.Web.Utils;

/// <summary>
/// Lớp tiện ích DBUtil (Bài 3 - LAB 3)
/// Phụ trách: Nguyễn Kim Song Thư (Nhóm trưởng)
/// Hỗ trợ kết nối và thực thi các câu lệnh SQL qua ADO.NET thuần.
/// </summary>
public static class DBUtil
{
    // Chuỗi kết nối mặc định đến SQL Server (LocalDB CafePOS)
    public static string connString { get; set; } =
        "Server=(localdb)\\MSSQLLocalDB;Database=CafePOS;Integrated Security=True;TrustServerCertificate=True;";

    /// <summary>
    /// Thực thi câu lệnh SQL SELECT trả về DataTable
    /// </summary>
    public static DataTable queryDataTable(string sql, List<object>? parameters = null)
    {
        DataTable dataTable = new DataTable();
        using (SqlConnection conn = new SqlConnection(connString))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Thêm tham số vào SQL nếu có (sử dụng NVARCHAR cho string để bảo toàn Unicode)
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            var paramName = $"@{i + 1}";
                            var value = parameters[i] ?? DBNull.Value;
                            if (value is string)
                            {
                                var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                p.Value = value;
                                cmd.Parameters.Add(p);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(paramName, value);
                            }
                        }
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi QueryDataTable: {ex.Message}");
                throw;
            }
        }
        return dataTable;
    }

    /// <summary>
    /// Thực thi và lấy giá trị đơn (dùng cho COUNT, SUM, SCOPE_IDENTITY)
    /// </summary>
    public static object? executeScalar(string sql, List<object>? parameters = null)
    {
        object? result = null;
        using (SqlConnection conn = new SqlConnection(connString))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            var paramName = $"@{i + 1}";
                            var value = parameters[i] ?? DBNull.Value;
                            if (value is string)
                            {
                                var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                p.Value = value;
                                cmd.Parameters.Add(p);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(paramName, value);
                            }
                        }
                    }

                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi executeScalar: {ex.Message}");
                throw;
            }
        }
        return result;
    }

    /// <summary>
    /// Thực thi lệnh INSERT, UPDATE, DELETE trả về số dòng bị ảnh hưởng
    /// </summary>
    public static int executeNonQuery(string sql, List<object>? parameters = null)
    {
        int rowsAffected = 0;
        using (SqlConnection conn = new SqlConnection(connString))
        {
            try
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            var paramName = $"@{i + 1}";
                            var value = parameters[i] ?? DBNull.Value;
                            if (value is string)
                            {
                                var p = new SqlParameter(paramName, SqlDbType.NVarChar);
                                p.Value = value;
                                cmd.Parameters.Add(p);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(paramName, value);
                            }
                        }
                    }

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi executeNonQuery: {ex.Message}");
                throw;
            }
        }
        return rowsAffected;
    }
}

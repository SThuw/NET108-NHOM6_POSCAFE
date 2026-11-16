using System.Net;
using System.Net.Mail;

namespace CafePOSManagement.Web.Services;

public sealed class EmailService(IConfiguration configuration)
{
    private string SmtpHost => configuration["Email:SmtpHost"]?.Trim() ?? "";
    private string SmtpUserName => configuration["Email:SmtpUserName"]?.Trim() ?? "";
    private string ApiKey => configuration["Email:ApiKey"]?.Trim() ?? "";
    private string FromAddress => configuration["Email:FromAddress"]?.Trim() ?? "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost) &&
        !string.IsNullOrWhiteSpace(SmtpUserName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(FromAddress);

    public async Task SendResetPasswordAsync(string recipientEmail, string recipientName, string newPassword)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Dịch vụ email chưa được cấu hình.");
        }

        var port = configuration.GetValue("Email:SmtpPort", 587);
        var enableSsl = configuration.GetValue("Email:EnableSsl", true);
        var fromName = configuration["Email:FromName"]?.Trim() ?? "PolyCafe";

        using var message = new MailMessage
        {
            From = new MailAddress(FromAddress, fromName),
            Subject = "PolyCafe - Mật khẩu mới",
            Body = $"Xin chào {recipientName},\r\n\r\nMật khẩu mới của bạn là: {newPassword}\r\n\r\nDùng mật khẩu này để đăng nhập PolyCafe.",
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(recipientEmail, recipientName));

        using var smtp = new SmtpClient(SmtpHost, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(SmtpUserName, ApiKey)
        };
        await smtp.SendMailAsync(message);
    }
}

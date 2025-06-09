using api.Helpers;
using api.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace api.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Validate email address
                if (!MailboxAddress.TryParse(to, out var toAddress))
                {
                    _logger.LogError("Invalid email address: {Email}", to);
                    return false;
                }

                var email = new MimeMessage();

                // Thiết lập thông tin người gửi
                email.From.Add(new MailboxAddress(
                    ConfigHelper.EmailDisplayName,
                    ConfigHelper.EmailSender));

                // Thiết lập người nhận
                email.To.Add(toAddress);

                // Thiết lập tiêu đề email
                email.Subject = subject;

                // Thiết lập nội dung email
                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                email.Body = bodyBuilder.ToMessageBody();

                // Cấu hình SMTP client với timeout và retry
                using var smtp = new SmtpClient();
                smtp.Timeout = 30000; // 30 seconds timeout

                await smtp.ConnectAsync(
                    ConfigHelper.EmailHost,
                    ConfigHelper.EmailPort,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    ConfigHelper.Email,
                    ConfigHelper.EmailPassword);

                // Gửi email
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var frontendUrl = ConfigHelper.UserUrl;
            var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var subject = "SmartBuy Mobile - Đặt Lại Mật Khẩu";

            // Tạo nội dung email HTML
            string htmlBody = $@"
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Đặt Lại Mật Khẩu</title>
                    <style>
                        @import url('https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap');
                        
                        @media only screen and (max-width: 600px) {{
                            .email-container {{
                                width: 100% !important;
                                padding: 15px !important;
                            }}
                            .header {{
                                padding: 15px !important;
                            }}
                            .content {{
                                padding: 20px !important;
                            }}
                            .btn {{
                                width: 100% !important;
                                padding: 12px 15px !important;
                                font-size: 16px !important;
                            }}
                            .footer {{
                                padding: 15px !important;
                            }}
                            .expiration-notice {{
                                margin: 15px 0 !important;
                                padding: 10px !important;
                            }}
                        }}
                    </style>
                </head>
                <body style=""margin:0; padding:0; font-family: 'Roboto', Arial, sans-serif; background-color:#f8f0f4; color:#333;"">
                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"" width=""100%"" style=""max-width: 600px; margin: 0 auto;"">
                        <!-- HEADER -->
                        <tr>
                            <td class=""header"" style=""background: linear-gradient(135deg, #FF82C4 0%, #FF5DB3 100%); padding: 20px; text-align: center; border-radius: 8px 8px 0 0;"">
                                <h1 style=""color: white; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 1px; text-shadow: 0 1px 2px rgba(0,0,0,0.1);"">SmartBuy Mobile</h1>
                            </td>
                        </tr>
                        
                        <!-- MAIN CONTENT -->
                        <tr>
                            <td class=""email-container"" style=""background-color: white; padding: 0; border-radius: 0 0 8px 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.05);"">
                                
                                <!-- ICON SECTION -->
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                    <tr>
                                        <td style=""padding: 30px 0 10px 0; text-align: center;"">
                                            <div style=""font-size: 36px; line-height: 1;"">🔒</div>
                                        </td>
                                    </tr>
                                </table>
                                
                                <!-- TEXT CONTENT -->
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                    <tr>
                                        <td class=""content"" style=""padding: 20px 40px 30px 40px; text-align: center;"">
                                            <h2 style=""color: #FF5DB3; margin: 0 0 20px 0; font-size: 28px; font-weight: 600;"">Đặt Lại Mật Khẩu</h2>
                                            <p style=""color: #555; font-size: 16px; line-height: 24px; margin: 0 0 25px 0;"">
                                                Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu từ bạn. Nhấn vào nút bên dưới để thiết lập mật khẩu mới.
                                            </p>
                                            
                                            <!-- BUTTON -->
                                            <a href=""{resetUrl}"" class=""btn"" style=""display: inline-block; background: linear-gradient(to right, #FF82C4, #FF5DB3); color: white; padding: 14px 30px; text-decoration: none; font-weight: 500; border-radius: 30px; margin: 25px 0; font-size: 16px; text-transform: uppercase; letter-spacing: 1px; box-shadow: 0 4px 10px rgba(255,93,179,0.3); transition: all 0.3s ease;"">
                                                Đặt Lại Mật Khẩu
                                            </a>
                                            
                                            <!-- EXPIRATION NOTICE -->
                                            <div class=""expiration-notice"" style=""background-color: #f2f6ff; border-left: 4px solid #FF5DB3; margin: 20px 0; padding: 12px 15px; font-size: 15px; color: #555; text-align: left;"">
                                                <strong style=""color: #333;"">Lưu ý:</strong> Liên kết này sẽ hết hạn sau 15 phút kể từ khi email được gửi.
                                            </div>
                                            
                                            <p style=""color: #777; font-size: 14px; line-height: 21px; margin: 30px 0 0 0; padding-top: 20px; border-top: 1px solid #f0e0ea;"">
                                                Nếu bạn không yêu cầu điều này, bạn có thể bỏ qua email này.
                                            </p>
                                            
                                            <!-- HELP TEXT -->
                                            <p style=""color: #999; font-size: 13px; margin-top: 30px;"">
                                                Nếu nút không hoạt động, bạn có thể sao chép và dán liên kết sau vào trình duyệt:
                                            </p>
                                            <p style=""background-color: #f9f0f5; padding: 10px; border-radius: 4px; font-size: 12px; word-break: break-all; color: #777;"">
                                                {resetUrl}
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        
                        <!-- FOOTER -->
                        <tr>
                            <td class=""footer"" style=""padding: 30px; text-align: center; color: #999; font-size: 13px;"">
                                <p style=""margin: 0 0 15px 0;"">
                                    — Đội ngũ <strong style=""color: #FF5DB3;"">SmartBuy Mobile</strong> —
                                </p>
                                <p style=""margin: 0; font-size: 12px;"">
                                    © 2025 SmartBuy Mobile. Tất cả các quyền được bảo lưu.
                                </p>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string verificationToken)
        {
            var frontendUrl = ConfigHelper.UserUrl;
            var verificationUrl = $"{frontendUrl}/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

            var subject = "SmartBuy Mobile - Xác Thực Địa Chỉ Email";

            // Tạo nội dung email HTML
            string htmlBody = $@"
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Xác Thực Địa Chỉ Email</title>
                    <style>
                        @import url('https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap');
                        
                        @media only screen and (max-width: 600px) {{
                            .email-container {{
                                width: 100% !important;
                                padding: 15px !important;
                            }}
                            .header {{
                                padding: 15px !important;
                            }}
                            .content {{
                                padding: 20px !important;
                            }}
                            .btn {{
                                width: 100% !important;
                                padding: 12px 15px !important;
                                font-size: 16px !important;
                            }}
                            .footer {{
                                padding: 15px !important;
                            }}
                            .expiration-notice {{
                                margin: 15px 0 !important;
                                padding: 10px !important;
                            }}
                        }}
                    </style>
                </head>
                <body style=""margin:0; padding:0; font-family: 'Roboto', Arial, sans-serif; background-color:#f8f0f4; color:#333;"">
                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"" width=""100%"" style=""max-width: 600px; margin: 0 auto;"">
                        <!-- HEADER -->
                        <tr>
                            <td class=""header"" style=""background: linear-gradient(135deg, #FF82C4 0%, #FF5DB3 100%); padding: 20px; text-align: center; border-radius: 8px 8px 0 0;"">
                                <h1 style=""color: white; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 1px; text-shadow: 0 1px 2px rgba(0,0,0,0.1);"">SmartBuy Mobile</h1>
                            </td>
                        </tr>
                        
                        <!-- MAIN CONTENT -->
                        <tr>
                            <td class=""email-container"" style=""background-color: white; padding: 0; border-radius: 0 0 8px 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.05);"">
                                
                                <!-- ICON SECTION -->
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                    <tr>
                                        <td style=""padding: 30px 0 10px 0; text-align: center;"">
                                            <div style=""font-size: 36px; line-height: 1;"">✉️</div>
                                        </td>
                                    </tr>
                                </table>
                                
                                <!-- TEXT CONTENT -->
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                    <tr>
                                        <td class=""content"" style=""padding: 20px 40px 30px 40px; text-align: center;"">
                                            <h2 style=""color: #FF5DB3; margin: 0 0 20px 0; font-size: 28px; font-weight: 600;"">Xác Thực Địa Chỉ Email</h2>
                                            <p style=""color: #555; font-size: 16px; line-height: 24px; margin: 0 0 25px 0;"">
                                                Cảm ơn bạn đã đăng ký tài khoản tại SmartBuy Mobile. Vui lòng xác thực địa chỉ email của bạn bằng cách nhấn vào nút bên dưới.
                                            </p>
                                            
                                            <!-- BUTTON -->
                                            <a href=""{verificationUrl}"" class=""btn"" style=""display: inline-block; background: linear-gradient(to right, #FF82C4, #FF5DB3); color: white; padding: 14px 30px; text-decoration: none; font-weight: 500; border-radius: 30px; margin: 25px 0; font-size: 16px; text-transform: uppercase; letter-spacing: 1px; box-shadow: 0 4px 10px rgba(255,93,179,0.3); transition: all 0.3s ease;"">
                                                Xác Thực Email
                                            </a>
                                            
                                            <!-- EXPIRATION NOTICE -->
                                            <div class=""expiration-notice"" style=""background-color: #f2f6ff; border-left: 4px solid #FF5DB3; margin: 20px 0; padding: 12px 15px; font-size: 15px; color: #555; text-align: left;"">
                                                <strong style=""color: #333;"">Lưu ý:</strong> Liên kết này sẽ hết hạn sau 24 giờ kể từ khi email được gửi.
                                            </div>
                                            
                                            <p style=""color: #777; font-size: 14px; line-height: 21px; margin: 30px 0 0 0; padding-top: 20px; border-top: 1px solid #f0e0ea;"">
                                                Nếu bạn không tạo tài khoản này, bạn có thể bỏ qua email này.
                                            </p>
                                            
                                            <!-- HELP TEXT -->
                                            <p style=""color: #999; font-size: 13px; margin-top: 30px;"">
                                                Nếu nút không hoạt động, bạn có thể sao chép và dán liên kết sau vào trình duyệt:
                                            </p>
                                            <p style=""background-color: #f9f0f5; padding: 10px; border-radius: 4px; font-size: 12px; word-break: break-all; color: #777;"">
                                                {verificationUrl}
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        
                        <!-- FOOTER -->
                        <tr>
                            <td class=""footer"" style=""padding: 30px; text-align: center; color: #999; font-size: 13px;"">
                                <p style=""margin: 0 0 15px 0;"">
                                    — Đội ngũ <strong style=""color: #FF5DB3;"">SmartBuy Mobile</strong> —
                                </p>
                                <p style=""margin: 0; font-size: 12px;"">
                                    © 2025 SmartBuy Mobile. Tất cả các quyền được bảo lưu.
                                </p>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, htmlBody);
        }
    }
}
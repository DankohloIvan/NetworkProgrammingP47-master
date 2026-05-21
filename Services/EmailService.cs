using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace NetworkProgrammingP47.Services
{
    internal class EmailService
    {
        private const String settingsFilename = "smtp_settings.json";
        private static SmtpData? _smtpData;
        public static SmtpData SmtpData
        {
            get
            {
                if (_smtpData == null)
                {
                    if (!File.Exists(settingsFilename))
                    {
                        throw new FileNotFoundException(
                            "Помилка підключення конфігурації smtp_settings.json\n" +
                            "Якщо ви клонували проєкт, перечитайте README");
                    }
                    // Зчитуємо файл та вилучаємо деталі конфігурації
                    var settings = JsonSerializer.Deserialize<JsonElement>(
                        File.ReadAllText(settingsFilename)
                    );
                    var gmailSection = settings.GetProperty("Gmail");
                    _smtpData = new()
                    {
                        Host = gmailSection.GetProperty("Host").GetString()!,
                        Port = gmailSection.GetProperty("Port").GetInt32()!,
                        Email = gmailSection.GetProperty("Email").GetString()!,
                        Key = gmailSection.GetProperty("Key").GetString()!,
                    };
                }
                return _smtpData;
            }
        }
        public static void SendEmail(string email, string subject, string body)
        {
            MailMessage mailMessage = new()
            {
                From = new MailAddress(SmtpData.Email, "NP☛P47", Encoding.UTF8),
                IsBodyHtml = true,
                Subject = subject,
                Body = body
            };
            mailMessage.To.Add(email);
            using SmtpClient smtpClient = new()
            {
                Host = SmtpData.Host,
                Port = SmtpData.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(SmtpData.Email, SmtpData.Key)
            };
            smtpClient.Send(mailMessage);
        }

        public static void SendNewPassword(string email, string newPassword)
        {
            String subject = "Ваш новий пароль для сайту NP☛P47";
            String body = $"<h1>Ваш новий пароль: {newPassword}</h1>";
            SendEmail(email, subject, body);
        }

        public static void SendConfirmCode(string email, string code)
        {
            String subject = "Підтвердження реєстрації на сайті NP☛P47";
            String body = $"<h1>Ваш код підтвердження: {code}</h1>";
            SendEmail(email, subject, body);
        }

        public static void SendLoginNotification(string email, DateTime loginAt)
        {
            String subject = "Новий вхід на сайті NP☛P47";
            String body =
                "<p>Зафіксовано новий вхід з вашим паролем " +
                $"{loginAt:dd.MM.yyyy HH:mm:ss}. " +
                "Якщо це були не ви, то радимо змінити пароль.</p>";
            SendEmail(email, subject, body);
        }
    }

    internal class SmtpData
    {
        public String Host { get; set; } = null!;
        public int Port { get; set; }
        public String Email { get; set; } = null!;
        public String Key { get; set; } = null!;
    }
}

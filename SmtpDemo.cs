using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace NetworkProgrammingP47
{
    internal class SmtpDemo
    {
        private const String settingsFilename = "smtp_settings.json";
        public void Run()
        {
            Console.WriteLine("SMTP - Simple Mail Transfer Protocol");
            if (!File.Exists(settingsFilename))
            {
                Console.WriteLine("Помилка підключення конфігурації smtp_settings.json\n" +
                    "Якщо ви клонували проєкт, перечитайте README");
                return;
            }
            // Зчитуємо файл та вилучаємо деталі конфігурації
            var settings = JsonSerializer.Deserialize<JsonElement>(
                File.ReadAllText(settingsFilename)
            );
            var gmailSection = settings.GetProperty("Gmail");
            String host  = gmailSection.GetProperty("Host").GetString()!;
            int    port  = gmailSection.GetProperty("Port").GetInt32()!;
            String email = gmailSection.GetProperty("Email").GetString()!;
            String key   = gmailSection.GetProperty("Key").GetString()!;
            // Console.WriteLine("{0} {1} {2} {3}", host,port,email,key);

            // Клієнт - інструмент управління сервером
            SmtpClient smtpClient = new()
            {
                Host = host,
                Port = port,
                EnableSsl = true,
                Credentials = new NetworkCredential(email, key)
            };
            // Надсилаємо найпростіше повідомлення
            // smtpClient.Send(email, "dns.lector@ukr.net", "NP-P47", "Hello from SMTP Demo");

            // Надсилаємо розширене повідомлення
            MailMessage mailMessage = new()
            {
                From = new MailAddress(email, "NP☛P47", Encoding.UTF8),
                IsBodyHtml = true,
                Body = @"<html>
                <h1>Шановний клієнте!</h1>
                <p>Тільки для вас діє чудова <b style='color:maroon'>пропозиція</b></p>
                <p>Деталі на <a href='https://itstep.org/'>сайті</a></p>
                <p><a href='https://itstep.org/' style='text-decoration:none; color:snow; background-color: maroon; border-radius: 5px; padding: 7px 10px; font-variant: small-caps'>Зареєструватись</a></p>
                </html>",
                Subject = "Весняна пропозиція ☛"
            };
            mailMessage.To.Add("azure.spd111.od.0@ukr.net");
            mailMessage.To.Add("dns.lector@ukr.net");
            // Attachments - вкладення файлів (попередньо налаштовуємо файли на копіювання до директорії збірки)
            mailMessage.Attachments.Add(
                new Attachment(
                    fileName: "./Attachments/NP.png",
                    mediaType: "image/png"
            ));
            mailMessage.Attachments.Add(
                new Attachment(
                    fileName: "./Attachments/README.pdf",
                    mediaType: "application/pdf"
            ));
            mailMessage.Attachments.Add(
                new Attachment( 
                    fileName: "./Attachments/дз.txt",
                    mediaType: "text/plain"
            ));
            smtpClient.Send(mailMessage);
            smtpClient.Dispose();
        }
    }
}
/* Д.З. Створити шаблон листа для підтвердження пошти
 * при реєстрації нового користувача. Включити
 * - вітання з реєстрацією
 * - код, який потрібно ввести
 * - посилання-кнопка для автоматичної активації
 * Надіслати на dns.lector@ukr.net (підписати тему як ДЗ Прізвище)
 */

/* SMTP - Simple Mail Transfer Protocol - протокол надсилання електронних листів
 * Загальна схема:              сайт
 *                                | 
 * Програма                Поштовий сервер 1               Поштовий сервер 2
 *  підключення      ключ   е-пошта (адреса)                е-пошта (адреса)
 * (автентифікація) <-----> розробника                      клієнта (користувача)
 * 
 *  формування     SMTP     від: розробник 
 *  повідомлення ---------> до: користувач ---------------> Перегляд
 *  для користувача         Повідомлення   <--------------- Відповідь
 *                    IMAP                 |
 *  Запит на читання ------>               |
 *  вхідної пошти    <----------------------
 *  
 *  
 *  ЗБЕРІГАННЯ ПАРОЛЬНОЇ ІНФОРМАЦІЇ
 *  При публікації проєкту на репозитаріях (Гітхаб)
 *  виникає проблема поширення паролів, які потрібні
 *  програмі для підключення до різних сервісів.
 *  Рішення:
 *  створюються два файли з парольними даними - 
 *  один зі справжніми паролями, але вилучений з 
 *  репозиторію (.gitignore)
 *  інший - з тою структурою, як у першого файлу, 
 *  але з шаблонними даними для паролів на кшталт
 *  CHANGE-ME, <YOUR-KEY>, ******
 *  другий файл входить до репозиторію, а також 
 *  до опису репозиторію додається інструкція з 
 *  відновлення першого файлу
 *  Порядок дій:
 *  1. Редагуємо .gitignore (до того, як файл буде 
 *       створено, інакше він може бути доданий автоматично)
 *       додаємо ім'я майбутнього файлу (smtp_settings.json)
 *       Зберігаємо зміни.
 *  2. Створюємо сам файл smtp_settings.json, заповнюємо
 *       його даними, переконуємось, що він не потрапляє до 
 *       репозиторію (git changes). У властивостях файлу
 *       встановити ознаку "Copy to Output Directory" до "Copy always"
 *  3. Створюємо копію файлу smtp_settings_sample.json,   
 *       видаляємо в ньому усі ключові дані, замінюємо шаблонами
 *  4. Створюємо або редагуємо README.MD (деталі див. у файлі)    
 */
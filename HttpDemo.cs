using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP47
{
    internal class HttpDemo
    {
        private Stopwatch stopwatch = new();
        public async Task RunAsync()
        {
            Console.WriteLine("Http Demo");
            // більш повне формування запиту, ніж URL, надає HttpRequestMessage
            HttpRequestMessage httpRequest = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://itstep.org/"),
                Version = new(1,1),
                // Content = new StringContent("Hello, world")
            };
            httpRequest.Headers.Add("My-Header", "My-Value");

            // Ресурси для роботи з мережею є некерованими (unmanaged)
            // тому їх слід руйнувати або вживати авто-руйнування - using
            using HttpClient httpClient = new();

            // Повні деталі відповіді дають методи SendAsync / Send
            HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequest);
            Console.WriteLine($"HTTP/{responseMessage.Version} {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}");
            foreach(var header in responseMessage.Headers)
            {
                Console.WriteLine("{0}: {1}", 
                    header.Key,
                    String.Join(", ", header.Value));
            }
            Console.WriteLine();
            // тіло (контент) пакету може бути великим, більш того, розподіленим
            // за декількома пакетами-відповідями (chunks). Це зазначається заголовком
            // Transfer-Encoding: chunked
            Console.WriteLine(await responseMessage.Content.ReadAsStringAsync());
        }

        public async Task RunAsync1()
        {
            Console.WriteLine("Http Demo");
            String url;
            try 
            {
                url = "https://itstep.org/";  // GetAndValidateUrl(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            stopwatch.Start();
            // інструмент надсилання запитів - HttpClient
            HttpClient httpClient = new();
            String data = await httpClient.GetStringAsync(url);
            long ms = stopwatch.ElapsedMilliseconds;
            Console.WriteLine(data);
            Console.WriteLine("------------------------");
            Console.WriteLine($"Elapsed {ms} Milliseconds for loading, {stopwatch.ElapsedMilliseconds} total");
        }

        private String GetAndValidateUrl()
        {
            Console.Write("Введіть URL-адресу: ");
            String url = Console.ReadLine()!;
            int index = url.IndexOf("://");   //  роздільні символи для схеми запиту
            if(index == -1)
            {
                throw new FormatException("Введений URL повинен містити схему запиту");
            }
            String scheme = url[..index];
            if(scheme != "http" &&  scheme != "https")
            {
                throw new FormatException($"Cхема запиту '{scheme}' не підтримується: тільки http або https");
            }
            Console.WriteLine($"Схема запиту: '{scheme}'");
            /* Традиційно, адреси без параметрів (без query) мають завершуватись 
             * символом '/': https://itstep.org/
             * Але також традиційно адреси без слешів приймаються без зауважень: https://itstep.org
             * Відповідно, границею доменного імені є або перший після схеми '/', або кінець рядка
             * Задача: визначити доменне ім'я, відокремивши частину адреси від кінця схеми до 
             * першого слешу або кінця рядка, якщо слешів в залишку адреси немає.
             * https://od.itstep.org/contacts  -> od.itstep.org
             *        |             |
             *    кінець схеми   перший '/'
             *    
             * https://itstep.org    -> itstep.org
             *        |          |
             *    кінець схеми   кінець рядка
             *    
             */
            return "https://itstep.org/";
        }
    }
}
/* HTTP протокол прикладного рівня для обміну даними в Інтернет
 * Запит (Request) - дані, які надсилає клієнт, починаючи обмін
 *  Першою характеристикою запиту є його метод. Стандартизовано 9 методів,
 *  з яких 5 - загального призначення (4 - службового)
 *  метод          семантика
 *  GET             Read - одержання даних
 *  POST            Create - створення (додавання) нових даних
 *  PATCH           Update - часткове оновлення наявних даних
 *  PUT             Replace - повна заміна наявних даних
 *  DELETE          Delete - видалення даних
 *  
 * Запити та відповіді є відносно тривалими процесами, тому 
 * виконуються в асинхронному режимі.
 * 
 * Повна структура НТТР-запиту складається з наступного:
 *                               | Перший рядок - завжди один, складається з трьох частин
 * PATCH /user/admin HTTP/1.1    | МЕТОД path Protocol/version
 * Connection: Keep-Alive        | Далі слідують заголовки за форматом
 * Host: itstep.org              | Назва-Заголовку: Значення-Заголовку
 * Accept: text/html             | по одному на рядок, загальна кількість - довільна
 * Accept: text/plain            | Повтор заголовку - має формувати масив даних
 * Content-Type: text/plain      |  за наявності тіла - необхідно зазначати його тип (МІМЕ) https://www.iana.org/assignments/media-types/media-types.xhtml
 *                               | Порожній рядок - визначає кінець заголовків
 * email=admin@i.ua              | Далі до кінця пакету - тіло - довільний набір даних
 * 
 * 
 * Відповідь (Response) має схожу структуру, але з відмінностями у першому рядку - 
 *  першим іде протокол (як у запиті), далі статус-код, далі фраза (reason-phrase)
 *
 * HTTP/1.1 200 OK
 * Connection: Close
 * Content-Type: application/json; charset=utf-8
 * 
 * {"name": "Admin", "email": "admin@i.ua"}
 * 
 * --------------------------------------------
 * HTTP/1.1 400 Bad Request   | 406 Not Acceptable - формально некоректно, бо 406 це про заголовки
 * Connection: Close
 * Content-Type: text/html
 * 
 * E-mail domain i.ua is not allowed for administrators
 * -------------------------------------------------
 * 
 * Тіло пакету - не обов'язковий елемент і може бути відсутнім.
 * Більш того, запитам методами GET та HEAD заборонено мати тіло
 * 
 * Перелік статус-кодів та їх фраз стандартизований https://upload.wikimedia.org/wikipedia/commons/0/09/%D0%A1%D0%BF%D0%B8%D1%81%D0%BE%D0%BA_%D0%BA%D0%BE%D0%B4%D1%96%D0%B2_%D1%81%D1%82%D0%B0%D0%BD%D1%83_HTTP.pdf
 * 
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace CheckGeneratorService
{
    public class Item
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Sum { get; set; }
        public int? InvoiceType { get; set; }
        public decimal? InvoiceSum { get; set; }
        public int ProductType { get; set; }
        public int PaymentType { get; set; }

        [JsonPropertyName("organizationForm")]
        public string OrganizationForm { get; set; }
    }

    public class Receipt
    {
        public List<Item> Items { get; set; }
        public int Id { get; set; }
        public int User { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    class Program
    {
        private static readonly Random rnd = new Random();

        static async Task Main(string[] args)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();

            Console.WriteLine("=== Генератор чеков запущен ===");

            while (true)
            {
                var receipt = GenerateReceipt();
                string json = JsonSerializer.Serialize(receipt, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                Console.WriteLine($"[Отправка чека] Id={receipt.Id}, User={receipt.User}");
                Console.WriteLine(json);

                await producer.ProduceAsync("receipts-topic", new Message<Null, string> { Value = json });

                await Task.Delay(3000);
            }
        }

        private static Receipt GenerateReceipt()
        {
            var items = new List<Item>
            {
                new Item
                {
                    Name = SampleNames[rnd.Next(SampleNames.Length)],
                    Price = rnd.Next(50, 5000),
                    Quantity = 1,
                    Sum = 0,
                    InvoiceType = rnd.Next(0, 2) == 0 ? null : rnd.Next(1, 4),
                    InvoiceSum = null,
                    ProductType = rnd.Next(1, 30),
                    PaymentType = rnd.Next(1, 3),
                    OrganizationForm = SampleOrgs[rnd.Next(SampleOrgs.Length)]
                }
            };

            items[0].Sum = items[0].Price * items[0].Quantity;

            return new Receipt
            {
                Items = items,
                Id = rnd.Next(10000, 99999),
                User = rnd.Next(1000, 20000),
                CreatedAt = DateTime.Now
            };
        }

       private static readonly string[] SampleNames = 
{
    "Молоко 1л", "Хлеб белый", "Шоколад", "Консультация юриста",
    "Ремонт обуви", "Стрижка волос", "Учебники", "Установка Windows",
    "Сыр 200г", "Кофе в зернах", "Йогурт 150г", "Ремонт компьютера",
    "Маникюр", "Прачечная", "Пицца 30см", "Билеты в кино",
    "Такси 5км", "Книга 'Программирование на C#'", "Салат Цезарь", 
    "Флешка 32ГБ", "Батарейки AA", "Ремонт телефона", "Парикмахерская окрашивание",
    "Тренажерный зал - разовое посещение", "Массаж", "Кроссовки", "Рюкзак школьный"
};


       private static readonly string[] SampleOrgs = 
{
    "ООО \"Молочный мир\"",
    "ООО \"Хлебокомбинат №1\"",
    "ООО \"Кондитерская фабрика\"",
    "ООО \"Правовой Альянс\"",
    "ООО \"Мастерская обуви\"",
    "Салон красоты \"Шарм\"",
    "ИП Иванов И.И.",
    "ООО \"РемонтТех\"",
    "ИП Петров П.П.",
    "ООО \"Учебник+\"",
    "ООО \"КомпьютерСервис\"",
    "Салон красоты \"Люкс\"",
    "ООО \"Пицца Плюс\"",
    "ООО \"Такси Экспресс\"",
    "ИП Смирнова С.С.",
    "ООО \"Флешка и Ко\"",
    "ООО \"Батарейки Сервис\"",
    "ООО \"Массажный центр\"",
    "ООО \"Спорт и Фитнес\"",
    "ИП Кузнецов К.К.",
    "ООО \"Рюкзак Мастер\"",
    "ООО \"Кроссовки Профи\"",
    "Салон красоты \"Элегант\"",
    "ООО \"Салатная студия\""
};

    }
}

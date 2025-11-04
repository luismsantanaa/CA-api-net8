using Domain.Entities.Examples;

namespace Persistence.Seeds.Examples
{
    internal class Products
    {
        public List<TestProduct>? ProductsList { get; set; } = new List<TestProduct>();

        public Products()
        {
            ProductsList.Clear();
            ProductsList = new List<TestProduct>
            {
        new TestProduct {
            Name = "Pure Buble",
            Price = 1000.25,
            Description = "Skincare suitable for men and women",
            Image = "https://api.lorem.space/image?w=640&h=480&r=6599",

          },
        new TestProduct {
            Name = "Avenger Shirt",
            Price = 170.35,
            Description = "New range of formal shirts are designed keeping you in mind. With fits and styling that will make you stand apart",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=866",

          },
        new TestProduct {
            Name = "Fantastic Fresh Soap",
            Price = 753.43,
            Description = "Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=8632",

          },
        new TestProduct {
            Name = "Rustic Granite Car",
            Price = 221.19,
            Description = "The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive",
            Image = "https://api.lorem.space/image?w=640&h=480&r=6704",

          },
         new TestProduct {
            Name = "Incredible Frozen Soap",
            Price = 174.00,
            Description = "The slim & simple Maple Gaming Keyboard from Dev Byte comes with a sleek body and 7- Color RGB LED Back-lighting for smart functionality",
            Image = "https://api.lorem.space/image/shoes?w=640&h=480&r=8795",

          },
        new TestProduct {
            Name = "Refined Steel Keyboard",
            Price = 414.12,
            Description = "The Football Is Good For Training And Recreational Purposes",
            Image = "https://api.lorem.space/image/furniture?w=640&h=480&r=491",

          },
        new TestProduct {
            Name = "Refined Rubber Chicken",
            Price = 476.86,
            Description = "New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016",
            Image = "https://api.lorem.space/image/shoes?w=640&h=480&r=4976",

          },
        new TestProduct {
            Name = "Incredible Bronze Salad",
            Price = 552.08,
            Description = "Carbonite web goalkeeper gloves are ergonomically designed to give easy fit",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=3063",

          },
        new TestProduct{
            Name = "Ergonomic Fresh Ball",
            Price = 455.13,
            Description = "The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=2691",

          },
        new TestProduct {
            Name = "Rustic Plastic Gloves",
            Price = 788.21,
            Description = "New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016",
            Image = "https://api.lorem.space/image/furniture?w=640&h=480&r=2499",

          },
        new TestProduct{
            Name = "Sleek Wooden Cheese",
            Price = 935.33,
            Description = "The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=4445",

          },
        new TestProduct {
            Name = "Fantastic Metal Chips",
            Price = 439.45,
            Description = "New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016",
            Image = "https://api.lorem.space/image/fashion?w=640&h=480&r=2851",

          },
        new TestProduct {
            Name = "Oriental Fresh Bike",
            Price = 952.59,
            Description = "The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive",
            Image = "https://api.lorem.space/image?w=640&h=480&r=1726",

          },
        new TestProduct{
            Name = "Oriental Steel Cheese",
            Price = 363.64,
            Description = "New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016",
            Image = "https://api.lorem.space/image?w=640&h=480&r=5412",

          },new TestProduct {
            Name = "Elegant Frozen Bacon",
            Price = 182.72,
            Description = "The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design",
            Image = "https://api.lorem.space/image?w=640&h=480&r=8437",

          },
        new TestProduct {
            Name = "Handmade Concrete Towels",
            Price = 806.89,
            Description = "Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=8227",

          },
        new TestProduct{
            Name = "Modern Cotton Soap",
            Price = 233.95,
            Description = "The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=4653",

          },
        new TestProduct {
            Name = "Sleek Steel Bike",
            Price = 566.99,
            Description = "The beautiful range of Apple Naturalé that has an exciting mix of natural ingredients. With the Goodness of 100% Natural Ingredients",
            Image = "https://api.lorem.space/image/shoes?w=640&h=480&r=2934",

          },
        new TestProduct{
            Name = "Awesome Metal Soap",
            Price = 479.00,
            Description = "Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals",
            Image = "https://api.lorem.space/image?w=640&h=480&r=3150",

          },
        new TestProduct {
            Name = "Handmade Granite Fish",
            Price = 382.00,
            Description = "Boston's most advanced compression wear technology increases muscle oxygenation, stabilizes active muscles",
            Image = "https://api.lorem.space/image/watch?w=640&h=480&r=257",

          },
        new TestProduct {
            Name = "Elegant Granite Table",
            Price = 899.00,
            Description = "The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design",
            Image = "https://api.lorem.space/image/fashion?w=640&h=480&r=1533",

          },
        new TestProduct{
            Name = "Elegant Granite Chips",
            Price = 861.00,
            Description = "The Nagasaki Lander is the trademarked name of several series of Nagasaki sport bikes, that started with the 1984 ABC800J",
            Image = "https://api.lorem.space/image/furniture?w=640&h=480&r=6852",

          },
        new TestProduct {
            Name = "Incredible Plastic Salad",
            Price = 691.00,
            Description = "Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals",
            Image = "https://api.lorem.space/image?w=640&h=480&r=7336",

          }};
        }

        public List<TestProduct> GetProducts()
        {
            return ProductsList!;
        }
    }
}

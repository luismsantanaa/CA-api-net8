/*
    Script de carga de datos de ejemplo
    Este script se ejecuta en cada deployment y usa MERGE para evitar duplicados
*/

PRINT 'Cargando datos de ejemplo...'

-- Variables para almacenar IDs de categorías
DECLARE @ClothesId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111'
DECLARE @ElectronicsId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222'
DECLARE @FurnitureId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333'
DECLARE @ShoesId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444'
DECLARE @OthersId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555'
DECLARE @LibrosId UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666'
DECLARE @NuevaCategoriaId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777'

-- Insertar categorías de ejemplo
MERGE [Example].[TestCategories] AS target
USING (VALUES
    (@ClothesId, N'Clothes', N'https://api.lorem.space/image/fashion?w=640&h=480&r=7018'),
    (@ElectronicsId, N'Electronics', N'https://api.lorem.space/image/watch?w=640&h=480&r=8257'),
    (@FurnitureId, N'Furniture', N'https://api.lorem.space/image/furniture?w=640&h=480&r=4054'),
    (@ShoesId, N'Shoes', N'https://api.lorem.space/image/shoes?w=640&h=480&r=6006'),
    (@OthersId, N'Others', N'https://api.lorem.space/image?w=640&h=480&r=2375'),
    (@LibrosId, N'Libros', N'https://www.pexels.com/es-es/foto/paris-libros-monitor-coleccion-12799377/'),
    (@NuevaCategoriaId, N'Nueva categoria', N'https://placeimg.com/640/480/any')
) AS source ([Id], [Name], [Image])
ON target.[Id] = source.[Id]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Active], [Name], [Description], [Image], [CreatedOn], [Version])
    VALUES (source.[Id], 1, source.[Name], NULL, source.[Image], GETUTCDATE(), 1);

PRINT 'Categorías insertadas/actualizadas correctamente'

-- Insertar productos de ejemplo
MERGE [Example].[TestProduct] AS target
USING (VALUES
    (N'Pure Buble', 1000.25, N'Skincare suitable for men and women', N'https://api.lorem.space/image?w=640&h=480&r=6599', @OthersId),
    (N'Avenger Shirt', 170.35, N'New range of formal shirts are designed keeping you in mind. With fits and styling that will make you stand apart', N'https://api.lorem.space/image/watch?w=640&h=480&r=866', @ClothesId),
    (N'Fantastic Fresh Soap', 753.43, N'Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals', N'https://api.lorem.space/image/watch?w=640&h=480&r=8632', @OthersId),
    (N'Rustic Granite Car', 221.19, N'The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive', N'https://api.lorem.space/image?w=640&h=480&r=6704', @OthersId),
    (N'Incredible Frozen Soap', 174.00, N'The slim & simple Maple Gaming Keyboard from Dev Byte comes with a sleek body and 7- Color RGB LED Back-lighting for smart functionality', N'https://api.lorem.space/image/shoes?w=640&h=480&r=8795', @OthersId),
    (N'Refined Steel Keyboard', 414.12, N'The Football Is Good For Training And Recreational Purposes', N'https://api.lorem.space/image/furniture?w=640&h=480&r=491', @ElectronicsId),
    (N'Refined Rubber Chicken', 476.86, N'New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016', N'https://api.lorem.space/image/shoes?w=640&h=480&r=4976', @OthersId),
    (N'Incredible Bronze Salad', 552.08, N'Carbonite web goalkeeper gloves are ergonomically designed to give easy fit', N'https://api.lorem.space/image/watch?w=640&h=480&r=3063', @OthersId),
    (N'Ergonomic Fresh Ball', 455.13, N'The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive', N'https://api.lorem.space/image/watch?w=640&h=480&r=2691', @OthersId),
    (N'Rustic Plastic Gloves', 788.21, N'New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016', N'https://api.lorem.space/image/furniture?w=640&h=480&r=2499', @FurnitureId),
    (N'Sleek Wooden Cheese', 935.33, N'The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive', N'https://api.lorem.space/image/watch?w=640&h=480&r=4445', @OthersId),
    (N'Fantastic Metal Chips', 439.45, N'New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016', N'https://api.lorem.space/image/fashion?w=640&h=480&r=2851', @OthersId),
    (N'Oriental Fresh Bike', 952.59, N'The automobile layout consists of a front-engine design, with transaxle-type transmissions mounted at the rear of the engine and four wheel drive', N'https://api.lorem.space/image?w=640&h=480&r=1726', @OthersId),
    (N'Oriental Steel Cheese', 363.64, N'New ABC 13 9370, 13.3, 5th Gen CoreA5-8250U, 8GB RAM, 256GB SSD, power UHD Graphics, OS 10 Home, OS Office A & J 2016', N'https://api.lorem.space/image?w=640&h=480&r=5412', @OthersId),
    (N'Elegant Frozen Bacon', 182.72, N'The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design', N'https://api.lorem.space/image?w=640&h=480&r=8437', @ElectronicsId),
    (N'Handmade Concrete Towels', 806.89, N'Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals', N'https://api.lorem.space/image/watch?w=640&h=480&r=8227', @ShoesId),
    (N'Modern Cotton Soap', 233.95, N'The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design', N'https://api.lorem.space/image/watch?w=640&h=480&r=4653', @OthersId),
    (N'Sleek Steel Bike', 566.99, N'The beautiful range of Apple Naturalé that has an exciting mix of natural ingredients. With the Goodness of 100% Natural Ingredients', N'https://api.lorem.space/image/shoes?w=640&h=480&r=2934', @OthersId),
    (N'Awesome Metal Soap', 479.00, N'Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals', N'https://api.lorem.space/image?w=640&h=480&r=3150', @ShoesId),
    (N'Handmade Granite Fish', 382.00, N'Boston''s most advanced compression wear technology increases muscle oxygenation, stabilizes active muscles', N'https://api.lorem.space/image/watch?w=640&h=480&r=257', @OthersId),
    (N'Elegant Granite Table', 899.00, N'The Apollotech B340 is an affordable wireless mouse with reliable connectivity, 12 months battery life and modern design', N'https://api.lorem.space/image/fashion?w=640&h=480&r=1533', @FurnitureId),
    (N'Elegant Granite Chips', 861.00, N'The Nagasaki Lander is the trademarked name of several series of Nagasaki sport bikes, that started with the 1984 ABC800J', N'https://api.lorem.space/image/furniture?w=640&h=480&r=6852', @FurnitureId),
    (N'Incredible Plastic Salad', 691.00, N'Andy shoes are designed to keeping in mind durability as well as trends, the most stylish range of shoes & sandals', N'https://api.lorem.space/image?w=640&h=480&r=7336', @OthersId)
) AS source ([Name], [Price], [Description], [Image], [CategoryId])
ON target.[Name] = source.[Name]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Active], [Name], [Description], [Image], [Price], [Stock], [CategoryId], [CreatedOn], [Version], [IsDeleted])
    VALUES (NEWID(), 1, source.[Name], source.[Description], source.[Image], source.[Price], 10, source.[CategoryId], GETUTCDATE(), 1, 0);

PRINT 'Productos insertados/actualizados correctamente'
PRINT 'Datos de ejemplo cargados exitosamente'
GO


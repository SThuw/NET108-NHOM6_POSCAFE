IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] varchar(40) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] varchar(40) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Categories_DisplayOrder] CHECK ([DisplayOrder] >= 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE TABLE [Discounts] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DiscountPercent] decimal(5,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Discounts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Discounts_Percent] CHECK ([DiscountPercent] >= 0 AND [DiscountPercent] <= 100)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] varchar(40) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [CategoryId] varchar(40) NOT NULL,
        [OriginalPrice] decimal(18,0) NULL,
        [Price] decimal(18,0) NOT NULL,
        [ImageUrl] nvarchar(500) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [Description] nvarchar(1000) NULL,
        [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Products_OriginalPrice] CHECK ([OriginalPrice] IS NULL OR ([OriginalPrice] >= [Price] AND [OriginalPrice] <= 10000000)),
        CONSTRAINT [CK_Products_Price] CHECK ([Price] > 0 AND [Price] <= 10000000),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] bigint NOT NULL IDENTITY,
        [RequestId] uniqueidentifier NOT NULL,
        [OrderNumber] varchar(20) NOT NULL,
        [OrderType] nvarchar(50) NOT NULL,
        [DiscountId] int NULL,
        [DiscountPercent] decimal(5,2) NOT NULL,
        [TaxPercent] decimal(5,2) NOT NULL,
        [Note] nvarchar(500) NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [Subtotal] decimal(18,0) NOT NULL,
        [DiscountAmount] decimal(18,0) NOT NULL,
        [TaxAmount] decimal(18,0) NOT NULL,
        [TotalAmount] decimal(18,0) NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Orders_Amounts] CHECK ([Subtotal] >= 0 AND [DiscountAmount] >= 0 AND [DiscountAmount] <= [Subtotal] AND [TaxAmount] >= 0 AND [TotalAmount] = [Subtotal] - [DiscountAmount] + [TaxAmount]),
        CONSTRAINT [CK_Orders_OrderType] CHECK ([OrderType] IN (N'Mang đi', N'Giao hàng', N'Tại bàn')),
        CONSTRAINT [CK_Orders_PaymentMethod] CHECK ([PaymentMethod] IN (N'Tiền mặt', N'Thẻ ngân hàng', N'Ví điện tử')),
        CONSTRAINT [CK_Orders_Percent] CHECK ([DiscountPercent] BETWEEN 0 AND 100 AND [TaxPercent] BETWEEN 0 AND 100),
        CONSTRAINT [CK_Orders_RequestId] CHECK ([RequestId] <> '00000000-0000-0000-0000-000000000000'),
        CONSTRAINT [CK_Orders_Status] CHECK ([Status] IN (N'Hoàn thành', N'Đã hủy')),
        CONSTRAINT [FK_Orders_Discounts_DiscountId] FOREIGN KEY ([DiscountId]) REFERENCES [Discounts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] bigint NOT NULL IDENTITY,
        [ProductId] varchar(40) NOT NULL,
        [ProductName] nvarchar(150) NOT NULL,
        [UnitPrice] decimal(18,0) NOT NULL,
        [Quantity] int NOT NULL,
        [OrderId] bigint NOT NULL,
        [LineTotal] AS CONVERT(decimal(18,0), [UnitPrice] * [Quantity]) PERSISTED,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderItems_Quantity] CHECK ([Quantity] BETWEEN 1 AND 99),
        CONSTRAINT [CK_OrderItems_UnitPrice] CHECK ([UnitPrice] > 0 AND [UnitPrice] <= 10000000),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'DisplayOrder', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] ON;
    EXEC(N'INSERT INTO [Categories] ([Id], [Code], [CreatedAt], [DisplayOrder], [IsActive], [Name])
    VALUES (''coldbrew'', ''coldbrew'', ''2026-11-07T08:00:00.0000000+07:00'', 4, CAST(1 AS bit), N''Cold Brew đặc biệt''),
    (''espresso'', ''espresso'', ''2026-11-07T08:00:00.0000000+07:00'', 5, CAST(1 AS bit), N''Cà phê Ý (Espresso)''),
    (''sandwich'', ''sandwich'', ''2026-11-07T08:00:00.0000000+07:00'', 3, CAST(1 AS bit), N''Bánh mì & Sandwich''),
    (''seasonal'', ''seasonal'', ''2026-11-07T08:00:00.0000000+07:00'', 6, CAST(1 AS bit), N''Đồ uống theo mùa''),
    (''snack'', ''snack'', ''2026-11-07T08:00:00.0000000+07:00'', 2, CAST(1 AS bit), N''Đồ ăn nhẹ''),
    (''tea'', ''tea'', ''2026-11-07T08:00:00.0000000+07:00'', 7, CAST(1 AS bit), N''Trà trái cây''),
    (''traditional'', ''traditional'', ''2026-11-07T08:00:00.0000000+07:00'', 1, CAST(1 AS bit), N''Cà phê truyền thống'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'DisplayOrder', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DiscountPercent', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Discounts]'))
        SET IDENTITY_INSERT [Discounts] ON;
    EXEC(N'INSERT INTO [Discounts] ([Id], [Code], [DiscountPercent], [IsActive], [Name])
    VALUES (1, N''NONE'', 0.0, CAST(1 AS bit), N''Không giảm''),
    (2, N''SALE10'', 10.0, CAST(1 AS bit), N''Giảm 10%''),
    (3, N''THANK15'', 15.0, CAST(1 AS bit), N''Giảm 15% tri ân''),
    (4, N''SALE20'', 20.0, CAST(1 AS bit), N''Giảm 20% Khách VIP'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DiscountPercent', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Discounts]'))
        SET IDENTITY_INSERT [Discounts] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DiscountAmount', N'DiscountId', N'DiscountPercent', N'Note', N'OrderNumber', N'OrderType', N'PaymentMethod', N'RequestId', N'Status', N'Subtotal', N'TaxAmount', N'TaxPercent', N'TotalAmount') AND [object_id] = OBJECT_ID(N'[Orders]'))
        SET IDENTITY_INSERT [Orders] ON;
    EXEC(N'INSERT INTO [Orders] ([Id], [CreatedAt], [DiscountAmount], [DiscountId], [DiscountPercent], [Note], [OrderNumber], [OrderType], [PaymentMethod], [RequestId], [Status], [Subtotal], [TaxAmount], [TaxPercent], [TotalAmount])
    VALUES (CAST(1 AS bigint), ''2026-11-07T08:10:00.0000000+07:00'', 0.0, NULL, 0.0, N''Đơn mẫu — ít đá'', ''#001'', N''Mang đi'', N''Tiền mặt'', ''00000000-0000-4000-8000-000000000001'', N''Hoàn thành'', 45000.0, 0.0, 0.0, 45000.0),
    (CAST(2 AS bigint), ''2026-11-07T08:20:00.0000000+07:00'', 10000.0, 2, 10.0, N''Đơn mẫu — dùng tại bàn'', ''#002'', N''Tại bàn'', N''Thẻ ngân hàng'', ''00000000-0000-4000-8000-000000000002'', N''Hoàn thành'', 100000.0, 7200.0, 8.0, 97200.0),
    (CAST(3 AS bigint), ''2026-11-07T08:30:00.0000000+07:00'', 20250.0, 3, 15.0, N''Đơn mẫu — giao hàng'', ''#003'', N''Giao hàng'', N''Ví điện tử'', ''00000000-0000-4000-8000-000000000003'', N''Hoàn thành'', 135000.0, 11475.0, 10.0, 126225.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'DiscountAmount', N'DiscountId', N'DiscountPercent', N'Note', N'OrderNumber', N'OrderType', N'PaymentMethod', N'RequestId', N'Status', N'Subtotal', N'TaxAmount', N'TaxPercent', N'TotalAmount') AND [object_id] = OBJECT_ID(N'[Orders]'))
        SET IDENTITY_INSERT [Orders] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'CreatedAt', N'Description', N'ImageUrl', N'IsAvailable', N'Name', N'OriginalPrice', N'Price') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] ON;
    EXEC(N'INSERT INTO [Products] ([Id], [CategoryId], [CreatedAt], [Description], [ImageUrl], [IsAvailable], [Name], [OriginalPrice], [Price])
    VALUES (''p1'', ''traditional'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cà phê sữa đá — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cà phê sữa đá'', 55000.0, 45000.0),
    (''p10'', ''seasonal'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cà phê sữa mùa đông — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cà phê sữa mùa đông'', NULL, 49000.0),
    (''p11'', ''coldbrew'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cold Brew đá — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cold Brew đá'', 55000.0, 49000.0),
    (''p12'', ''snack'', ''2026-11-07T08:00:00.0000000+07:00'', N''Bánh quy bơ — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Bánh quy bơ'', 35000.0, 29000.0),
    (''p13'', ''seasonal'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cà phê bọt biển Whipped Coffee — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cà phê bọt biển Whipped Coffee'', 65000.0, 45000.0),
    (''p14'', ''traditional'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cà phê phin Filter Coffee — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cà phê phin Filter Coffee'', NULL, 22000.0),
    (''p15'', ''espresso'', ''2026-11-07T08:00:00.0000000+07:00'', N''Latte nghệ thuật — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Latte nghệ thuật'', NULL, 60000.0),
    (''p16'', ''coldbrew'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cold Brew cam sả — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cold Brew cam sả'', NULL, 45000.0),
    (''p2'', ''espresso'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cappuccino — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cappuccino'', 65000.0, 55000.0),
    (''p3'', ''snack'', ''2026-11-07T08:00:00.0000000+07:00'', N''Bánh Croissant bơ — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Bánh Croissant bơ'', 50000.0, 45000.0),
    (''p4'', ''espresso'', ''2026-11-07T08:00:00.0000000+07:00'', N''Americano nóng — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Americano nóng'', 45000.0, 40000.0),
    (''p5'', ''traditional'', ''2026-11-07T08:00:00.0000000+07:00'', N''Bạc xỉu đá — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Bạc xỉu đá'', NULL, 39000.0),
    (''p6'', ''sandwich'', ''2026-11-07T08:00:00.0000000+07:00'', N''Sandwich nướng — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Sandwich nướng'', 75000.0, 65000.0),
    (''p7'', ''coldbrew'', ''2026-11-07T08:00:00.0000000+07:00'', N''Cold Brew sữa tươi — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Cold Brew sữa tươi'', 60000.0, 55000.0),
    (''p8'', ''tea'', ''2026-11-07T08:00:00.0000000+07:00'', N''Trà chanh thanh mát — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1556679343-c7306c1976bc?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Trà chanh thanh mát'', NULL, 45000.0),
    (''p9'', ''espresso'', ''2026-11-07T08:00:00.0000000+07:00'', N''Latte kem sữa — món mẫu của CafePOS.'', N''https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=85'', CAST(1 AS bit), N''Latte kem sữa'', 65000.0, 59000.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CategoryId', N'CreatedAt', N'Description', N'ImageUrl', N'IsAvailable', N'Name', N'OriginalPrice', N'Price') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'OrderId', N'ProductId', N'ProductName', N'Quantity', N'UnitPrice') AND [object_id] = OBJECT_ID(N'[OrderItems]'))
        SET IDENTITY_INSERT [OrderItems] ON;
    EXEC(N'INSERT INTO [OrderItems] ([Id], [OrderId], [ProductId], [ProductName], [Quantity], [UnitPrice])
    VALUES (CAST(1 AS bigint), CAST(1 AS bigint), ''p1'', N''Cà phê sữa đá'', 1, 45000.0),
    (CAST(2 AS bigint), CAST(2 AS bigint), ''p2'', N''Cappuccino'', 1, 55000.0),
    (CAST(3 AS bigint), CAST(2 AS bigint), ''p3'', N''Bánh Croissant bơ'', 1, 45000.0),
    (CAST(4 AS bigint), CAST(3 AS bigint), ''p13'', N''Cà phê bọt biển Whipped Coffee'', 2, 45000.0),
    (CAST(5 AS bigint), CAST(3 AS bigint), ''p16'', N''Cold Brew cam sả'', 1, 45000.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'OrderId', N'ProductId', N'ProductName', N'Quantity', N'UnitPrice') AND [object_id] = OBJECT_ID(N'[OrderItems]'))
        SET IDENTITY_INSERT [OrderItems] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Code] ON [Categories] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Discounts_Code] ON [Discounts] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderItems_OrderId_ProductId] ON [OrderItems] ([OrderId], [ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE INDEX [IX_Orders_CreatedAt] ON [Orders] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE INDEX [IX_Orders_DiscountId] ON [Orders] ([DiscountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_RequestId] ON [Orders] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_CategoryId_Name] ON [Products] ([CategoryId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107124907_InitialPosSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107124907_InitialPosSchema', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107125234_NormalizeVietnameseNames'
)
BEGIN
    DROP INDEX [IX_Products_CategoryId_Name] ON [Products];
    DROP INDEX [IX_Products_Name] ON [Products];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(150) COLLATE Latin1_General_100_CI_AI NOT NULL;
    CREATE UNIQUE INDEX [IX_Products_CategoryId_Name] ON [Products] ([CategoryId], [Name]);
    CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107125234_NormalizeVietnameseNames'
)
BEGIN
    DROP INDEX [IX_Categories_Name] ON [Categories];
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Name');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Categories] ALTER COLUMN [Name] nvarchar(100) COLLATE Latin1_General_100_CI_AI NOT NULL;
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107125234_NormalizeVietnameseNames'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107125234_NormalizeVietnameseNames', N'8.0.33');
END;
GO

COMMIT;
GO


# Entities

Giai đoạn 3 có Category, Product, DiscountCode, Order và OrderItem.
Các record giữ dữ liệu bất biến sau khi tạo; OrderItem lưu tên/giá lúc chốt đơn.
Service in-memory quản lý dữ liệu phía sau interface. Chưa có DbSet, mapping
hay migration; thiết kế SQL Server thuộc giai đoạn 4, tích hợp EF Core thuộc giai đoạn 5.


LAB 6 - PHAN CODE TOP 5 THUC UONG BAN CHAY
Thanh vien: Nguyen Lam Chi Tam
Branch: feature/tam-lab6-top-selling

Cac file:
- src/CafePOSManagement.Web/Controllers/TopSellingController.cs
- src/CafePOSManagement.Web/Services/TopSellingService.cs
- src/CafePOSManagement.Web/Views/TopSelling/Index.cshtml

Luu y:
- Controller tao TopSellingService truc tiep de khong can sua Program.cs va tranh cham file chung.
- Chi tinh cac hoa don co Status = 'Hoan thanh'.
- Loc ngay theo ngay lich Viet Nam (UTC+7), ngay ket thuc duoc tinh tron ngay.
- Doanh thu lay tu OrderItems.LineTotal (gia tai thoi diem chot don), khong lay gia hien tai cua Products.
- Chua them link menu vao _Layout.cshtml; theo phan cong, truong nhom se tich hop sau.
- Moi truong tao file khong co dotnet SDK nen chua the chay dotnet build. Can build/test tren may lap trinh truoc khi merge.
- Link để mở Chức năng TOP 5: https://localhost:xxxx/TopSelling/Index

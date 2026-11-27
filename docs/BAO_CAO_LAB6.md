# BÁO CÁO THỰC HÀNH LAB 6 — DỰ ÁN .NET 1 (NET108)
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyCafe - POS Cafe)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)  
**Nhóm trưởng:** Nguyễn Kim Song Thư (MSSV: TB02272)

---

## MỤC TIÊU LAB 6
✓ Xây dựng và bổ sung thêm các chức năng cho quản lý hóa đơn (Danh sách đơn hàng & Chi tiết đơn hàng).  
✓ Xây dựng các chức năng thống kê (5 thức uống bán chạy nhất & Thống kê doanh thu tùy chọn theo khoảng thời gian).  
✓ Tạo biểu đồ trực quan cho các chức năng thống kê bằng thư viện **Chart.js** kết hợp ứng dụng AI.

---

## BẢNG PHÂN CÔNG & TỔNG HỢP KẾT QUẢ THỰC HIỆN LAB 6

| STT | Họ và tên | MSSV | Vai trò | Nội dung phụ trách trong LAB 6 | Minh chứng Source Code | Đánh giá |
| :-: | :--- | :---: | :---: | :--- | :--- | :---: |
| **1** | **Nguyễn Kim Song Thư** | **TB02272** | **Nhóm trưởng** | - **Bài 3:** Tích hợp biểu đồ **Chart.js** (`<canvas id="myChart">`) cho thống kê Top 5 đồ uống & điều phối Dashboard biểu đồ.<br>- Khớp nối hệ thống điều hướng `_Layout.cshtml`, đồng bộ Migration CSDL và tổng hợp báo cáo Lab 6. | - `Views/TopSelling/Index.cshtml`<br>- `Views/Shared/_Layout.cshtml`<br>- `docs/BAO_CAO_LAB6.md` | 100% |
| **2** | **Nguyễn Trí Nguyễn** | **TB02277** | Thành viên | - **Bài 1 (Phần A):** Hiển thị danh sách tất cả đơn hàng ở phía Quản lý, bộ lọc tìm kiếm theo mã đơn, lọc theo khoảng thời gian, cập nhật trạng thái đơn và modal xem nhanh. | - `Controllers/Lab3Controller.cs`<br>- `Views/Lab3/Index.cshtml` (`#tab-orders`)<br>- `DAL/OrderDAL.cs` | 100% |
| **3** | **Trần Thị Tuyết Kha** | **TB02214** | Thành viên | - **Bài 1 (Phần B):** Xem chi tiết đơn hàng với đầy đủ: danh sách sản phẩm, số lượng, đơn giá, tổng tiền, trạng thái, **tên nhân viên tạo đơn**, ngày tạo đơn & in hóa đơn 80mm. | - `Controllers/OrderDetailController.cs`<br>- `Views/OrderDetail/Index.cshtml`<br>- `Views/OrderDetail/_PrintReceipt.cshtml` | 100% |
| **4** | **Ninh Văn Đức** | **TB02291** | Thành viên | - **Bài 2 (Phần A) & Bài 3:** Xây dựng chức năng thống kê doanh thu tùy chọn theo khoảng thời gian (Từ ngày - Đến ngày) kèm hệ thống biểu đồ **Chart.js** biến động doanh thu & cơ cấu danh mục. | - `Controllers/RevenueReportController.cs`<br>- `Services/RevenueReportService.cs`<br>- `Views/RevenueReport/Index.cshtml`<br>- `wwwroot/js/revenue-report.js` | 100% |
| **5** | **Nguyễn Lâm Chí Tâm** | **TB02234** | Thành viên | - **Bài 2 (Phần B):** Xây dựng chức năng thống kê **Top 5 thức uống bán chạy nhất** theo số lượng ly bán ra và doanh thu tùy chọn theo khoảng thời gian. | - `Controllers/TopSellingController.cs`<br>- `Services/TopSellingService.cs`<br>- `Views/TopSelling/Index.cshtml` | 100% |

---

## PHẦN I: BÀI 1 — QUẢN LÝ HÓA ĐƠN Ở PHÍA QUẢN LÝ (5 ĐIỂM)

### 1.1. Hiển thị danh sách tất cả đơn hàng (`/Lab3?tab=orders`)
- **Giao diện danh sách:** Hiển thị toàn bộ các đơn hàng đã phát sinh trong hệ thống, sắp xếp mới nhất lên đầu (`ORDER BY Id DESC`).
- **Thông tin hiển thị trên bảng:** Số hóa đơn (`#001`, `#002`...), Loại đơn (Tại bàn / Mang đi / Giao hàng / Kiosk), Thời gian lập, Tạm tính, Giảm giá, Thuế VAT, Tổng thanh toán, Hình thức thanh toán, Trạng thái đơn hàng (`Đang chuẩn bị`, `Chờ nhận món`, `Hoàn thành`, `Hủy đơn`).
- **Bộ lọc thông minh:** Tìm kiếm tức thì theo mã hóa đơn kết hợp lọc theo khoảng ngày (`Từ ngày` - `Đến ngày`).

### 1.2. Xem chi tiết đơn hàng (`/OrderDetail/{id}` và Modal Chi tiết)
Khi Quản lý bấm vào nút **Chi tiết** (hoặc **Xem nhanh**) trên bất kỳ đơn hàng nào, hệ thống hiển thị đầy đủ các thông tin theo đúng yêu cầu đề bài:
1. **Thông tin chung của đơn hàng:**
   - Mã hóa đơn (`OrderNumber`), Trạng thái đơn hàng (`Status`), **Tên nhân viên tạo đơn** (`CreatedByUser.FullName`), Ngày giờ tạo đơn (`CreatedAt`), Loại đơn và Phương thức thanh toán.
2. **Danh sách sản phẩm trong đơn:**
   - Bảng chi tiết từng món gồm: Tên sản phẩm (`ProductName`), Đơn giá (`UnitPrice`), Số lượng (`Quantity`), Thành tiền (`LineTotal`).
   - Phần tổng kết cuối bảng: Tạm tính (`Subtotal`), Giảm giá (`DiscountAmount`), Thuế VAT (`TaxAmount`) và Tổng cộng (`Total`).
3. **In hóa đơn nhiệt 80mm:** Hỗ trợ nút **In hóa đơn** với bố cục biên lai chuẩn máy in hóa đơn POS (`_PrintReceipt.cshtml`).

---

## PHẦN II: BÀI 2 — THỐNG KÊ 5 THỨC UỐNG BÁN CHẠY & DOANH THU THEO KHOẢNG THỜI GIAN (3 ĐIỂM)

### 2.1. Thống kê 5 thức uống bán chạy (`/TopSelling`)
- Được xây dựng tại `TopSellingController` và `TopSellingService`:
  - Truy vấn các hóa đơn có trạng thái **`Hoàn thành`** trong khoảng thời gian tùy chọn (`fromDate` - `toDate`).
  - Nhóm dữ liệu (`GroupBy`) theo mã và tên món từ bảng `OrderItems`, tính tổng số lượng ly bán ra (`SUM(Quantity)`) và tổng doanh thu (`SUM(LineTotal)`).
  - Sắp xếp giảm dần theo số lượng ly bán ra và lấy **Top 5** (`.OrderByDescending(x => x.QuantitySold).ThenByDescending(x => x.Revenue).Take(5)`).

### 2.2. Thống kê doanh thu tùy chọn theo khoảng thời gian (`/RevenueReport`)
- Được xây dựng tại `RevenueReportController` và `RevenueReportService`:
  - Cho phép chọn nhanh kỳ thống kê (*Hôm nay*, *Tuần này*, *Tháng này*, *Năm nay*) hoặc tùy chọn khoảng ngày bất kỳ (*Từ ngày* - *Đến ngày*).
  - Tổng hợp chính xác từ SQL Server: **Tổng doanh thu**, **Tổng số lượng đơn hàng hoàn thành**, **Giá trị trung bình mỗi đơn (AOV)** và **Tổng số ly đã bán**, kèm tỷ lệ tăng trưởng so với kỳ liền trước.

---

## PHẦN III: BÀI 3 — TẠO BIỂU ĐỒ CHO CÁC CHỨC NĂNG THỐNG KÊ VỚI CHART.JS & AI (2 ĐIỂM)

Hệ thống sử dụng thư viện **Chart.js** (`wwwroot/lib/chart.js/chart.umd.min.js`) để trực quan hóa dữ liệu thống kê trên cả 2 trang:

### 3.1. Biểu đồ cột Top 5 thức uống bán chạy (`<canvas id="myChart"></canvas>`)
Tại trang `Views/TopSelling/Index.cshtml`, dữ liệu danh sách Top 5 món được serialize sang JSON và vẽ trực tiếp lên thẻ `<canvas id="myChart"></canvas>`:

```html
<div style="width: 88%; max-width: 920px; height: 320px; margin: 18px auto 0;">
    <canvas id="myChart" aria-label="Biểu đồ Top 5 thức uống bán chạy"></canvas>
</div>
```

```javascript
const labels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.Items.Select(x => x.ProductName)));
const quantityValues = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.Items.Select(x => x.QuantitySold)));
const revenueValues = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.Items.Select(x => x.Revenue)));
const ctx = document.getElementById('myChart').getContext('2d');
new Chart(ctx, {
    type: 'bar',
    data: {
        labels: labels,
        datasets: [
            { label: 'Số lượng bán (Ly)', data: quantityValues, yAxisID: 'y' },
            { label: 'Doanh thu (VNĐ)', data: revenueValues, yAxisID: 'y1' }
        ]
    }
});
```

### 3.2. Hệ thống biểu đồ phân tích doanh thu chuyên sâu (`/RevenueReport`)
Tại trang `Views/RevenueReport/Index.cshtml` và `wwwroot/js/revenue-report.js`:
- **Biểu đồ kết hợp (Bar + Line Chart - `#chart-daily-trend`):** Biểu diễn biến động doanh thu và số lượng đơn hàng theo từng ngày trong khoảng thời gian đã chọn.
- **Biểu đồ Doughnut (`#chart-category-donut`):** Biểu diễn tỷ trọng cơ cấu doanh thu theo từng nhóm danh mục đồ uống.
- **Biểu đồ Sparkline trên 4 thẻ KPI:** Trực quan hóa xu hướng tăng trưởng ngay trên từng chỉ số tổng hợp.

---

## KẾT LUẬN
Nhóm 6 đã hoàn thành đầy đủ 100% các yêu cầu từ **Bài 1 đến Bài 3 của LAB 6**, tích hợp liền mạch trên hệ thống menu Quản lý của PolyCafe.

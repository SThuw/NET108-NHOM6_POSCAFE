# KẾ HOẠCH PHÂN CHIA CÔNG VIỆC THỰC HIỆN LAB 6 — DỰ ÁN .NET 1 (NET108)
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyCafe - POS Cafe)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)  
**Nhóm trưởng điều phối:** Nguyễn Kim Song Thư (MSSV: TB02272)

---

## 🎯 MỤC TIÊU LAB 6
✓ Xây dựng và bổ sung các chức năng cho quản lý hóa đơn (Danh sách & Xem chi tiết).  
✓ Xây dựng các chức năng thống kê (Top 5 thức uống bán chạy & Thống kê doanh thu theo khoảng thời gian).  
✓ Tạo biểu đồ trực quan hóa dữ liệu thống kê bằng thư viện **Chart.js** kết hợp AI.  
✓ Đảm bảo **5 thành viên làm việc song song, độc lập trên 5 nhánh Git riêng biệt, 100% không bị xung đột (conflict) code**.

---

## 📋 BẢNG PHÂN CHIA NHIỆM VỤ CHI TIẾT (5 THÀNH VIÊN)

| STT | Thành viên & MSSV | Vai trò | Nội dung đảm nhiệm trong LAB 6 | File / Thư mục code RIÊNG BIỆT (Không đụng nhau) | Tên nhánh Git riêng |
| :-: | :--- | :---: | :--- | :--- | :--- |
| **1** | **Nguyễn Kim Song Thư**<br>*(TB02272 - Nhóm trưởng)* | Leader / Dashboard & Chart | **Bài 3 (2.0đ):** Tích hợp biểu đồ **Chart.js** (Biểu đồ cột doanh thu & Biểu đồ tròn Top 5 món), viết báo cáo tổng hợp Lab 6 & điều phối duyệt Pull Request trên Git. | • `Controllers/DashboardController.cs`<br>• `Views/Dashboard/Index.cshtml`<br>• `docs/BAO_CAO_LAB6.md` | `feature/thu-lab6-chartjs` |
| **2** | **Nguyễn Trí Nguyễn**<br>*(TB02277)* | Dev 1 / Quản lý Đơn hàng | **Bài 1 - Phần A (2.5đ):** Hiển thị danh sách tất cả hóa đơn ở phía Quản lý, bộ lọc trạng thái (Hoàn tất/Hủy) & lọc theo ngày tạo. | • `Controllers/OrderAdminController.cs`<br>• `Views/OrderAdmin/Index.cshtml`<br>• `Models/ViewModels/OrderAdminViewModels.cs` | `feature/nguyen-lab6-order-list` |
| **3** | **Trần Thị Tuyết Kha**<br>*(TB02214)* | Dev 2 / Chi tiết Đơn hàng | **Bài 1 - Phần B (2.5đ):** Trang xem chi tiết hóa đơn: danh sách món, số lượng, đơn giá, tổng tiền, tên nhân viên tạo đơn, ngày giờ tạo & giao diện xem in bill. | • `Controllers/OrderDetailController.cs`<br>• `Views/OrderDetail/Index.cshtml`<br>• `Views/OrderDetail/_PrintReceipt.cshtml` | `feature/kha-lab6-order-details` |
| **4** | **Ninh Văn Đức**<br>*(TB02291)* | Dev 3 / Thống kê Doanh thu | **Bài 2 - Phần A (1.5đ):** Thống kê tổng doanh thu tùy chọn theo khoảng thời gian (Từ ngày... Đến ngày...), số lượng đơn, doanh thu trung bình mỗi đơn. | • `Controllers/RevenueReportController.cs`<br>• `Views/RevenueReport/Index.cshtml`<br>• `Services/RevenueReportService.cs` | `feature/duc-lab6-revenue-report` |
| **5** | **Nguyễn Lâm Chí Tâm**<br>*(TB02234)* | Dev 4 / Thống kê Top món | **Bài 2 - Phần B (1.5đ):** Thống kê **Top 5 thức uống bán chạy nhất** (theo số lượng ly bán ra và doanh số thu về), bảng xếp hạng đồ uống hot theo thời gian. | • `Controllers/TopSellingController.cs`<br>• `Views/TopSelling/Index.cshtml`<br>• `Services/TopSellingService.cs` | `feature/tam-lab6-top-selling` |

---

## 🛡️ NGUYÊN TẮC VÀNG ĐỂ CODE TRÊN GIT KHÔNG BỊ CONFLICT ("ĐÁNH NHAU")

Để tránh tình trạng 2 người cùng sửa chung 1 file dẫn đến xung đột code:

### 1. Quy tắc về File
- **Mỗi người làm đúng Controller và View của mình:** Tuyệt đối không chỉnh sửa Controller hay View của người khác.
- **KHÔNG sửa các file hệ thống chung:** Các file như `_Layout.cshtml`, `Program.cs`, `site.css` tạm thời **không ai được tự ý sửa**. 
  - Giao diện của bạn cần css gì thì viết riêng thẻ `<style>` trong file View của bạn.
  - Sau khi cả 4 bạn hoàn thành, **Nhóm trưởng (Song Thư)** sẽ là người duy nhất mở `_Layout.cshtml` gắn link menu điều hướng vào hệ thống.

### 2. Quy tắc về Nhánh (Branch) & Lệnh Git
- **Bước 1: Trước khi bắt đầu code (BẮT BUỘC):** Phải kéo code mới nhất từ `main` về máy rồi mới tạo nhánh mới:
  ```bash
  git checkout main
  git pull origin main
  git checkout -b <ten-nhanh-cua-ban>
  ```
  *(Ví dụ Tuyết Kha: `git checkout -b feature/kha-lab6-order-details`)*

- **Bước 2: Trong khi làm việc:** Chỉ code và commit trên nhánh riêng của mình:
  ```bash
  git add .
  git commit -m "feat(lab6): hoan thanh giao dien va chuc nang..."
  git push origin <ten-nhanh-cua-ban>
  ```

- **Bước 3: Sau khi hoàn thành:** Lên giao diện GitHub, bấm vào tab **Pull requests** ➔ Tạo yêu cầu gộp (**Create pull request**) gửi cho Nhóm trưởng Song Thư review và merge vào `main`.

---

## 📱 MẪU TIN NHẮN GỬI VÀO NHÓM CHAT (ZALO / MESSENGER)

Nhóm trưởng có thể copy toàn bộ đoạn tin nhắn dưới đây để gửi cho các thành viên trong nhóm:

> *"Chào mọi người, nhóm mình đã hoàn thành 100% Lab 5 được gộp lên `main` rồi nhé!  
> Đây là kế hoạch phân chia công việc cho **LAB 6** của nhóm mình. Để tránh bị xung đột code trên Git, mỗi người làm Controller, View và Nhánh riêng biệt của mình, tuyệt đối không chỉnh sửa file của người khác nha:*
>
> 1. **Nguyễn Trí Nguyễn:** Phụ trách **Bài 1A - Quản lý danh sách đơn hàng**  
>    - Nhánh: `feature/nguyen-lab6-order-list`  
>    - Code trong: `Controllers/OrderAdminController.cs` và `Views/OrderAdmin/Index.cshtml`
>
> 2. **Trần Thị Tuyết Kha:** Phụ trách **Bài 1B - Xem chi tiết đơn hàng & nhân viên tạo đơn**  
>    - Nhánh: `feature/kha-lab6-order-details`  
>    - Code trong: `Controllers/OrderDetailController.cs` và `Views/OrderDetail/Index.cshtml`
>
> 3. **Ninh Văn Đức:** Phụ trách **Bài 2A - Thống kê doanh thu theo khoảng thời gian**  
>    - Nhánh: `feature/duc-lab6-revenue-report`  
>    - Code trong: `Controllers/RevenueReportController.cs` và `Views/RevenueReport/Index.cshtml`
>
> 4. **Nguyễn Lâm Chí Tâm:** Phụ trách **Bài 2B - Thống kê Top 5 thức uống bán chạy nhất**  
>    - Nhánh: `feature/tam-lab6-top-selling`  
>    - Code trong: `Controllers/TopSellingController.cs` và `Views/TopSelling/Index.cshtml`
>
> 5. **Song Thư:** Phụ trách **Bài 3 - Tích hợp biểu đồ Chart.js**, tổng hợp báo cáo và duyệt merge code trên Git.
>
> ⚠️ **LƯU Ý:** Trước khi gõ lệnh tạo nhánh, mọi người nhớ gõ `git checkout main` và `git pull origin main` để lấy code mới nhất về máy nhé! Chi tiết kế hoạch đã được lưu trong file `docs/KE_HOACH_PHAN_CONG_LAB6.md` trên Git của nhóm."*

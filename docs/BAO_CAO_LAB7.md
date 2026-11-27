# BÁO CÁO THỰC HÀNH LAB 7 — KIỂM THỬ PHẦN MỀM (DỰ ÁN .NET 1)
**Đề tài:** Hệ thống Quản lý Bán hàng Quán Đồ uống (PolyKaFé - POS Cafe)  
**Nhóm thực hiện:** Nhóm 6  
**Lớp / Khóa:** Dự án .NET 1 (NET108 — FPT Polytechnic)  
**Kho lưu trữ GitHub:** [https://github.com/SThuw/NET108-NHOM6_POSCAFE](https://github.com/SThuw/NET108-NHOM6_POSCAFE)  
**Nhóm trưởng:** Nguyễn Kim Song Thư (MSSV: TB02272)

---

## MỤC TIÊU LAB 7
✓ Hiểu đúng vai trò của kiểm thử phần mềm trong vòng đời phát triển hệ thống.  
✓ Phân tích yêu cầu và lập kế hoạch kiểm thử (Test Plan) cho hệ thống PolyKaFé.  
✓ Xây dựng Test Case có cấu trúc, đúng nghiệp vụ và bao phủ yêu cầu (Happy Path & Invalid/Exception Case).  
✓ Thực hiện kiểm thử, phát hiện – ghi nhận – theo dõi lỗi theo quy trình chuẩn.  
✓ Ứng dụng AI trong phân tích, lập kế hoạch kiểm thử và xác định điểm dễ phát sinh lỗi trong ASP.NET Core MVC.

---

## BÀI 1 (3 ĐIỂM): PHÂN TÍCH VÀ XÁC ĐỊNH PHẠM VI KIỂM THỬ

### 1.1. Phạm vi kiểm thử (Test Scope)

#### A. Các chức năng CẦN kiểm thử (In-Scope)
1. **Đăng nhập & Phân quyền (`LoginController`, `EmployeesController`):**
   - Đăng nhập bằng Email/Mật khẩu hợp lệ (Quản lý / Nhân viên), xử lý sai thông tin, tài khoản bị khóa (`IsActive = false`).
   - Phân quyền truy cập: Nhân viên (`Employee`) không được truy cập trang Quản trị (`/Products`, `/Employees`, `/Lab3`, `/RevenueReport`, `/TopSelling`) — phải trả về HTTP `403 Denied`.
   - Quên mật khẩu & Cấp lại mật khẩu qua Email, Đổi mật khẩu cá nhân.
2. **Quản lý đồ uống (`ProductsController`, `CategoriesController`):**
   - CRUD đồ uống (Thêm, Xem chi tiết, Sửa, Xóa / Chuyển trạng thái Ngừng bán nếu đã phát sinh hóa đơn).
   - Tìm kiếm đồ uống theo tên/mã (hỗ trợ tiếng Việt có dấu/không dấu) và lọc theo danh mục, trạng thái.
   - Kiểm tra dữ liệu đầu vào: Giá bán > 0, không nhận số thập phân lẻ đồng VND, chặn upload ảnh không hợp lệ (SVG/sai định dạng).
3. **Quản lý & Xử lý Hóa đơn (`PosController`, `KioskController`, `Lab3Controller`, `OrderDetailController`):**
   - Tạo đơn hàng tại quầy POS và màn hình Kiosk tự phục vụ (chọn món, tăng/giảm số lượng, ghi chú, áp dụng giảm giá VIP theo số lần ghé/SĐT, thuế VAT 5%).
   - Xử lý Idempotency (chống tạo trùng đơn khi bấm thanh toán nhiều lần), cập nhật trạng thái đơn (`Đang chuẩn bị` ➔ `Chờ nhận món` ➔ `Hoàn thành` / `Hủy đơn`).
   - Xem danh sách hóa đơn, lọc theo mã/ngày và xem chi tiết hóa đơn (danh sách món, số lượng, đơn giá, tổng tiền, nhân viên tạo đơn, ngày tạo, in bill 80mm).
4. **Quản lý nhân viên (`EmployeesController`):**
   - CRUD nhân viên (Thêm tài khoản, Sửa vai trò/trạng thái, Khóa tài khoản).
   - Chặn trùng Email, chặn tự khóa tài khoản đang đăng nhập và chặn khóa tài khoản Quản lý cuối cùng.

#### B. Các chức năng KHÔNG nằm trong phạm vi kiểm thử (Out-of-Scope)
- Thanh toán trực tuyến qua cổng thanh toán bên thứ 3 (VNPay / MoMo thật — hệ thống chỉ ghi nhận phương thức thanh toán nội bộ).
- Kiểm thử tải hiệu năng cao (Stress/Load Testing với hàng nghìn người dùng đồng thời trên môi trường Cloud).
- Giao diện trên trình duyệt lỗi thời (Internet Explorer) và ứng dụng di động Native (iOS/Android).
- Máy chủ gửi Email SMTP thực tế của nhà cung cấp bên ngoài (Google SMTP server uptime).

---

### 1.2. Mục tiêu kiểm thử của đợt kiểm thử
- **Đảm bảo tính đúng đắn nghiệp vụ:** Tất cả các luồng cốt lõi (Đăng nhập, Quản lý sản phẩm, Tạo & xử lý hóa đơn, Quản lý nhân sự) hoạt động đúng theo đặc tả SRS và yêu cầu các Lab 1–6.
- **Bảo toàn dữ liệu & tài chính:** Không xảy ra sai lệch tính tiền (tạm tính, giảm giá trước thuế, làm tròn VNĐ, tổng thanh toán) và không mất dữ liệu hóa đơn cũ khi ngừng bán sản phẩm hay khóa nhân viên.
- **Đảm bảo an toàn & phân quyền:** Ngăn chặn truy cập trái phép chức năng Quản lý, chống SQL Injection, XSS và CSRF (`ValidateAntiForgeryToken`).
- **Sẵn sàng nghiệm thu:** Đạt tỷ lệ kiểm thử tự động 100% (`35/35` checks) và không còn lỗi nghiêm trọng (Critical/High) trước khi bàn giao.

---

### 1.3. Phân tích 3 rủi ro kiểm thử ảnh hưởng đến chất lượng hoặc tiến độ

| STT | Rủi ro kiểm thử | Mức độ ảnh hưởng | Giải pháp phòng ngừa & Giảm thiểu |
| :-: | :--- | :---: | :--- |
| **1** | **Rủi ro xung đột Migration & CSDL (`SqlException`):** Nhiều thành viên cùng chỉnh sửa Entity/Seed Data trên Git dẫn đến trùng khóa chính hoặc thiếu cột khi cập nhật DB trên máy kiểm thử. | **Cao** | Bọc kiểm tra `IF NOT EXISTS` và `COL_LENGTH` trong các Migration; luôn chạy `dotnet ef database update` trên máy sạch trước khi test UI. |
| **2** | **Rủi ro sai lệch tính toán tiền & tạo trùng đơn (Race Condition):** Thu ngân hoặc khách Kiosk bấm nút Thanh toán liên tiếp khi mạng chậm gây trùng đơn hàng hoặc sai số làm tròn VNĐ. | **Cao** | Áp dụng `RequestId` (Idempotency Key) ở tầng Server và viết Automated Checks mô phỏng 20 luồng thanh toán đồng thời để kiểm chứng. |
| **3** | **Rủi ro thiếu dữ liệu mẫu / cấu hình môi trường:** Máy kiểm thử chưa cài SQL Server LocalDB hoặc chưa cấu hình SMTP Email làm chức năng Quên mật khẩu không chạy được. | **Trung bình** | Chuẩn bị sẵn `DbInitializer` tự nạp dữ liệu mẫu khi khởi động, cung cấp file `CHAY_WEB.bat` và thông báo rõ trạng thái cấu hình dịch vụ Email. |

---

## BÀI 2 (2 ĐIỂM): LẬP KẾ HOẠCH KIỂM THỬ (TEST PLAN)

### 2.1. Phạm vi kiểm thử
- **Đối tượng:** Ứng dụng Web ASP.NET Core 8 MVC `CafePOSManagement.Web`.
- **Các phân hệ trọng tâm:** Xác thực & Phân quyền (`Login`), Quản lý Thực đơn (`Products`, `Categories`), Bán hàng & Xử lý Hóa đơn (`Pos`, `Kiosk`, `Lab3`, `OrderDetail`), Quản lý Nhân sự (`Employees`) và Thống kê (`RevenueReport`, `TopSelling`).

### 2.2. Phương pháp tiếp cận kiểm thử
Kết hợp song song **2 phương pháp**:
1. **Kiểm thử thủ công (Manual Testing):** Thực hiện kiểm thử hộp đen (Black-box) trực tiếp trên trình duyệt cho giao diện UI/UX, luồng Kiosk tự phục vụ, Responsive mobile/desktop, modal chi tiết hóa đơn và biểu đồ Chart.js.
2. **Kiểm thử tự động (Automation Testing):** Sử dụng project kiểm thử tích hợp có sẵn của nhóm là **`tests/CafePOSManagement.Checks`** (gồm **35 Automated Checks** kiểm tra tìm kiếm không dấu, lọc danh mục, tính tiền thuế/giảm giá trên Server, kiểm tra Idempotency 20 luồng đồng thời, validate ảnh upload...).

### 2.3. Chiến lược kiểm thử
- **Giai đoạn 1 — Unit & Integration Check:** Chạy `dotnet build` và `CafePOSManagement.Checks` sau mỗi lần sửa code backend/service.
- **Giai đoạn 2 — Functional Testing (Theo phân hệ):** Kiểm thử từng chức năng CRUD, phân quyền Manager/Employee và luồng tạo/cập nhật hóa đơn trên trình duyệt.
- **Giai đoạn 3 — Regression Testing (Kiểm thử hồi quy):** Kiểm thử lại toàn bộ luồng chính sau khi gộp nhánh các thành viên vào `main` để đảm bảo không phát sinh lỗi chéo (như lỗi Migration hoặc đứt link điều hướng).

### 2.4. Nguồn lực kiểm thử (Nhân sự, Môi trường, Công cụ)
- **Nhân sự (5 thành viên Nhóm 6):**
  - *Nguyễn Kim Song Thư (Trưởng nhóm):* Điều phối Test Plan, kiểm thử Kiosk/POS, kiểm thử tích hợp toàn hệ thống & hồi quy trên `main`.
  - *Nguyễn Trí Nguyễn:* Kiểm thử danh sách hóa đơn, bộ lọc tìm kiếm & CSDL SQL Server.
  - *Trần Thị Tuyết Kha:* Kiểm thử chi tiết hóa đơn, in biên lai & thực đơn/combo.
  - *Ninh Văn Đức:* Kiểm thử đăng nhập, phân quyền, quản lý nhân viên & thống kê doanh thu.
  - *Nguyễn Lâm Chí Tâm:* Kiểm thử luồng thanh toán POS & thống kê Top 5 bán chạy.
- **Môi trường kiểm thử:** Windows 11, .NET 8 SDK, SQL Server LocalDB (`MSSQLLocalDB`), trình duyệt Google Chrome / Microsoft Edge (cổng `http://localhost:5088`).
- **Công cụ sử dụng:** Visual Studio / VS Code, Git & GitHub Pull Requests, Chrome DevTools (kiểm tra Console/Network), `CafePOSManagement.Checks` (Automation Test Runner).

### 2.5. Tiêu chí kết thúc kiểm thử (Exit Criteria)
- 100% Test Case đã xây dựng được thực thi.
- `dotnet build CafePOSManagement.sln` đạt **0 Warning, 0 Error**.
- Bộ kiểm thử tự động `CafePOSManagement.Checks` đạt **35/35 PASS**.
- Không còn lỗi mức độ **Critical** hoặc **High** chưa được khắc phục.

---

## BÀI 3A (2 ĐIỂM): XÂY DỰNG TEST CASE CHO CHỨC NĂNG TẠO VÀ XỬ LÝ HÓA ĐƠN

| Test Case ID | Unit to Test | Test Data | Steps to Execute | Expected Result | Loại |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **TC_ORD_01** | Tạo hóa đơn tại quầy POS (`PosController.Checkout`) | • Món: `Cà phê sữa đá` (SL: 2, Giá: 45.000đ)<br>• Giảm giá: `SALE10` (10%)<br>• Thuế VAT: 5%<br>• Thanh toán: `Tiền mặt` (`cash`)<br>• Loại đơn: `Mang đi` (`pickup`) | 1. Đăng nhập tài khoản Thu ngân/Quản lý.<br>2. Chọn 2 ly `Cà phê sữa đá` thêm vào giỏ.<br>3. Chọn mã giảm `SALE10`, hình thức `Tiền mặt`.<br>4. Bấm **Thanh toán**. | Hệ thống tạo hóa đơn mới (VD: `#004`), lưu CSDL với: Tạm tính = `90.000₫`, Giảm = `9.000₫`, VAT (5% của 81.000) = `4.050₫`, Tổng = `85.050₫`, ghi nhận đúng nhân viên tạo đơn và hiển thị hóa đơn in. | Happy Path |
| **TC_ORD_02** | Tạo hóa đơn Kiosk theo hạng thành viên (`Kiosk` + `PosController`) | • SĐT: `0901234567` (Khách mới - lần 1)<br>• Món: `Matcha Latte` (SL: 1, Giá: 55.000đ)<br>• Ghi chú: `Ít đá, ít đường` | 1. Mở `/Kiosk`, nhập SĐT `0901234567` và bấm bắt đầu đặt món.<br>2. Chọn `Matcha Latte`, nhập ghi chú `Ít đá, ít đường`.<br>3. Bấm **Thanh toán**. | Hệ thống tự động áp dụng ưu đãi `Giảm 5% (Thành viên mới)`, thuế 5%, không cho khách tự sửa mã giảm giá/thuế, tạo đơn thành công kèm mã số thứ tự Kiosk. | Happy Path |
| **TC_ORD_03** | Cập nhật trạng thái & xem chi tiết đơn (`Lab3Controller`, `OrderDetailController`) | • Hóa đơn `#004` trạng thái `Đang chuẩn bị` | 1. Vào `/Lab3?tab=orders`.<br>2. Bấm nút `Chờ nhận` rồi bấm `Hoàn thành` ở đơn `#004`.<br>3. Bấm nút **Chi tiết** để mở `/OrderDetail/4`. | Trạng thái đơn cập nhật thành `Hoàn thành` trong CSDL; trang `/OrderDetail/4` hiển thị đầy đủ danh sách món, số lượng, đơn giá, tổng tiền, tên nhân viên tạo đơn và ngày giờ tạo. | Happy Path |
| **TC_ORD_04** | Tạo hóa đơn khi giỏ hàng trống / số lượng không hợp lệ (`OrderService.Checkout`) | • Giỏ hàng: `[]` (rỗng) hoặc Số lượng: `0` / `-2` | 1. Mở `/Pos`, để giỏ hàng trống (hoặc gửi API `POST /Pos/Checkout` với `Items = []` / `Quantity = -2`).<br>2. Bấm **Thanh toán**. | Hệ thống từ chối xử lý (HTTP `400 Bad Request`), hiển thị thông báo lỗi hợp lệ ("Giỏ hàng trống" / "Số lượng không hợp lệ"), không ghi bản ghi rác vào bảng `Orders`. | Invalid / Exception |
| **TC_ORD_05** | Chống tạo trùng đơn khi bấm thanh toán nhiều lần (`Idempotency`) | • Cùng `RequestId` (GUID) gửi đồng thời 20 request `Checkout` | 1. Tạo 1 giỏ hàng hợp lệ kèm `RequestId = guid-A`.<br>2. Gửi đồng thời 20 request `Checkout` cùng `guid-A` tới Server. | Hệ thống chỉ tạo **duy nhất 1 hóa đơn** trong CSDL, 19 request còn lại trả về kết quả của hóa đơn đã tạo, không bị trừ/tính doanh thu 20 lần. | Invalid / Exception |
| **TC_ORD_06** | Tạo hóa đơn với sản phẩm ngừng bán hoặc mã giảm giá không tồn tại | • Món: `p1` (`IsAvailable = false`)<br>• Mã giảm giá: `FAKE99` | 1. Gửi yêu cầu `Checkout` chứa sản phẩm đang ở trạng thái `Ngừng bán` hoặc mã giảm giá `FAKE99`. | Server chặn giao dịch, trả về thông báo lỗi sản phẩm ngừng phục vụ / mã giảm giá không hợp lệ, không phát sinh đơn hàng. | Invalid / Exception |

---

## BÀI 3B (2 ĐIỂM): THỰC HIỆN KIỂM THỬ VÀ GHI NHẬN LỖI

### 3.1. Báo cáo ghi nhận lỗi thực tế trong quá trình kiểm thử hệ thống PolyKaFé

#### 🐞 Lỗi 1 (BUG_01): Lỗi văng ngoại lệ SQL trùng khóa chính khi chạy Migration CSDL
- **Mô tả lỗi:** Khi chạy cập nhật cơ sở dữ liệu (`dotnet ef database update`) trên môi trường đã có sẵn dữ liệu danh mục từ `Program.cs`, hệ thống bị văng lỗi SQL `Violation of PRIMARY KEY constraint 'PK_Categories'` ở giá trị `'matcha'`, làm kẹt luôn Migration thêm cột `CreatedByUserId` vào bảng `Orders`.
- **Steps to Reproduce (Các bước tái hiện):**
  1. Khởi chạy web lần đầu để `Program.cs` tự khởi tạo DB và chèn sẵn 3 danh mục `matcha`, `milktea`, `combo`.
  2. Kéo code mới chứa Migration `AddKhaMenuAndUniqueImages` và `AddOrderCreatedByUser`.
  3. Chạy lệnh `dotnet ef database update --project src/CafePOSManagement.Web`.
- **Actual Result:** Hệ thống dừng giữa chừng với lỗi `Cannot insert duplicate key in object 'dbo.Categories'`, bảng `Orders` không có cột `CreatedByUserId`.
- **Expected Result:** Migration tự kiểm tra nếu danh mục/sản phẩm đã tồn tại thì bỏ qua, chạy thông suốt đến cuối để thêm cột `CreatedByUserId` vào `Orders`.
- **Mức độ nghiêm trọng (Severity):** **Critical (Nghiêm trọng)** — Làm gián đoạn truy vấn bảng `Orders` và các trang Chi tiết hóa đơn/Thống kê. *(Đã khắc phục bằng `IF NOT EXISTS` và `COL_LENGTH` trong các Migration).*

#### 🐞 Lỗi 2 (BUG_02): Lỗi 404 Not Found khi bấm nút "Danh sách hóa đơn" tại trang Chi tiết hóa đơn
- **Mô tả lỗi:** Trên trang Chi tiết hóa đơn (`/OrderDetail/{id}`), nút điều hướng quay lại danh sách hóa đơn trỏ sai tên Controller dẫn đến trang báo lỗi không tìm thấy (404).
- **Steps to Reproduce (Các bước tái hiện):**
  1. Đăng nhập tài khoản Quản lý.
  2. Truy cập trang chi tiết một hóa đơn bất kỳ (VD: `/OrderDetail/1`).
  3. Bấm vào nút **"Danh sách hóa đơn"** ở góc trên bên phải.
- **Actual Result:** Trình duyệt điều hướng tới `/OrderAdmin/Index` và hiển thị lỗi **HTTP 404 Not Found**.
- **Expected Result:** Trình duyệt điều hướng về đúng trang danh sách hóa đơn của hệ thống tại `/Lab3?tab=orders`.
- **Mức độ nghiêm trọng (Severity):** **Medium (Trung bình)** — Ảnh hưởng luồng trải nghiệm người dùng khi xem hóa đơn. *(Đã khắc phục trong `Views/OrderDetail/Index.cshtml`).*

#### 🐞 Lỗi 3 (BUG_03): Khách hàng Kiosk có thể tự chọn mã giảm giá và thay đổi thuế VAT
- **Mô tả lỗi:** Ở màn hình đặt món Kiosk dành cho khách tự phục vụ, ô chọn mã giảm giá và thuế VAT đang hiển thị cho phép khách tự ý chọn mức giảm cao nhất mà không cần kiểm tra hạng thành viên.
- **Steps to Reproduce (Các bước tái hiện):**
  1. Mở màn hình `/Kiosk` mà không nhập SĐT (hoặc nhập SĐT bất kỳ).
  2. Vào màn hình đặt món, chọn món và mở khung giỏ hàng.
  3. Bấm vào dropdown Mã giảm giá và Thuế để đổi giá trị.
- **Actual Result:** Khách hàng tự chọn được mã giảm giá 20% và thuế 0%.
- **Expected Result:** Hệ thống mặc định thuế VAT 5%, khóa không cho khách tự chọn mã giảm giá; tự động nhận diện hạng thành viên theo số lần ghé của SĐT (Mới 5%, VIP 10%, VVIP 15%) và ẩn ô ưu đãi với khách vãng lai.
- **Mức độ nghiêm trọng (Severity):** **High (Cao)** — Ảnh hưởng trực tiếp đến doanh thu và nghiệp vụ bán hàng. *(Đã khắc phục tại `pos.js` và `Pos/Index.cshtml`).*

---

### 3.2. Mô tả vòng đời của lỗi (Bug Life Cycle)
Trong dự án PolyKaFé, mỗi lỗi từ lúc Tester phát hiện đến khi hoàn tất tuân theo quy trình chuẩn **6 trạng thái**:

```text
[1. New (Mới)] ──> [2. Assigned (Đã giao)] ──> [3. Open / In Progress (Đang sửa)]
                                                            │
                                                            ▼
[6. Closed (Đóng lỗi)] <── [5. Verified (Đã kiểm tra lại)] <── [4. Fixed (Đã sửa)]
         ▲                              │
         └──── (Nếu vẫn lỗi) ──> [Reopened (Mở lại)]
```

1. **New (Mới phát hiện):** Tester phát hiện lỗi khi chạy test, ghi nhận đầy đủ mô tả, bước tái hiện, ảnh chụp màn hình và mức độ Severity.
2. **Assigned (Đã phân công):** Nhóm trưởng đánh giá mức độ ưu tiên và giao lỗi cho thành viên phụ trách module đó (qua GitHub Issues/nhóm chat).
3. **In Progress (Đang khắc phục):** Developer tái hiện lỗi ở local trên nhánh `feature/...`, tìm nguyên nhân và viết code sửa lỗi.
4. **Fixed (Đã sửa):** Developer commit bản sửa lỗi kèm test kiểm chứng và tạo Pull Request gửi lại.
5. **Verified / Retest (Kiểm thử lại):** Tester kéo code về chạy lại đúng Test Case bị lỗi trước đó và kiểm thử hồi quy các phần liên quan. Nếu lỗi vẫn còn ➔ chuyển về **Reopened** để sửa tiếp.
6. **Closed (Đóng lỗi):** Xác nhận lỗi đã được xử lý triệt để, 100% test pass ➔ Merge vào `main` và đóng ghi nhận lỗi.

---

## BÀI 4 (1 ĐIỂM): ỨNG DỤNG AI TRONG PHÂN TÍCH VÀ XÂY DỰNG KỊCH BẢN KIỂM THỬ

### 4.1. Prompt 1 — Phân tích và xác định phạm vi kiểm thử
> **Prompt:**  
> *"Tôi đang đóng vai trò QA/Tester cho hệ thống web quản lý quán cà phê PolyKaFé viết bằng ASP.NET Core 8 MVC. Hệ thống gồm các phân hệ: Đăng nhập & phân quyền (Manager/Employee), Quản lý đồ uống (CRUD, lọc, tìm kiếm), Quản lý & xử lý hóa đơn (POS, Kiosk, xem chi tiết), Quản lý nhân viên, và Thống kê (Doanh thu, Top 5 món, biểu đồ Chart.js). Hãy giúp tôi phân tích chi tiết: (1) Danh sách chức năng In-Scope cần kiểm thử kỹ, (2) Các yếu tố Out-of-Scope nên loại trừ, (3) Mục tiêu chất lượng của đợt kiểm thử, và (4) 4 rủi ro lớn nhất có thể xảy ra khi tích hợp nhiều nhánh Git của 5 thành viên kèm phương án phòng ngừa."*

### 4.2. Prompt 2 — Lập kế hoạch kiểm thử (sử dụng tính năng Canvas của AI)
> **Prompt:**  
> *"Hãy bật chế độ Canvas để cùng tôi soạn thảo một bản Kế hoạch Kiểm thử (Test Plan) chuyên nghiệp cho dự án PolyKaFé (.NET 8 MVC + SQL Server LocalDB). Nội dung cần trình bày rõ theo cấu trúc bảng biểu gồm: Phạm vi kiểm thử, Chiến lược kết hợp Manual Testing trên trình duyệt và Automation Testing (dựa trên bộ 35 checks của project CafePOSManagement.Checks), Phân bổ nguồn lực kiểm thử cho 5 thành viên Nhóm 6, Môi trường & Công cụ (Chrome DevTools, EF Core, GitHub PR), và Tiêu chí kết thúc kiểm thử (Exit Criteria) rõ ràng bằng số liệu đo lường được."*

### 4.3. Prompt 3 — Xác định những vị trí hay xảy ra lỗi của hệ thống .NET Core MVC
> **Prompt:**  
> *"Dựa trên kiến trúc ASP.NET Core 8 MVC sử dụng Entity Framework Core, Cookie Authentication và Razor Views của hệ thống PolyKaFé, hãy liệt kê các vị trí 'điểm nóng' thường xuyên phát sinh lỗi nhất (như Model Binding, Validation tiền tệ/ngày tháng, Anti-Forgery Token ở các lời gọi AJAX `fetch`, phân quyền `[Authorize]`, EF Core Migration trùng dữ liệu seed, và Race Condition khi chốt đơn hàng). Với mỗi vị trí, hãy chỉ rõ dấu hiệu nhận biết lỗi và cách viết Test Case để bắt được lỗi đó."*

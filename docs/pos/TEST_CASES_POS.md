# Test Cases – POS / G5 và Y2

> Đây là kịch bản kiểm thử cần thực thi trên môi trường chạy. Chỉ đánh dấu Pass sau khi đã chạy và lưu ảnh/log minh chứng.

| ID | Nhóm | Dữ liệu / thao tác | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|---|
| POS-01 | Client | Nhấn Ghi nhận đơn khi giỏ trống | Không gửi request; hiện thông báo giỏ trống | Chưa chạy | Pending |
| POS-02 | Client | Thêm một món hợp lệ | Món xuất hiện, số lượng = 1 | Chưa chạy | Pending |
| POS-03 | Client | Bấm giảm khi số lượng = 1 | Không có số lượng 0/âm; dòng món được xử lý đúng theo UI | Chưa chạy | Pending |
| POS-04 | Client | Tăng số lượng đến 99 rồi tăng tiếp | Không vượt quá 99 | Chưa chạy | Pending |
| POS-05 | Client | Xóa một món | Món biến mất, tổng tiền tính lại | Chưa chạy | Pending |
| POS-06 | Client | Xóa toàn bộ giỏ | Giỏ rỗng, tổng tiền bằng 0 | Chưa chạy | Pending |
| POS-07 | Server | POST Checkout với RequestId = Guid.Empty | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-08 | Server | POST Checkout với Quantity = 0 hoặc -1 | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-09 | Server | POST Checkout với Quantity = 100 | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-10 | Server | POST Checkout với ProductId không tồn tại | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-11 | Server | POST Checkout với món đã ngừng bán | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-12 | Server | Gửi hai lần cùng RequestId và cùng nội dung | Chỉ có một đơn trong Orders; lần hai trả đơn đã có | Chưa chạy | Pending |
| POS-13 | Server | Dùng RequestId cũ nhưng nội dung khác trên fallback in-memory | Bị từ chối | Chưa chạy | Pending |
| POS-14 | Server | Dùng mã giảm giá không tồn tại | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-15 | Server | Dùng VAT ngoài [0, 5, 8, 10] | HTTP 400, không tạo đơn | Chưa chạy | Pending |
| POS-16 | Sửa đơn | Tạo đơn, mở hóa đơn, nhấn Sửa/Bổ sung | Giỏ được nạp lại; nút chuyển thành Cập nhật đơn | Chưa chạy | Pending |
| POS-17 | Sửa đơn | Thêm/bớt món rồi cập nhật | Cùng Order Id/OrderNumber; OrderItems và tổng tiền thay đổi | Chưa chạy | Pending |
| POS-18 | Sửa đơn | Gửi lại cùng RequestId cập nhật | Không tạo đơn mới; trả trạng thái đã cập nhật | Chưa chạy | Pending |
| POS-19 | Sửa đơn | Cập nhật đơn không tồn tại hoặc đã hủy | HTTP 400, dữ liệu không đổi | Chưa chạy | Pending |
| POS-20 | Tính tiền | So sánh subtotal, discount, VAT, total với dữ liệu Server | Tổng = subtotal - discount + VAT; làm tròn nhất quán | Chưa chạy | Pending |

## Ảnh minh chứng cần chụp
1. POS có thực đơn và giỏ hàng với ít nhất hai món.
2. Giỏ hàng trống và thông báo không cho thanh toán.
3. Giỏ hàng sau khi tăng/giảm/xóa món.
4. Hóa đơn sau khi ghi nhận thành công.
5. Hóa đơn sau khi sửa/bổ sung món, thể hiện giữ nguyên mã đơn.
6. Request thất bại do Quantity = 0 hoặc ProductId không tồn tại (Network/Response hoặc log kiểm thử).
7. Gửi lại RequestId và đối chiếu chỉ có một bản ghi trong Orders.

Không chụp dữ liệu cá nhân thật hoặc thông tin đăng nhập vào ảnh báo cáo.

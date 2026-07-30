# 📋 CHECKLIST TÍNH NĂNG DỰ ÁN WebMTTQ

> **Dự án:** Cổng Thông Tin Điện Tử MTTQ Phường Tân Định  
> **Công nghệ:** ASP.NET Core MVC (.NET 8/9) + Entity Framework Core + SQL Server

---

## I. 🏠 GIAO DIỆN CÔNG KHAI (Public Site)

### 1. Trang chủ (Home)
- [x] Trang chủ hiển thị thông tin cơ bản
- [x] Trang "Đang xây dựng" (UnderConstruction)
- [x] Trang Chính sách bảo mật (Privacy)
- [x] Trang báo lỗi (Error)

### 2. Cổng Thông Tin An Sinh Xã Hội
- [x] Hiển thị danh sách thông tin nhận ủng hộ
- [x] Thống kê tổng số lượt ủng hộ (có cache 10 phút)
- [x] Thống kê tổng số tiền ủng hộ (có cache)
- [x] Ngày cập nhật mới nhất
- [x] Danh sách người ủng hộ có phân trang (10 dòng/trang)
- [x] Hiển thị số dư quỹ vì người nghèo
- [x] Thống kê kết quả chăm lo (có cache 15 phút):
  - [x] Tổng kinh phí chăm lo
  - [x] Tổng lượt hộ chăm lo
  - [x] Tổng hoạt động chăm lo
  - [x] Tổng đơn vị tham gia
  - [x] Thống kê theo tháng (biểu đồ)
  - [x] Thống kê theo nhóm đơn vị
- [x] Bản đồ an sinh xã hội (có cache 30 phút):
  - [x] Hiển thị danh sách địa điểm trên bản đồ
  - [x] Tổng số địa điểm
  - [x] Phân loại nhóm đơn vị
  - [x] Dữ liệu JSON cho Google Maps
  - [x] Hình ảnh thực tế (base64)
- [x] Form gửi yêu cầu trợ giúp (có validation)
- [x] Tải danh sách ủng hộ bằng AJAX (Partial View)

### 3. Trang Tin tức (News)
- [x] Danh sách tin tức theo chuyên mục
- [x] Phân trang

### 4. Trang Giới thiệu (About)
- [x] Trang giới thiệu chung

### 5. Hộp thư Góp ý (Góp ý công khai)
- [x] Form gửi góp ý
- [x] Upload tệp đính kèm (minh chứng)
- [x] Validation dữ liệu đầu vào
- [x] Thông báo thành công

### 6. Đăng nhập (Auth)
- [x] Trang đăng nhập
- [x] Kiểm tra session tự động chuyển hướng
- [x] Xác thực tài khoản từ database
- [x] Lưu thông tin session (AdminLoggedIn, UserId, HoTen, TenDangNhap)
- [x] Đăng xuất (xóa session)

---

## II. 🔐 GIAO DIỆN QUẢN TRỊ (Admin Panel)

### 1. Dashboard
- [x] Trang tổng quan admin
- [x] Sidebar điều hướng đầy đủ
- [x] Hiển thị tên người dùng đã đăng nhập
- [x] Nút đăng xuất
- [x] Nút xem trang chủ
- [x] Responsive sidebar (mobile toggle)
- [x] Phân cách nhóm menu

### 2. Quản lý Thông tin ủng hộ
- [x] Danh sách thông tin ủng hộ
- [x] Thêm mới thông tin ủng hộ
- [x] Chỉnh sửa thông tin ủng hộ
- [x] Xóa thông tin ủng hộ
- [x] Anti-forgery token bảo vệ

### 3. Quản lý Danh sách ủng hộ
- [x] Danh sách ủng hộ (sắp xếp ngày mới nhất)
- [x] Thêm mới người ủng hộ
- [x] Chỉnh sửa thông tin ủng hộ
- [x] Xóa người ủng hộ
- [x] Xử lý lỗi DbUpdateConcurrencyException
- [x] Anti-forgery token bảo vệ

### 4. Quản lý Số dư Quỹ
- [x] Danh sách số dư quỹ (sắp xếp ngày cập nhật)
- [x] Thêm mới số dư quỹ (tự động gán ngày hiện tại)
- [x] Chỉnh sửa số dư quỹ (tự động cập nhật ngày)
- [x] Xóa số dư quỹ
- [x] Anti-forgery token bảo vệ

### 5. Quản lý Kết quả Chăm lo
- [x] Danh sách kết quả chăm lo (sắp xếp theo tháng)
- [x] Thêm mới kết quả chăm lo (tự động gán ngày cập nhật)
- [x] Chỉnh sửa kết quả chăm lo
- [x] Xóa kết quả chăm lo
- [x] Anti-forgery token bảo vệ

### 6. Quản lý Bản đồ An sinh
- [x] Danh sách địa điểm bản đồ (lọc xóa mềm)
- [x] Thêm mới địa điểm:
  - [x] Upload hình ảnh thực tế
  - [x] Làm tròn tọa độ (6 số thập phân)
  - [x] Xóa cache bản đồ sau khi thêm
- [x] Chỉnh sửa địa điểm:
  - [x] Giữ nguyên hình ảnh cũ nếu không upload mới
  - [x] Xóa cache bản đồ sau khi sửa
- [x] Xóa mềm địa điểm (DaXoa = true)
- [x] Xóa cache bản đồ sau khi xóa
- [x] Anti-forgery token bảo vệ

### 7. Quản lý Yêu cầu Trợ giúp
- [x] Danh sách yêu cầu trợ giúp (lọc xóa mềm)
- [x] Xem chi tiết & cập nhật trạng thái
- [x] Xóa mềm yêu cầu (DaXoa = true)
- [x] Anti-forgery token bảo vệ

### 8. Quản lý Hộp thư Góp ý
- [x] Danh sách góp ý (lọc xóa mềm, sắp xếp mới nhất)
- [x] Xem chi tiết góp ý
- [x] Cập nhật trạng thái xử lý
- [x] Phản hồi nội dung góp ý
- [x] Xóa mềm góp ý (DaXoa = true)
- [x] Thông báo thành công (TempData)
- [x] Anti-forgery token bảo vệ

### 9. BaseAdminController (Lớp nền tảng)
- [x] Kiểm tra session đăng nhập trước mỗi action
- [x] Tự động chuyển hướng về trang login nếu chưa đăng nhập

---

## III. ⚙️ KIẾN TRÚC & KỸ THUẬT

### 1. Cấu hình hệ thống
- [x] Cấu hình Entity Framework Core + SQL Server
- [x] Cấu hình Session (30 phút timeout, HttpOnly)
- [x] Cấu hình Memory Cache (IMemoryCache)
- [x] Cấu hình Routing mặc định
- [x] Cấu hình HTTPS Redirect
- [x] Cấu hình Exception Handling (Development/Production)

### 2. Tối ưu hiệu năng
- [x] Sử dụng AsNoTracking() cho truy vấn chỉ đọc
- [x] Cache thống kê ủng hộ (10 phút)
- [x] Cache kết quả chăm lo (15 phút)
- [x] Cache dữ liệu bản đồ (30 phút)
- [x] Cache tổng số dòng danh sách ủng hộ
- [x] Phân trang danh sách ủng hộ (10 dòng/trang)
- [x] Tải dữ liệu phân trang bằng AJAX (Partial View)

### 3. Bảo mật
- [x] Xác thực bằng Session
- [x] ValidateAntiForgeryToken trên các form POST
- [x] Kiểm tra ModelState.IsValid trước khi xử lý
- [x] Xóa mềm dữ liệu (Soft Delete) thay vì xóa vật lý
- [x] Kiểm tra tồn tại dữ liệu trước khi thao tác
- [x] Xử lý lỗi DbUpdateConcurrencyException

### 4. Database Models (Entity Framework)
- [x] BaiViet (Bài viết)
- [x] CauHinhHeThong (Cấu hình hệ thống)
- [x] ChuongTrinhHoTro (Chương trình hỗ trợ)
- [x] ChuyenMuc (Chuyên mục)
- [x] DanhMucQuy (Danh mục quỹ)
- [x] DanhSachUngHo (Danh sách ủng hộ)
- [x] DiaDiemBanDo (Địa điểm bản đồ)
- [x] DoanTheToChuc (Đoàn thể tổ chức)
- [x] DonXinHoTro (Đơn xin hỗ trợ)
- [x] HopThuGopY (Hộp thư góp ý)
- [x] KetQuaChamLo (Kết quả chăm lo)
- [x] KhoanDongGop (Khoản đóng góp)
- [x] LuotTraoTang (Lượt trao tặng)
- [x] NguoiCanGiupDo (Người cần giúp đỡ)
- [x] NguoiDanCanTroGiup (Người dân cần trợ giúp)
- [x] NguoiDung (Người dùng)
- [x] NhaHaoTam (Nhà hảo tâm)
- [x] NhatKyHeThong (Nhật ký hệ thống)
- [x] SoDuQuyViNguoiNgheo (Số dư quỹ vì người nghèo)
- [x] ThanhPhanGiaoDien (Thành phần giao diện)
- [x] ThongTinNhanUngHo (Thông tin nhận ủng hộ)
- [x] VaiTro (Vai trò)
- [x] VanBanTaiLieu (Văn bản tài liệu)

### 5. Migrations
- [x] Migration: ThongTinUngHo
- [x] Migration: DanhSachUngHo
- [x] Migration: AddNgayThucHienToDiaDiemBanDo

---

## IV. 🎨 GIAO DIỆN NGƯỜI DÙNG

### 1. Admin Layout
- [x] Sidebar cố định bên trái
- [x] Topbar hiển thị thông tin người dùng
- [x] Font chữ Playfair Display + Be Vietnam Pro
- [x] Font Awesome icons
- [x] Responsive design (mobile sidebar toggle)
- [x] Màu sắc chủ đạo (đỏ - vàng đặc trưng)
- [x] Hiệu ứng active menu

### 2. CSS Admin
- [x] File admin.css riêng biệt
- [x] CSS variables (màu sắc, kích thước)
- [x] Responsive layout
- [x] Hiệu ứng hover, active

---

## V. 📋 TỔNG HỢP

| Hạng mục | Số lượng |
|----------|----------|
| Controllers | 13 |
| Models (DbSet) | 24 |
| Views (Admin) | 20+ |
| Migrations | 3 |
| Public Pages | 6+ modules |
| Admin Modules | 8 modules |

---

> **Cập nhật lần cuối:** 24/07/2026
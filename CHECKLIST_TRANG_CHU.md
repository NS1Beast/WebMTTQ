# Checklist - Thiết kế Trang chủ & Cài đặt Trang chủ Admin

## 1. Models & Database
- [x] Create `TrangChuMuc` model (HomepageSection)
- [x] Update `Banner` model (add MoTa, HieuUng, TocDo, ThoiGianDung fields)
- [x] Update `DataMTTQContext` (add DbSet for TrangChuMuc, update Banner config)
- [x] Update `HomePageViewModel` (add sections)

## 2. Controllers
- [x] Create `AdminTrangChuController` (CRUD for homepage sections)
- [x] Update `AdminBannerController` (add banner settings)
- [x] Update `HomeController` (load sections and banner settings)

## 3. Admin Views
- [x] Create `Views/Admin/TrangChu/Index.cshtml`
- [x] Create `Views/Admin/TrangChu/Create.cshtml`
- [x] Create `Views/Admin/TrangChu/Edit.cshtml`
- [x] Update `Views/Admin/Banner/Index.cshtml`
- [x] Update `Views/Admin/Banner/Create.cshtml`
- [x] Update `Views/Admin/Banner/Edit.cshtml`
- [x] Update `Views/Shared/_AdminLayout.cshtml` (add menu link)

## 4. Homepage (Frontend)
- [x] Redesign `Views/Home/Index.cshtml` with beautiful sections
- [x] Optimize banner slider (smooth transitions, advanced controls)
- [x] Add scroll-triggered animations

## 5. CSS & JavaScript
- [x] Add homepage section styles to `wwwroot/css/mttq.css`
- [x] Add admin trangchu styles to `wwwroot/css/admin.css`
- [x] Add enhanced banner JavaScript
- [x] Add scroll animation library/scripts

## 6. Migration
- [x] Create and apply migration for new models
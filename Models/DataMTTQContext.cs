using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

public partial class DataMTTQContext : DbContext
{
    public DataMTTQContext()
    {
    }

    public DataMTTQContext(DbContextOptions<DataMTTQContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiViet> BaiViets { get; set; }
    public virtual DbSet<CauHinhHeThong> CauHinhHeThongs { get; set; }
    public virtual DbSet<ChuongTrinhHoTro> ChuongTrinhHoTros { get; set; }
    public DbSet<ThongTinNhanUngHo> ThongTinNhanUngHos { get; set; }
    public DbSet<SoDuQuyViNguoiNgheo> SoDuQuyViNguoiNgheos { get; set; }
    public DbSet<DanhSachUngHo> DanhSachUngHos { get; set; }
    public DbSet<KetQuaChamLo> KetQuaChamLos { get; set; }

    public  DbSet<NguoiDanCanTroGiup> NguoiDanCanTroGiups { get; set; }
    public virtual DbSet<ChuyenMuc> ChuyenMucs { get; set; }
    public virtual DbSet<DanhMucQuy> DanhMucQuies { get; set; }
    public virtual DbSet<DiaDiemBanDo> DiaDiemBanDos { get; set; }
    public virtual DbSet<DoanTheToChuc> DoanTheToChucs { get; set; }
    public virtual DbSet<DonXinHoTro> DonXinHoTros { get; set; }
    public virtual DbSet<HopThuGopY> HopThuGopies { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<TrangChuMuc> TrangChuMucs { get; set; }
    public DbSet<TrangChuTinTuc> TrangChuTinTucs { get; set; }
    public virtual DbSet<KhoanDongGop> KhoanDongGops { get; set; }
    public virtual DbSet<LuotTraoTang> LuotTraoTangs { get; set; }
    public virtual DbSet<NguoiCanGiupDo> NguoiCanGiupDos { get; set; }
    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }
    public virtual DbSet<NhaHaoTam> NhaHaoTams { get; set; }
    public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }
    public virtual DbSet<MaXacThuc> MaXacThus { get; set; }
    public virtual DbSet<ThanhPhanGiaoDien> ThanhPhanGiaoDiens { get; set; }
    public virtual DbSet<VaiTro> VaiTros { get; set; }
    public virtual DbSet<VaiTroQuyen> VaiTroQuyens { get; set; }
    public virtual DbSet<VanBanTaiLieu> VanBanTaiLieus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=DataMTTQ;Integrated Security=True;TrustServerCertificate=True;Command Timeout=300;");
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // => optionsBuilder.UseSqlServer("Server=DESKTOP-C5LJ9BM\\SQL2025_DEV;Database=DataMTTQ;Integrated Security=True;TrustServerCertificate=True;Command Timeout=300;");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.IdbaiViet).HasName("PK__BaiViet__FC50A207B66391FD");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.LaTinNoiBat).HasDefaultValue(false);
            entity.Property(e => e.LuotXem).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue("BanNhap");

            entity.HasOne(d => d.IdchuyenMucNavigation).WithMany(p => p.BaiViets).HasConstraintName("FK_BaiViet_ChuyenMuc");

            entity.HasOne(d => d.IdnguoiDungNavigation).WithMany(p => p.BaiViets).HasConstraintName("FK_BaiViet_NguoiDung");
        });

        modelBuilder.Entity<CauHinhHeThong>(entity =>
        {
            entity.HasKey(e => e.IdcauHinh).HasName("PK__CauHinhH__DE9A3B7A3F70271C");
        });

        modelBuilder.Entity<ChuongTrinhHoTro>(entity =>
        {
            entity.HasKey(e => e.IdchuongTrinh).HasName("PK__ChuongTr__7B0509A41C0A9DEB");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.TrangThai).HasDefaultValue("DangTrienKhai");
        });

        modelBuilder.Entity<ChuyenMuc>(entity =>
        {
            entity.HasKey(e => e.IdchuyenMuc).HasName("PK__ChuyenMu__1078CD135189713F");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.ThuTu).HasDefaultValue(0);

            entity.HasOne(d => d.IdchuyenMucChaNavigation).WithMany(p => p.InverseIdchuyenMucChaNavigation).HasConstraintName("FK_ChuyenMuc_ChuyenMucCha");
        });

        modelBuilder.Entity<DanhMucQuy>(entity =>
        {
            entity.HasKey(e => e.Idquy).HasName("PK__DanhMucQ__A743077E0D5CACAF");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.TonQuy).HasDefaultValue(0m);
            entity.Property(e => e.TongChi).HasDefaultValue(0m);
            entity.Property(e => e.TongThu).HasDefaultValue(0m);
            entity.Property(e => e.TrangThai).HasDefaultValue("HoatDong");
        });

        modelBuilder.Entity<DiaDiemBanDo>(entity =>
        {
            entity.HasKey(e => e.IddiaDiem).HasName("PK__DiaDiemB__3DD0D654483F5641");

            // ĐÃ THÊM 2 DÒNG NÀY ĐỂ ÉP KIỂU DỮ LIỆU TỌA ĐỘ CHO EF CORE TRÁNH LỖI OUT OF RANGE
            entity.Property(e => e.ViDo).HasColumnType("decimal(12, 8)");
            entity.Property(e => e.KinhDo).HasColumnType("decimal(12, 8)");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.TrangThai).HasDefaultValue("HienThi");

            entity.HasOne(d => d.IddonViNavigation).WithMany(p => p.DiaDiemBanDos).HasConstraintName("FK_DiaDiem_DonVi");
        });

        modelBuilder.Entity<DoanTheToChuc>(entity =>
        {
            entity.HasKey(e => e.IddonVi).HasName("PK__DoanTheT__082302BFBA08A593");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
        });

        modelBuilder.Entity<DonXinHoTro>(entity =>
        {
            entity.HasKey(e => e.Iddon).HasName("PK__DonXinHo__93E3A4198A461737");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.MucDoUuTien).HasDefaultValue("BinhThuong");
            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("ChoXuLy");

            entity.HasOne(d => d.IdnguoiCanGiupNavigation).WithMany(p => p.DonXinHoTros).HasConstraintName("FK_DonXin_NguoiCanGiup");

            entity.HasOne(d => d.IdnguoiXuLyNavigation).WithMany(p => p.DonXinHoTros).HasConstraintName("FK_DonXin_NguoiDung");
        });

        modelBuilder.Entity<HopThuGopY>(entity =>
        {
            entity.HasKey(e => e.IdgopY).HasName("PK__HopThuGo__D232A95EF309C612");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("ChoXuLy");

            entity.HasOne(d => d.IdnguoiXuLyNavigation).WithMany(p => p.HopThuGopies).HasConstraintName("FK_GopY_NguoiDung");
        });

        modelBuilder.Entity<KetQuaChamLo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KetQuaChamLo");

            entity.Property(e => e.KinhPhi).HasPrecision(18, 2);
        });

        modelBuilder.Entity<KhoanDongGop>(entity =>
        {
            entity.HasKey(e => e.IdgiaoDich).HasName("PK__KhoanDon__5E5A4D81942F33FF");

            entity.ToTable("KhoanDongGop", tb => tb.HasTrigger("trg_KhoanDongGop_AuditQuy"));

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayUngHo).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoTien).HasDefaultValue(0m);
            entity.Property(e => e.TrangThai).HasDefaultValue("ThanhCong");

            entity.HasOne(d => d.IdnguoiTiepNhanNavigation).WithMany(p => p.KhoanDongGops)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DongGop_NguoiDung");

            entity.HasOne(d => d.IdnhaHaoTamNavigation).WithMany(p => p.KhoanDongGops)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DongGop_NhaHaoTam");

            entity.HasOne(d => d.IdquyNavigation).WithMany(p => p.KhoanDongGops).HasConstraintName("FK_DongGop_Quy");
        });

        modelBuilder.Entity<LuotTraoTang>(entity =>
        {
            entity.HasKey(e => e.IdtraoTang).HasName("PK__LuotTrao__45A28075254501D6");

            entity.ToTable("LuotTraoTang", tb => tb.HasTrigger("trg_LuotTraoTang_AuditQuy"));

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayPhanBo).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SoTienHoTro).HasDefaultValue(0m);

            entity.HasOne(d => d.IdchuongTrinhNavigation).WithMany(p => p.LuotTraoTangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TraoTang_ChuongTrinh");

            entity.HasOne(d => d.IdnguoiCanGiupNavigation).WithMany(p => p.LuotTraoTangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TraoTang_NguoiCanGiup");

            entity.HasOne(d => d.IdnguoiCapNavigation).WithMany(p => p.LuotTraoTangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TraoTang_NguoiDung");

            entity.HasOne(d => d.IdquyNavigation).WithMany(p => p.LuotTraoTangs).HasConstraintName("FK_TraoTang_Quy");
        });

        modelBuilder.Entity<NguoiCanGiupDo>(entity =>
        {
            entity.HasKey(e => e.IdnguoiCanGiup).HasName("PK__NguoiCan__92DA5CD99DA55672");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.IdnguoiDung).HasName("PK__NguoiDun__FCD7DB090922A357");

            entity.ToTable("NguoiDung", tb => tb.HasTrigger("trg_NguoiDung_UpdateNgay"));

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue("HoatDong");

            // Unique index trên Email nhưng chỉ áp dụng khi Email IS NOT NULL
            // (cho phép nhiều user không có email - NULL)
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("UQ__NguoiDun__A9D105348E5266DA")
                .HasFilter("[Email] IS NOT NULL");

            entity.HasOne(d => d.IdvaiTroNavigation).WithMany(p => p.NguoiDungs).HasConstraintName("FK_NguoiDung_VaiTro");
        });

        modelBuilder.Entity<MaXacThuc>(entity =>
        {
            entity.HasKey(e => e.IdmaXacThuc).HasName("PK__MaXacThuc");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DaSuDung).HasDefaultValue(false);
        });

        modelBuilder.Entity<NhaHaoTam>(entity =>
        {
            entity.HasKey(e => e.IdnhaHaoTam).HasName("PK__NhaHaoTa__9835F40B35BF40E3");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
        });

        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(e => e.IdnhatKy).HasName("PK__NhatKyHe__72F501D28C3EF6A3");

            entity.Property(e => e.ThoiGianTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdnguoiDungNavigation).WithMany(p => p.NhatKyHeThongs).HasConstraintName("FK_NhatKy_NguoiDung");
        });

        modelBuilder.Entity<SoDuQuyViNguoiNgheo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SoDuQuyViNguoiNgheo");

            entity.Property(e => e.TienMat).HasPrecision(18, 2);
            entity.Property(e => e.TienGuiNganHang).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ThanhPhanGiaoDien>(entity =>
        {
            entity.HasKey(e => e.IdthanhPhan).HasName("PK__ThanhPha__90B69433059DBB34");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.ThuTu).HasDefaultValue(0);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.IdvaiTro).HasName("PK__VaiTro__45D3FF49D7A1FDCD");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<VaiTroQuyen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VaiTroQuyen");

            entity.HasIndex(e => new { e.IdVaiTro, e.MaModule }).IsUnique().HasDatabaseName("UQ_VaiTroQuyen_VaiTro_Module");

            entity.HasOne(d => d.IdVaiTroNavigation)
                .WithMany(p => p.VaiTroQuyens)
                .HasForeignKey(d => d.IdVaiTro)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_VaiTroQuyen_VaiTro");
        });

        modelBuilder.Entity<VanBanTaiLieu>(entity =>
        {
            entity.HasKey(e => e.IdvanBan).HasName("PK__VanBanTa__FDADF58BFAC3319C");

            entity.Property(e => e.DaXoa).HasDefaultValue(false);

            entity.HasOne(d => d.IdchuyenMucNavigation).WithMany(p => p.VanBanTaiLieus).HasConstraintName("FK_VanBan_ChuyenMuc");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
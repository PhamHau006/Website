using Microsoft.EntityFrameworkCore;
using ASM_GS.Models;

namespace ASM_GS.Controllers
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public virtual DbSet<MaNhapGiamGia> MaNhapGiamGia { get; set; }
        public virtual DbSet<AnhSanPham> AnhSanPhams { get; set; }
        public virtual DbSet<ChiTietCombo> ChiTietCombos { get; set; }
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual DbSet<Combo> Combos { get; set; }
        public virtual DbSet<DanhGia> DanhGia { get; set; }
        public virtual DbSet<DanhMuc> DanhMucs { get; set; }
        public virtual DbSet<DonHang> DonHangs { get; set; }
        public virtual DbSet<GiamGia> GiamGia { get; set; }
        public virtual DbSet<GioHang> GioHangs { get; set; }
        public virtual DbSet<HoaDon> HoaDons { get; set; }
        public virtual DbSet<KhachHang> KhachHangs { get; set; }
        public virtual DbSet<NhanVien> NhanViens { get; set; }
        public virtual DbSet<SanPham> SanPhams { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
        public virtual DbSet<MaNhapGiamGia> MaNhapGiamGias { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ giữa SanPham và AnhSanPham
            modelBuilder.Entity<AnhSanPham>()
                .HasOne(a => a.MaSanPhamNavigation)
                .WithMany(s => s.AnhSanPhams)
                .HasForeignKey(a => a.MaSanPham);

            // Cấu hình quan hệ giữa SanPham và DanhMuc
            modelBuilder.Entity<SanPham>()
                .HasOne(s => s.MaDanhMucNavigation)
                .WithMany(d => d.SanPhams)
                .HasForeignKey(s => s.MaDanhMuc);
            modelBuilder.Entity<TaiKhoan>()
               .HasOne(tk => tk.MaKhachHangNavigation)
               .WithMany(kh => kh.TaiKhoans)
               .HasForeignKey(tk => tk.MaKhachHang);

            // Cấu hình quan hệ giữa ChiTietComBo và ComBo
            modelBuilder.Entity<ChiTietCombo>()
                .HasOne(cc => cc.MaComboNavigation)
                .WithMany(c => c.ChiTietCombos)
                .HasForeignKey(cc => cc.MaCombo)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Cascade nếu cần

            // Cấu hình quan hệ giữa ChiTietComBo và SanPham
            modelBuilder.Entity<ChiTietCombo>()
                .HasOne(cc => cc.MaSanPhamNavigation)
                .WithMany(sp => sp.ChiTietCombos)
                .HasForeignKey(cc => cc.MaSanPham)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Cascade nếu cần

            // Cấu hình quan hệ giữa GiamGia và HoaDon
            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.MaGiamGiaNavigation) // Thiết lập mối quan hệ từ HoaDon đến GiamGia
                .WithMany(g => g.HoaDons)           // Một GiamGia có thể liên kết với nhiều HoaDon
                .HasForeignKey(h => h.MaGiamGia)    // Khóa ngoại là MaGiamGia
                .OnDelete(DeleteBehavior.SetNull);  // Xóa mềm: nếu GiamGia bị xóa, MaGiamGia sẽ được set null

            // Cấu hình mối quan hệ giữa GiamGia và MaNhapGiamGia
            modelBuilder.Entity<MaNhapGiamGia>()
                .HasOne(m => m.GiamGia) // Một MaNhapGiamGia liên kết với một GiamGia
                .WithMany(g => g.MaNhapGiamGias) // Một GiamGia có nhiều MaNhapGiamGias
                .HasForeignKey(m => m.MaGiamGia) // Khóa ngoại là MaGiamGia
                .OnDelete(DeleteBehavior.Cascade); // Xóa mã giảm giá sẽ xóa cả mã nhập liên quan
            modelBuilder.Entity<ChiTietGioHang>()
        .HasOne(c => c.GioHang)
        .WithMany(g => g.ChiTietGioHangs)
        .HasForeignKey(c => c.MaGioHang);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.SanPham)
                .WithMany()
                .HasForeignKey(c => c.MaSanPham)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.Combo)
                .WithMany()
                .HasForeignKey(c => c.MaCombo)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.HoaDon)
                .WithMany(h => h.ChiTietHoaDons)
                .HasForeignKey(c => c.MaHoaDon);
            base.OnModelCreating(modelBuilder);
            // Seed Data cho DanhMuc
            modelBuilder.Entity<DanhMuc>().HasData(
                new DanhMuc { MaDanhMuc = "DM001", TenDanhMuc = "Dưỡng da", TrangThai = 1 },
                new DanhMuc { MaDanhMuc = "DM002", TenDanhMuc = "Chống nắng", TrangThai = 1 },
                new DanhMuc { MaDanhMuc = "DM003", TenDanhMuc = "Làm sạch", TrangThai = 1 }
            );
            // Seed Data cho SanPham (10 sản phẩm skincare)
            modelBuilder.Entity<SanPham>().HasData(
                new SanPham { MaSanPham = "SP001", TenSanPham = "Kem dưỡng ẩm", Gia = 300000, MaDanhMuc = "DM001", SoLuong = 100, TrangThai = 1 },
                new SanPham { MaSanPham = "SP002", TenSanPham = "Sữa rửa mặt", Gia = 200000, MaDanhMuc = "DM003", SoLuong = 150, TrangThai = 1 },
                new SanPham { MaSanPham = "SP003", TenSanPham = "Nước hoa hồng", Gia = 250000, MaDanhMuc = "DM001", SoLuong = 120, TrangThai = 1 },
                new SanPham { MaSanPham = "SP004", TenSanPham = "Serum dưỡng trắng", Gia = 500000, MaDanhMuc = "DM001", SoLuong = 80, TrangThai = 1 },
                new SanPham { MaSanPham = "SP005", TenSanPham = "Mặt nạ cấp ẩm", Gia = 150000, MaDanhMuc = "DM001", SoLuong = 200, TrangThai = 1 },
                new SanPham { MaSanPham = "SP006", TenSanPham = "Kem chống nắng", Gia = 350000, MaDanhMuc = "DM002", SoLuong = 90, TrangThai = 1 },
                new SanPham { MaSanPham = "SP007", TenSanPham = "Tẩy trang", Gia = 180000, MaDanhMuc = "DM003", SoLuong = 110, TrangThai = 1 },
                new SanPham { MaSanPham = "SP008", TenSanPham = "Tinh chất ngừa mụn", Gia = 400000, MaDanhMuc = "DM001", SoLuong = 70, TrangThai = 1 },
                new SanPham { MaSanPham = "SP009", TenSanPham = "Xịt khoáng", Gia = 220000, MaDanhMuc = "DM002", SoLuong = 90, TrangThai = 1 },
                new SanPham { MaSanPham = "SP010", TenSanPham = "Kem dưỡng da ban đêm", Gia = 450000, MaDanhMuc = "DM001", SoLuong = 50, TrangThai = 1 }
            );

            modelBuilder.Entity<AnhSanPham>().HasData(
               // 4 ảnh cho sản phẩm SP001
               new AnhSanPham { Id = 1, MaSanPham = "SP001", UrlAnh = "img/AnhSanPham/kemduongam1.jpg" },
               new AnhSanPham { Id = 2, MaSanPham = "SP001", UrlAnh = "img/AnhSanPham/kemduongam2.jpg" },
               new AnhSanPham { Id = 3, MaSanPham = "SP001", UrlAnh = "img/AnhSanPham/kemduongam3.jpg" },
               new AnhSanPham { Id = 4, MaSanPham = "SP001", UrlAnh = "img/AnhSanPham/kemduongam4.jpg" },

               // 4 ảnh cho sản phẩm SP002
               new AnhSanPham { Id = 5, MaSanPham = "SP002", UrlAnh = "img/AnhSanPham/suaruamat1.jpg" },
               new AnhSanPham { Id = 6, MaSanPham = "SP002", UrlAnh = "img/AnhSanPham/suaruamat2.jpg" },
               new AnhSanPham { Id = 7, MaSanPham = "SP002", UrlAnh = "img/AnhSanPham/suaruamat3.jpg" },
               new AnhSanPham { Id = 8, MaSanPham = "SP002", UrlAnh = "img/AnhSanPham/suaruamat4.jpg" },

               // 4 ảnh cho sản phẩm SP003
               new AnhSanPham { Id = 9, MaSanPham = "SP003", UrlAnh = "img/AnhSanPham/toner1.png" },
               new AnhSanPham { Id = 10, MaSanPham = "SP003", UrlAnh = "img/AnhSanPham/toner2.png" },
               new AnhSanPham { Id = 11, MaSanPham = "SP003", UrlAnh = "img/AnhSanPham/toner3.png" },
               new AnhSanPham { Id = 12, MaSanPham = "SP003", UrlAnh = "img/AnhSanPham/toner4.png" },

               // 4 ảnh cho sản phẩm SP004
               new AnhSanPham { Id = 13, MaSanPham = "SP004", UrlAnh = "img/AnhSanPham/serumtrang1.jpg" },
               new AnhSanPham { Id = 14, MaSanPham = "SP004", UrlAnh = "img/AnhSanPham/serumtrang2.jpg" },
               new AnhSanPham { Id = 15, MaSanPham = "SP004", UrlAnh = "img/AnhSanPham/serumtrang3.jpg" },
               new AnhSanPham { Id = 16, MaSanPham = "SP004", UrlAnh = "img/AnhSanPham/serumtrang4.jpg" },

               // 4 ảnh cho sản phẩm SP005
               new AnhSanPham { Id = 17, MaSanPham = "SP005", UrlAnh = "img/AnhSanPham/mark1.jpg" },
               new AnhSanPham { Id = 18, MaSanPham = "SP005", UrlAnh = "img/AnhSanPham/mark2.jpg" },
               new AnhSanPham { Id = 19, MaSanPham = "SP005", UrlAnh = "img/AnhSanPham/mark3.jpg" },
               new AnhSanPham { Id = 20, MaSanPham = "SP005", UrlAnh = "img/AnhSanPham/mark4.jpg" },

               // 4 ảnh cho sản phẩm SP006
               new AnhSanPham { Id = 21, MaSanPham = "SP006", UrlAnh = "img/AnhSanPham/kcn1.jpg" },
               new AnhSanPham { Id = 22, MaSanPham = "SP006", UrlAnh = "img/AnhSanPham/kcn2.jpg" },
               new AnhSanPham { Id = 23, MaSanPham = "SP006", UrlAnh = "img/AnhSanPham/kcn3.jpg" },
               new AnhSanPham { Id = 24, MaSanPham = "SP006", UrlAnh = "img/AnhSanPham/kcn4.jpg" },

               // 4 ảnh cho sản phẩm SP007
               new AnhSanPham { Id = 25, MaSanPham = "SP007", UrlAnh = "img/AnhSanPham/taytrang1.jpg" },
               new AnhSanPham { Id = 26, MaSanPham = "SP007", UrlAnh = "img/AnhSanPham/taytrang2.jpg" },
               new AnhSanPham { Id = 27, MaSanPham = "SP007", UrlAnh = "img/AnhSanPham/taytrang3.jpg" },
               new AnhSanPham { Id = 28, MaSanPham = "SP007", UrlAnh = "img/AnhSanPham/taytrang4.jpg" },

               // 4 ảnh cho sản phẩm SP008
               new AnhSanPham { Id = 29, MaSanPham = "SP008", UrlAnh = "img/AnhSanPham/tinhchat1.jpg" },
               new AnhSanPham { Id = 30, MaSanPham = "SP008", UrlAnh = "img/AnhSanPham/tinhchat2.jpg" },
               new AnhSanPham { Id = 31, MaSanPham = "SP008", UrlAnh = "img/AnhSanPham/tinhchat3.jpg" },
               new AnhSanPham { Id = 32, MaSanPham = "SP008", UrlAnh = "img/AnhSanPham/tinhchat4.jpg" },

               // 4 ảnh cho sản phẩm SP009
               new AnhSanPham { Id = 33, MaSanPham = "SP009", UrlAnh = "img/AnhSanPham/xitkhoang1.jpg" },
               new AnhSanPham { Id = 34, MaSanPham = "SP009", UrlAnh = "img/AnhSanPham/xitkhoang2.jpg" },
               new AnhSanPham { Id = 35, MaSanPham = "SP009", UrlAnh = "img/AnhSanPham/xitkhoang3.jpg" },
               new AnhSanPham { Id = 36, MaSanPham = "SP009", UrlAnh = "img/AnhSanPham/xitkhoang4.jpg" },

               // 4 ảnh cho sản phẩm SP010
               new AnhSanPham { Id = 37, MaSanPham = "SP010", UrlAnh = "img/AnhSanPham/bandem1.jpg" },
               new AnhSanPham { Id = 38, MaSanPham = "SP010", UrlAnh = "img/AnhSanPham/bandem2.jpg" },
               new AnhSanPham { Id = 39, MaSanPham = "SP010", UrlAnh = "img/AnhSanPham/bandem3.jpg" },
               new AnhSanPham { Id = 40, MaSanPham = "SP010", UrlAnh = "img/AnhSanPham/bandem4.jpg" }
           );
            modelBuilder.Entity<KhachHang>().HasData(
                new KhachHang
                {
                    MaKhachHang = "KH001",
                    TenKhachHang = "Embo",
                    SoDienThoai = "0123456789",
                    DiaChi = "123 Main St",
                    NgayDangKy = new DateOnly(2023, 1, 15),
                    HinhAnh = null,
                    Cccd = "123456789",
                    NgaySinh = new DateOnly(1990, 1, 1),
                    GioiTinh = true,
                    TrangThai = 1
                },
                new KhachHang
                {
                    MaKhachHang = "KH002",
                    TenKhachHang = "Ember",
                    SoDienThoai = "0987654321",
                    DiaChi = "456 Elm St",
                    NgayDangKy = new DateOnly(2023, 1, 16),
                    HinhAnh = null,
                    Cccd = "987654321",
                    NgaySinh = new DateOnly(1992, 2, 2),
                    GioiTinh = false,
                    TrangThai = 1
                }
        
            );

            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan
                {
                    MaTaiKhoan = "TK001",
                    TenTaiKhoan = "customer1",
                    MatKhau = "123",
                    VaiTro = "Customer",
                    MaKhachHang = "KH001",
                    Email = "customer1@example.com"
                },
                new TaiKhoan
                {
                    MaTaiKhoan = "TK002",
                    TenTaiKhoan = "customer2",
                    MatKhau = "123",
                    VaiTro = "Customer",
                    MaKhachHang = "KH002",
                    Email = "customer2@example.com"
                });


            // Seed Data cho bảng Combo
            modelBuilder.Entity<Combo>().HasData(
                new Combo { MaCombo = "CB001", TenCombo = "Combo Dưỡng Ẩm", MoTa = "Combo gồm các sản phẩm dưỡng ẩm", Gia = 800000, TrangThai = 1, Anh = "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg" },
                new Combo { MaCombo = "CB002", TenCombo = "Combo Chăm Sóc Da", MoTa = "Combo chăm sóc da toàn diện", Gia = 1200000 , TrangThai = 1,  Anh = "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg" },
                new Combo { MaCombo = "CB003", TenCombo = "Combo Ngừa Mụn", MoTa = "Combo sản phẩm ngừa mụn hiệu quả", Gia = 950000 , TrangThai = 1 , Anh = "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg" }
            );

            // Seed Data cho bảng ChiTietCombo
            modelBuilder.Entity<ChiTietCombo>().HasData(
                new ChiTietCombo { Id = 1, MaCombo = "CB001", MaSanPham = "SP001", SoLuong = 1 }, // Kem dưỡng ẩm
                new ChiTietCombo { Id = 2, MaCombo = "CB001", MaSanPham = "SP005", SoLuong = 1 }, // Mặt nạ cấp ẩm
                new ChiTietCombo { Id = 3, MaCombo = "CB001", MaSanPham = "SP006", SoLuong = 1 }, // Kem chống nắng
                new ChiTietCombo { Id = 4, MaCombo = "CB002", MaSanPham = "SP002", SoLuong = 1 }, // Sữa rửa mặt
                new ChiTietCombo { Id = 5, MaCombo = "CB002", MaSanPham = "SP003", SoLuong = 1 }, // Nước hoa hồng
                new ChiTietCombo { Id = 6, MaCombo = "CB002", MaSanPham = "SP004", SoLuong = 1 }, // Serum dưỡng trắng
                new ChiTietCombo { Id = 7, MaCombo = "CB002", MaSanPham = "SP010", SoLuong = 1 }, // Kem dưỡng da ban đêm
                new ChiTietCombo { Id = 8, MaCombo = "CB003", MaSanPham = "SP008", SoLuong = 1 }, // Tinh chất ngừa mụn
                new ChiTietCombo { Id = 9, MaCombo = "CB003", MaSanPham = "SP007", SoLuong = 1 }, // Tẩy trang
                new ChiTietCombo { Id = 10, MaCombo = "CB003", MaSanPham = "SP009", SoLuong = 1 }  // Xịt khoáng
            );

            // Seed Data cho bảng GiamGia
            modelBuilder.Entity<GiamGia>().HasData(
                new GiamGia
                {
                    MaGiamGia = "CP001",
                    TenGiamGia = "Giảm giá mùa hè",
                    GiaTri = 0.25m, //Giảm giá 25%
                    NgayBatDau = new DateOnly(2025, 6, 1),
                    NgayKetThuc = new DateOnly(2025, 6, 30),
                    TrangThai = 1,
                    SoLuongMaNhapToiDa = 100
                },
                new GiamGia
                {
                    MaGiamGia = "CP002",
                    TenGiamGia = "Giảm giá Noel",
                    GiaTri = 0.15m, //Giảm giá 15%
                    NgayBatDau = new DateOnly(2025, 12, 20),
                    NgayKetThuc = new DateOnly(2025, 12, 25),
                    TrangThai = 1,
                    SoLuongMaNhapToiDa = 100
                }
            );
        }
    }
}

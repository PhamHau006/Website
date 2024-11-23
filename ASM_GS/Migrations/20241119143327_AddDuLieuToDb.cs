using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ASM_GS.Migrations
{
    /// <inheritdoc />
    public partial class AddDuLieuToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    MaCombo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenCombo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    Anh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.MaCombo);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    MaDanhMuc = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "GiamGia",
                columns: table => new
                {
                    MaGiamGia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenGiamGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    SoLuongMaNhapToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiamGia", x => x.MaGiamGia);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDangKy = table.Column<DateOnly>(type: "date", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cccd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    GioiTinh = table.Column<bool>(type: "bit", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: true),
                    Cccd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioiTinh = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.MaNhanVien);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenSanPham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaDanhMuc = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateOnly>(type: "date", nullable: true),
                    DonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nsx = table.Column<DateOnly>(type: "date", nullable: true),
                    Hsd = table.Column<DateOnly>(type: "date", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.MaSanPham);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMucs",
                        principalColumn: "MaDanhMuc");
                });

            migrationBuilder.CreateTable(
                name: "MaNhapGiamGia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaGiamGia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaNhapGiamGia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaNhapGiamGia_GiamGia_MaGiamGia",
                        column: x => x.MaGiamGia,
                        principalTable: "GiamGia",
                        principalColumn: "MaGiamGia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    MaDonHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayDatHang = table.Column<DateOnly>(type: "date", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK_DonHangs_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    MaGioHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayTao = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.MaGioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    MaHoaDon = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayXuatHoaDon = table.Column<DateOnly>(type: "date", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: true),
                    MaGiamGia = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_GiamGia_MaGiamGia",
                        column: x => x.MaGiamGia,
                        principalTable: "GiamGia",
                        principalColumn: "MaGiamGia",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTrang = table.Column<int>(type: "int", nullable: false),
                    MaNhanVienNavigationMaNhanVien = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang");
                    table.ForeignKey(
                        name: "FK_TaiKhoans_NhanViens_MaNhanVienNavigationMaNhanVien",
                        column: x => x.MaNhanVienNavigationMaNhanVien,
                        principalTable: "NhanViens",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "AnhSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UrlAnh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnhSanPhams_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietCombos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCombo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietCombos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietCombos_Combos_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "Combos",
                        principalColumn: "MaCombo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietCombos_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    MaKhachHangNavigationMaKhachHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPhamNavigationMaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_KhachHangs_MaKhachHangNavigationMaKhachHang",
                        column: x => x.MaKhachHangNavigationMaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGia_SanPhams_MaSanPhamNavigationMaSanPham",
                        column: x => x.MaSanPhamNavigationMaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaChiTietDonHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaCombo = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.MaChiTietDonHang);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_Combos_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "Combos",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHangs",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGioHang = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaCombo = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_Combos_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "Combos",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_GioHangs_MaGioHang",
                        column: x => x.MaGioHang,
                        principalTable: "GioHangs",
                        principalColumn: "MaGioHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaCombo = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_Combos_MaCombo",
                        column: x => x.MaCombo,
                        principalTable: "Combos",
                        principalColumn: "MaCombo");
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_HoaDons_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "MaHoaDon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham");
                });

            migrationBuilder.InsertData(
                table: "Combos",
                columns: new[] { "MaCombo", "Anh", "Gia", "MoTa", "TenCombo", "TrangThai" },
                values: new object[,]
                {
                    { "CB001", "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg", 800000m, "Combo gồm các sản phẩm dưỡng ẩm", "Combo Dưỡng Ẩm", 1 },
                    { "CB002", "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg", 1200000m, "Combo chăm sóc da toàn diện", "Combo Chăm Sóc Da", 1 },
                    { "CB003", "img/AnhCombo/z5959105369727_62f7dd6336f7577e1dd7ee873b52f574.jpg", 950000m, "Combo sản phẩm ngừa mụn hiệu quả", "Combo Ngừa Mụn", 1 }
                });

            migrationBuilder.InsertData(
                table: "DanhMucs",
                columns: new[] { "MaDanhMuc", "TenDanhMuc", "TrangThai" },
                values: new object[,]
                {
                    { "DM001", "Dưỡng da", 1 },
                    { "DM002", "Chống nắng", 1 },
                    { "DM003", "Làm sạch", 1 }
                });

            migrationBuilder.InsertData(
                table: "GiamGia",
                columns: new[] { "MaGiamGia", "GiaTri", "NgayBatDau", "NgayKetThuc", "SoLuongMaNhapToiDa", "TenGiamGia", "TrangThai" },
                values: new object[,]
                {
                    { "CP001", 0.25m, new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 30), 100, "Giảm giá mùa hè", 1 },
                    { "CP002", 0.15m, new DateOnly(2025, 12, 20), new DateOnly(2025, 12, 25), 100, "Giảm giá Noel", 1 }
                });

            migrationBuilder.InsertData(
                table: "KhachHangs",
                columns: new[] { "MaKhachHang", "Cccd", "DiaChi", "GioiTinh", "HinhAnh", "NgayDangKy", "NgaySinh", "SoDienThoai", "TenKhachHang", "TrangThai" },
                values: new object[,]
                {
                    { "KH001", "123456789", "123 Main St", true, null, new DateOnly(2023, 1, 15), new DateOnly(1990, 1, 1), "0123456789", "Embo", 1 },
                    { "KH002", "987654321", "456 Elm St", false, null, new DateOnly(2023, 1, 16), new DateOnly(1992, 2, 2), "0987654321", "Ember", 1 }
                });

            migrationBuilder.InsertData(
                table: "NhanViens",
                columns: new[] { "MaNhanVien", "Cccd", "DiaChi", "GioiTinh", "HinhAnh", "NgayBatDau", "NgaySinh", "SoDienThoai", "TenNhanVien", "TrangThai", "VaiTro" },
                values: new object[] { "NV001", "123456789", "123 Main St", true, null, new DateOnly(2023, 1, 15), new DateOnly(1990, 1, 1), "0123456789", "Admin", 1, "Admin" });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "MaTaiKhoan", "Email", "MaKhachHang", "MaNhanVien", "MaNhanVienNavigationMaNhanVien", "MatKhau", "TenTaiKhoan", "TinhTrang", "VaiTro" },
                values: new object[] { "TK003", "admin@example.com", null, "NV001", null, "123456", "admin", 0, "Admin" });

            migrationBuilder.InsertData(
                table: "SanPhams",
                columns: new[] { "MaSanPham", "DonVi", "Gia", "Hsd", "MaDanhMuc", "MoTa", "NgayThem", "Nsx", "SoLuong", "TenSanPham", "TrangThai" },
                values: new object[,]
                {
                    { "SP001", null, 300000m, null, "DM001", null, null, null, 100, "Kem dưỡng ẩm", 1 },
                    { "SP002", null, 200000m, null, "DM003", null, null, null, 150, "Sữa rửa mặt", 1 },
                    { "SP003", null, 250000m, null, "DM001", null, null, null, 120, "Nước hoa hồng", 1 },
                    { "SP004", null, 500000m, null, "DM001", null, null, null, 80, "Serum dưỡng trắng", 1 },
                    { "SP005", null, 150000m, null, "DM001", null, null, null, 200, "Mặt nạ cấp ẩm", 1 },
                    { "SP006", null, 350000m, null, "DM002", null, null, null, 90, "Kem chống nắng", 1 },
                    { "SP007", null, 180000m, null, "DM003", null, null, null, 110, "Tẩy trang", 1 },
                    { "SP008", null, 400000m, null, "DM001", null, null, null, 70, "Tinh chất ngừa mụn", 1 },
                    { "SP009", null, 220000m, null, "DM002", null, null, null, 90, "Xịt khoáng", 1 },
                    { "SP010", null, 450000m, null, "DM001", null, null, null, 50, "Kem dưỡng da ban đêm", 1 }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "MaTaiKhoan", "Email", "MaKhachHang", "MaNhanVien", "MaNhanVienNavigationMaNhanVien", "MatKhau", "TenTaiKhoan", "TinhTrang", "VaiTro" },
                values: new object[,]
                {
                    { "TK001", "customer1@example.com", "KH001", null, null, "123", "customer1", 0, "Customer" },
                    { "TK002", "customer2@example.com", "KH002", null, null, "123", "customer2", 0, "Customer" }
                });

            migrationBuilder.InsertData(
                table: "AnhSanPhams",
                columns: new[] { "Id", "MaSanPham", "UrlAnh" },
                values: new object[,]
                {
                    { 1, "SP001", "img/AnhSanPham/kemduongam1.jpg" },
                    { 2, "SP001", "img/AnhSanPham/kemduongam2.jpg" },
                    { 3, "SP001", "img/AnhSanPham/kemduongam3.jpg" },
                    { 4, "SP001", "img/AnhSanPham/kemduongam4.jpg" },
                    { 5, "SP002", "img/AnhSanPham/suaruamat1.jpg" },
                    { 6, "SP002", "img/AnhSanPham/suaruamat2.jpg" },
                    { 7, "SP002", "img/AnhSanPham/suaruamat3.jpg" },
                    { 8, "SP002", "img/AnhSanPham/suaruamat4.jpg" },
                    { 9, "SP003", "img/AnhSanPham/toner1.png" },
                    { 10, "SP003", "img/AnhSanPham/toner2.png" },
                    { 11, "SP003", "img/AnhSanPham/toner3.png" },
                    { 12, "SP003", "img/AnhSanPham/toner4.png" },
                    { 13, "SP004", "img/AnhSanPham/serumtrang1.jpg" },
                    { 14, "SP004", "img/AnhSanPham/serumtrang2.jpg" },
                    { 15, "SP004", "img/AnhSanPham/serumtrang3.jpg" },
                    { 16, "SP004", "img/AnhSanPham/serumtrang4.jpg" },
                    { 17, "SP005", "img/AnhSanPham/mark1.jpg" },
                    { 18, "SP005", "img/AnhSanPham/mark2.jpg" },
                    { 19, "SP005", "img/AnhSanPham/mark3.jpg" },
                    { 20, "SP005", "img/AnhSanPham/mark4.jpg" },
                    { 21, "SP006", "img/AnhSanPham/kcn1.jpg" },
                    { 22, "SP006", "img/AnhSanPham/kcn2.jpg" },
                    { 23, "SP006", "img/AnhSanPham/kcn3.jpg" },
                    { 24, "SP006", "img/AnhSanPham/kcn4.jpg" },
                    { 25, "SP007", "img/AnhSanPham/taytrang1.jpg" },
                    { 26, "SP007", "img/AnhSanPham/taytrang2.jpg" },
                    { 27, "SP007", "img/AnhSanPham/taytrang3.jpg" },
                    { 28, "SP007", "img/AnhSanPham/taytrang4.jpg" },
                    { 29, "SP008", "img/AnhSanPham/tinhchat1.jpg" },
                    { 30, "SP008", "img/AnhSanPham/tinhchat2.jpg" },
                    { 31, "SP008", "img/AnhSanPham/tinhchat3.jpg" },
                    { 32, "SP008", "img/AnhSanPham/tinhchat4.jpg" },
                    { 33, "SP009", "img/AnhSanPham/xitkhoang1.jpg" },
                    { 34, "SP009", "img/AnhSanPham/xitkhoang2.jpg" },
                    { 35, "SP009", "img/AnhSanPham/xitkhoang3.jpg" },
                    { 36, "SP009", "img/AnhSanPham/xitkhoang4.jpg" },
                    { 37, "SP010", "img/AnhSanPham/bandem1.jpg" },
                    { 38, "SP010", "img/AnhSanPham/bandem2.jpg" },
                    { 39, "SP010", "img/AnhSanPham/bandem3.jpg" },
                    { 40, "SP010", "img/AnhSanPham/bandem4.jpg" }
                });

            migrationBuilder.InsertData(
                table: "ChiTietCombos",
                columns: new[] { "Id", "MaCombo", "MaSanPham", "SoLuong" },
                values: new object[,]
                {
                    { 1, "CB001", "SP001", 1 },
                    { 2, "CB001", "SP005", 1 },
                    { 3, "CB001", "SP006", 1 },
                    { 4, "CB002", "SP002", 1 },
                    { 5, "CB002", "SP003", 1 },
                    { 6, "CB002", "SP004", 1 },
                    { 7, "CB002", "SP010", 1 },
                    { 8, "CB003", "SP008", 1 },
                    { 9, "CB003", "SP007", 1 },
                    { 10, "CB003", "SP009", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPhams_MaSanPham",
                table: "AnhSanPhams",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietCombos_MaCombo",
                table: "ChiTietCombos",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietCombos_MaSanPham",
                table: "ChiTietCombos",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaCombo",
                table: "ChiTietDonHangs",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaDonHang",
                table: "ChiTietDonHangs",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_MaSanPham",
                table: "ChiTietDonHangs",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_MaCombo",
                table: "ChiTietGioHangs",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_MaGioHang",
                table: "ChiTietGioHangs",
                column: "MaGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_MaSanPham",
                table: "ChiTietGioHangs",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_MaCombo",
                table: "ChiTietHoaDons",
                column: "MaCombo");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_MaHoaDon",
                table: "ChiTietHoaDons",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_MaSanPham",
                table: "ChiTietHoaDons",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaKhachHangNavigationMaKhachHang",
                table: "DanhGia",
                column: "MaKhachHangNavigationMaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaSanPhamNavigationMaSanPham",
                table: "DanhGia",
                column: "MaSanPhamNavigationMaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaKhachHang",
                table: "DonHangs",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_MaKhachHang",
                table: "GioHangs",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaGiamGia",
                table: "HoaDons",
                column: "MaGiamGia");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaKhachHang",
                table: "HoaDons",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_MaNhapGiamGia_MaGiamGia",
                table: "MaNhapGiamGia",
                column: "MaGiamGia");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_MaDanhMuc",
                table: "SanPhams",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_MaKhachHang",
                table: "TaiKhoans",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_MaNhanVienNavigationMaNhanVien",
                table: "TaiKhoans",
                column: "MaNhanVienNavigationMaNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhSanPhams");

            migrationBuilder.DropTable(
                name: "ChiTietCombos");

            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "ChiTietGioHangs");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDons");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "MaNhapGiamGia");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "GiamGia");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "DanhMucs");
        }
    }
}

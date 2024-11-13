using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM_GS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnhSanPhamData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams");

            migrationBuilder.AlterColumn<string>(
                name: "TenSanPham",
                table: "SanPhams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "SanPhams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanPhams",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaDanhMuc",
                table: "SanPhams",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonVi",
                table: "SanPhams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenDanhMuc",
                table: "DanhMucs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 21,
                column: "UrlAnh",
                value: "img/AnhSanPham/kcn1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 22,
                column: "UrlAnh",
                value: "img/AnhSanPham/kcn2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 23,
                column: "UrlAnh",
                value: "img/AnhSanPham/kcn3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 24,
                column: "UrlAnh",
                value: "img/AnhSanPham/kcn4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 25,
                column: "UrlAnh",
                value: "img/AnhSanPham/taytrang1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 26,
                column: "UrlAnh",
                value: "img/AnhSanPham/taytrang2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 27,
                column: "UrlAnh",
                value: "img/AnhSanPham/taytrang3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 28,
                column: "UrlAnh",
                value: "img/AnhSanPham/taytrang4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 29,
                column: "UrlAnh",
                value: "img/AnhSanPham/tinhchat1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 30,
                column: "UrlAnh",
                value: "img/AnhSanPham/tinhchat2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 31,
                column: "UrlAnh",
                value: "img/AnhSanPham/tinhchat3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 32,
                column: "UrlAnh",
                value: "img/AnhSanPham/tinhchat4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 33,
                column: "UrlAnh",
                value: "img/AnhSanPham/xitkhoang1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 34,
                column: "UrlAnh",
                value: "img/AnhSanPham/xitkhoang2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 35,
                column: "UrlAnh",
                value: "img/AnhSanPham/xitkhoang3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 36,
                column: "UrlAnh",
                value: "img/AnhSanPham/xitkhoang4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 37,
                column: "UrlAnh",
                value: "img/AnhSanPham/bandem1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 38,
                column: "UrlAnh",
                value: "img/AnhSanPham/bandem2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 39,
                column: "UrlAnh",
                value: "img/AnhSanPham/bandem3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 40,
                column: "UrlAnh",
                value: "img/AnhSanPham/bandem4.jpg");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams",
                column: "MaDanhMuc",
                principalTable: "DanhMucs",
                principalColumn: "MaDanhMuc",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams");

            migrationBuilder.AlterColumn<string>(
                name: "TenSanPham",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "SanPhams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaDanhMuc",
                table: "SanPhams",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DonVi",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenDanhMuc",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 21,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp006_1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 22,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp006_2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 23,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp006_3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 24,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp006_4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 25,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp007_1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 26,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp007_2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 27,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp007_3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 28,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp007_4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 29,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp008_1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 30,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp008_2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 31,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp008_3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 32,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp008_4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 33,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp009_1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 34,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp009_2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 35,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp009_3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 36,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp009_4.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 37,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp010_1.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 38,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp010_2.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 39,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp010_3.jpg");

            migrationBuilder.UpdateData(
                table: "AnhSanPhams",
                keyColumn: "Id",
                keyValue: 40,
                column: "UrlAnh",
                value: "img/AnhSanPham/sp010_4.jpg");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams",
                column: "MaDanhMuc",
                principalTable: "DanhMucs",
                principalColumn: "MaDanhMuc");
        }
    }
}

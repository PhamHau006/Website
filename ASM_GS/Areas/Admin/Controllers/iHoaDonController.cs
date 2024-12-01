using ASM_GS.Controllers;
using ASM_GS.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

class SanPhamViewModel
{
    public string TenSanPham { get; set; }
    public int SoLuong { get; set; }
    public decimal Gia { get; set; }
    public decimal ThanhTien { get; set; }
}

[Area("Admin")]
public class iHoaDonController : Controller
{
    private readonly ApplicationDbContext _context;

    public iHoaDonController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult ExportToWord(string maHoaDon)
    {
        if (string.IsNullOrEmpty(maHoaDon))
        {
            return NotFound("Mã hóa đơn không hợp lệ.");
        }

        // Lấy thông tin hóa đơn từ database
        var hoaDon = _context.HoaDons
            .Where(hd => hd.MaHoaDon == maHoaDon)
            .Select(hd => new
            {
                hd.MaHoaDon,
                KhachHang = hd.KhachHang.TenKhachHang,
                hd.NgayXuatHoaDon,
                SanPhams = hd.ChiTietHoaDons.Select(ct => new SanPhamViewModel
                {
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : ct.Combo.TenCombo,
                    SoLuong = ct.SoLuong,
                    Gia = ct.SanPham != null ? ct.SanPham.Gia : 0,
                    ThanhTien = (ct.SanPham != null ? ct.SanPham.Gia : 0) * ct.SoLuong
                }).ToList(),
                hd.TongTien
            })
            .FirstOrDefault();

        if (hoaDon == null)
        {
            return NotFound("Không tìm thấy hóa đơn.");
        }

        // Tạo file Word trong bộ nhớ
        using (var memoryStream = new MemoryStream())
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                // Tiêu đề "HÓA ĐƠN MUA HÀNG"
                Paragraph title = new Paragraph(
                    new Run(
                        new RunProperties(
                            new Bold(), // In đậm
                            new FontSize() { Val = "40" }, // Kích thước chữ
                            new RunFonts { Ascii = "Times New Roman (Headings)" } // Phông chữ
                        ),
                        new Text("HÓA ĐƠN MUA HÀNG")
                    )
                );

                title.ParagraphProperties = new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center },
                    new SpacingBetweenLines() { After = "240" }
                );

                body.Append(title);

                // Thông tin công ty và khách hàng
                body.Append(CreateBoldLabelAndText("Tên công ty", "Công ty TNHH Dịch vụ Hóa đơn điện tử"));
                body.Append(CreateBoldLabelAndText("Địa chỉ", "58/16 Dương 47, Khu phố 6, Hiệp Bình Chánh, TP.HCM"));
                body.Append(CreateBoldLabelAndText("Điện thoại", "1900 068 838"));
                body.Append(CreateBoldLabelAndText("Số tài khoản", "78787878"));
                body.Append(CreateBoldLabelAndText("Tên khách hàng", hoaDon.KhachHang));
                body.Append(CreateBoldLabelAndText("Ngày xuất hóa đơn", hoaDon.NgayXuatHoaDon.ToString("dd/MM/yyyy")));

                // Tạo bảng danh sách sản phẩm
                Table productTable = CreateProductTable(hoaDon.SanPhams);
                body.Append(productTable);

                // Tổng tiền
                Paragraph total = new Paragraph(
                    new Run(
                        new RunProperties(
                            new Bold(),
                            new FontSize() { Val = "30" },
                            new RunFonts { Ascii = "Times New Roman (Headings)" }
                        ),
                        new Text("Tổng cộng:   " + hoaDon.TongTien.ToString("N0") + " VND")
                    )
                );

                total.ParagraphProperties = new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Right },
                    new SpacingBetweenLines() { Before = "240" }
                );

                body.Append(total);

                // Lưu tài liệu
                mainPart.Document.AppendChild(body);
                mainPart.Document.Save();
            }

            return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"HoaDon_{maHoaDon}.docx");
        }
    }

    Paragraph CreateBoldLabelAndText(string label, string text)
    {
        RunProperties labelProperties = new RunProperties(
            new Bold(),
            new FontSize() { Val = "30" },
            new RunFonts { Ascii = "Times New Roman (Headings)" }
        );

        RunProperties textProperties = new RunProperties(
            new FontSize() { Val = "30" },
            new RunFonts { Ascii = "Times New Roman (Headings)" }
        );

        // Tạo khoảng trắng sau dấu 2 chấm
        string spacing = "   "; // 3 khoảng trắng

        Paragraph paragraph = new Paragraph();
        paragraph.Append(
            new Run(labelProperties, new Text($"{label}:{spacing}")), // Thêm khoảng trắng sau dấu :
            new Run(textProperties, new Text(text)) // Nội dung văn bản
        );

        return paragraph;
    }

    Table CreateProductTable(List<SanPhamViewModel> products)
    {
        Table table = new Table();

        // Style bảng
        TableProperties tableProperties = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 12 },
                new BottomBorder { Val = BorderValues.Single, Size = 12 },
                new LeftBorder { Val = BorderValues.Single, Size = 12 },
                new RightBorder { Val = BorderValues.Single, Size = 12 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
            )
        );
        table.AppendChild(tableProperties);

        // Header bảng
        TableRow headerRow = new TableRow();
        headerRow.Append(
            CreateTableCell("STT", true),
            CreateTableCell("Tên hàng hóa, dịch vụ", true),
            CreateTableCell("Đơn vị tính", true),
            CreateTableCell("Số lượng", true),
            CreateTableCell("Đơn giá (VND)", true),
            CreateTableCell("Thành tiền (VND)", true)
        );
        table.AppendChild(headerRow);

        // Dữ liệu sản phẩm
        int stt = 1;
        foreach (var product in products)
        {
            TableRow row = new TableRow();
            row.Append(
                CreateTableCell(stt.ToString()),
                CreateTableCell(product.TenSanPham),
                CreateTableCell("Cái"),
                CreateTableCell(product.SoLuong.ToString()),
                CreateTableCell($"{product.Gia:N0}"),
                CreateTableCell($"{product.ThanhTien:N0}")
            );
            table.AppendChild(row);
            stt++;
        }

        return table;
    }

    TableCell CreateTableCell(string text, bool isHeader = false)
    {
        RunProperties runProperties = new RunProperties(
            new FontSize() { Val = "28" },
            new RunFonts { Ascii = "Times New Roman (Headings)" }
        );

        if (isHeader)
        {
            runProperties.Append(new Bold());
        }

        return new TableCell(
            new TableCellProperties(
                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3000" }
            ),
            new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center }
                ),
                new Run(runProperties, new Text(text))
            )
        );
    }
    [HttpPost]
    public IActionResult ExportAllInvoices()
    {
        // Lấy tất cả hóa đơn
        var invoices = _context.HoaDons
            .Include(h => h.KhachHang)
            .Include(h => h.ChiTietHoaDons)
            .ThenInclude(cd => cd.SanPham) // Giả sử hóa đơn có liên kết với sản phẩm
            .Where(h => h.TrangThai == 1) // Chỉ lấy hóa đơn có trạng thái "Hoàn tất"
            .ToList();

        var filePath = GenerateInvoiceWordFile(invoices);

        return File(System.IO.File.ReadAllBytes(filePath), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "All_Invoices.docx");
    }

    private string GenerateInvoiceWordFile(List<HoaDon> invoices)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", "All_Invoices.docx");

        // Tạo tài liệu Word sử dụng Open XML SDK
        using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
        {
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = new Body();

            foreach (var invoice in invoices)
            {
                body.Append(new Paragraph(new Run(new Text($"Hóa đơn ID: {invoice.MaHoaDon}"))));
                body.Append(new Paragraph(new Run(new Text($"Khách hàng: {invoice.KhachHang.TenKhachHang}"))));
                body.Append(new Paragraph(new Run(new Text($"Địa chỉ: {invoice.KhachHang.DiaChi}"))));
                body.Append(new Paragraph(new Run(new Text($"Ngày xuất hóa đơn: {invoice.NgayXuatHoaDon:dd/MM/yyyy}"))));

                body.Append(new Paragraph(new Run(new Text("Danh sách sản phẩm:"))));

                // Duyệt qua các sản phẩm trong hóa đơn
                foreach (var item in invoice.ChiTietHoaDons)
                {
                    body.Append(new Paragraph(new Run(new Text($"Tên sản phẩm: {item.SanPham.TenSanPham}"))));
                    body.Append(new Paragraph(new Run(new Text($"Số lượng: {item.SoLuong}"))));
                    body.Append(new Paragraph(new Run(new Text($"Đơn giá: {item.Gia:N0} VND"))));
                    body.Append(new Paragraph(new Run(new Text($"Thành tiền: {item.SoLuong * item.Gia:N0} VND"))));
                }

                body.Append(new Paragraph(new Run(new Text($"Tổng tiền: {invoice.TongTien:N0} VND"))));
                body.Append(new Paragraph(new Run(new Text("--------------------------------------------------"))));
            }

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return filePath;
    }

    [HttpPost]
    public IActionResult RemoveStaffAccount()
    {
        HttpContext.Session.Remove("StaffAccount");
        HttpContext.Session.Remove("Staff");
        return RedirectToAction("Index", "LoginAdmin", new { area = "" });
    }
}

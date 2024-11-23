function loadChiTietHoaDon(maHoaDon) {
    $.ajax({
        url: '/Admin/HoaDon/GetChiTietHoaDon',
        type: 'GET',
        data: { maHoaDon: maHoaDon },
        success: function (data) {
            let content = '<table class="table table-striped"><thead><tr><th>Sản phẩm</th><th>Số lượng</th><th>Giá</th></tr></thead><tbody>';
            data.forEach(item => {
                content += `<tr>
                                <td>${item.SanPham}</td>
                                <td>${item.SoLuong}</td>
                                <td>${item.Gia.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })}</td>
                            </tr>`;
            });
            content += '</tbody></table>';
            $('#chiTietContent').html(content);
        },
        error: function () {
            alert('Không thể tải chi tiết hóa đơn!');
        }
    });
}

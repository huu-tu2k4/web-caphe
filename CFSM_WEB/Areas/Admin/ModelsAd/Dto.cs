using System.ComponentModel.DataAnnotations;

namespace CFSM_WEB.Areas.Admin.ModelsAd
{

    public class HoaDonDTO
    {
        public int MaHoaDon { get; set; }
        public DateTime? NgayLap { get; set; }
        public string DiaChi { get; set; }
        public string CachThanhToan { get; set; }
        public decimal? ThanhTien { get; set; }
        public string TrangThaiHoaDon { get; set; }
        public string KhachHangHoTen { get; set; }
        public string NhanVienHoTen { get; set; }
    }

 
    public class ChiTietHdDTO
    {
        public int MaHoaDon { get; set; }
        public int MaCthd { get; set; }
        public string TenDoAn { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? TongTien { get; set; }
    }

 
    public class NhanVienDTO
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string TenDangNhap { get; set; }
        public string ChucVu { get; set; }
        public string TenHienThi { get; set; }
        public int? TrangThai { get; set; }
      
    }
    public class ChangePasswordDTO
    {
        [Required]
        public string MatKhauCu { get; set; }

        [Required]
        [MinLength(6)]
        public string MatKhauMoi { get; set; }

        [Compare("MatKhauMoi")]
        public string XacNhanMatKhau { get; set; }
    }
    public class DoAnDTO
    {
        public int MaDoAn { get; set; }
        public string TenDoAn { get; set; }
        public string AnhDoAn { get; set; }
        public decimal? DonGia { get; set; }
        public string MoTaDoAn { get; set; }
        public int? MaMenu { get; set; }
        public string TenMenu { get; set; } // Từ MaMenuNavigation
    }

    public class MenuDTO
    {
        public int MaMenu { get; set; }
        public string TenMenu { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using CFSM_WEB.Models;
using Microsoft.EntityFrameworkCore;
using CFSM_WEB.ViewModels;
using CFSM_WEB.Helpers;

namespace CFSM_WEB.Areas.Admin.Controllers
{
    // DTO cho THoaDon
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

    // DTO cho TChiTietHd
    public class ChiTietHdDTO
    {
        public int MaHoaDon { get; set; }
        public int MaCthd { get; set; }
        public string TenDoAn { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? TongTien { get; set; }
    }

    // DTO cho TNhanVien
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

    [Area("admin")]
    [Route("api/admin/homeadminapi")]
    [ApiController]
    public class HomeAdminApiController : ControllerBase
    {
        private readonly QuanLyQuanCaPheContext db;

        public HomeAdminApiController(QuanLyQuanCaPheContext context)
        {
            db = context;
        }

        // API để lấy danh sách hóa đơn
        [HttpGet]
        [Route("DanhSachHD")]
        public IActionResult GetDanhSachHD()
        {
            var listHD = db.THoaDons
                           .Include(x => x.MaKhachHangNavigation)
                           .Include(x => x.MaNhanVienNavigation)
                           .Select(x => new HoaDonDTO
                           {
                               MaHoaDon = x.MaHoaDon,
                               NgayLap = x.NgayLap,
                               DiaChi = x.DiaChi,
                               CachThanhToan = x.CachThanhToan,
                               ThanhTien = x.ThanhTien,
                               TrangThaiHoaDon = x.TrangThaiHoaDon,
                               KhachHangHoTen = x.MaKhachHangNavigation.HoTen,
                               NhanVienHoTen = x.MaNhanVienNavigation.HoTen
                           })
                           .ToList();

            return Ok(listHD);
        }

        // API để lấy chi tiết hóa đơn
        [HttpGet]
        [Route("DanhMucChiTietHD")]
        public IActionResult GetDanhMucChiTietHD(int maHD)
        {
            var listChiTietHD = db.TChiTietHds
                                  .Where(x => x.MaHoaDon == maHD)
                                  .Include(x => x.MaDoAnNavigation)
                                  .Include(x => x.MaHoaDonNavigation)
                                  .Select(x => new ChiTietHdDTO
                                  {
                                      MaHoaDon = x.MaHoaDon,
                                      MaCthd = x.MaCthd,
                                      TenDoAn = x.MaDoAnNavigation.TenDoAn,
                                      SoLuong = x.SoLuong,
                                      DonGia = x.DonGia,
                                      TongTien = x.TongTien
                                  })
                                  .ToList();

            if (!listChiTietHD.Any())
            {
                return NotFound(new { Message = "Không tìm thấy chi tiết hóa đơn nào cho mã hóa đơn này." });
            }

            return Ok(listChiTietHD);
        }

        // API để lấy danh sách nhân viên
        [HttpGet]
        [Route("DanhSachNhanVien")]
        public IActionResult GetDanhSachNhanVien()
        {
            var listNhanVien = db.TNhanViens
                                 .Select(x => new NhanVienDTO
                                 {
                                     MaNhanVien = x.MaNhanVien,
                                     HoTen = x.HoTen,
                                     Email = x.Email,
                                     DiaChi = x.DiaChi,
                                     SoDienThoai = x.SoDienThoai,
                                     TenDangNhap = x.TenDangNhap,
                                     ChucVu = x.ChucVu,
                                     TenHienThi = x.TenHienThi,
                                     TrangThai = x.TrangThai
                                 })
                                 .ToList();

            return Ok(listNhanVien);
        }

        // API để dừng hoạt động nhân viên
        [HttpPost]
        [Route("DungHoatDongNV")]
        public IActionResult DungHoatDongNV(int maNV)
        {
            var nhanVien = db.TNhanViens.FirstOrDefault(k => k.MaNhanVien == maNV);

            if (nhanVien == null)
            {
                return NotFound(new { Message = "Nhân viên không tồn tại" });
            }

            nhanVien.TrangThai = 0;
            db.SaveChanges();

            return Ok(new { Message = "Nhân viên đã bị dừng hoạt động" });
        }

        // API để thêm tài khoản nhân viên
        [HttpPost]
        [Route("ThemTaiKhoanNhanVien")]
        public IActionResult ThemTaiKhoanNhanVien([FromBody] RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var acc = db.TTaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.UserName);
            if (acc != null)
            {
                return BadRequest(new { Message = "Tên đăng nhập đã tồn tại" });
            }

            byte[] salt;
            var hashedPassword = PasswordHelper.HashPassword(model.Password, out salt);
            var taiKhoan = new TTaiKhoan
            {
                TenDangNhap = model.UserName,
                MatKhau = hashedPassword,
                Salt = Convert.ToBase64String(salt),
                LoaiTaiKhoan = 1
            };

            db.TTaiKhoans.Add(taiKhoan);
            db.SaveChanges();

            var nhanVien = new TNhanVien
            {
                HoTen = model.FullName,
                Email = model.Email,
                DiaChi = model.Address,
                SoDienThoai = model.PhoneNumber,
                TenDangNhap = model.UserName,
                ChucVu = "Nhân viên",
                TenHienThi = "a",
                TrangThai = 1
            };

            db.TNhanViens.Add(nhanVien);
            db.SaveChanges();

            return Ok(new { Message = "Tài khoản nhân viên đã được thêm thành công" });
        }
    }
}
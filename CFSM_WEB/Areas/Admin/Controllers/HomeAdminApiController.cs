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
        [Route("DanhSachKhachHang")]
        public IActionResult DanhSachKhachHang()
        {
            var listkh = db.TKhachHangs.ToList();
            return Ok(listkh);
        }
        [HttpGet]
        [Route("DanhSachKhachHangOn")]
        public IActionResult DanhSachKhachHangOn()
        {
            var danhSachKhachHangOn = db.TKhachHangs
                                        .Where(k => k.TrangThai == 1)
                                        .ToList();

            return Ok(danhSachKhachHangOn);
        }
        [HttpPut]
        [Route("DungHoatDongKhach")]
        public IActionResult DungHoatDongKhach(int maKhach)
        {
            try
            {
                var khachHang = db.TKhachHangs.FirstOrDefault(k => k.MaKhachHang == maKhach);
                if (khachHang == null)
                {
                    return NotFound(new { Message = "Khách hàng không tồn tại" });
                }

                khachHang.TrangThai = 0;
                db.SaveChanges();

                return Ok(new { Message = "Khách hàng đã bị dừng hoạt động" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi dừng hoạt động khách hàng: " + ex.Message });
            }
        }
        [HttpGet]
        [Route("DanhSachKhachHangOff")]
        public IActionResult DanhSachKhachHangOff()
        {
            var danhSachKhachHangOff = db.TKhachHangs
                                         .Where(k => k.TrangThai == 0)
                                         .ToList();

            return Ok(danhSachKhachHangOff);
        }
        [HttpPut]
        [Route("MoHoatDongKhach")]
        public IActionResult MoHoatDongKhach(int maKhach)
        {
            try
            {
                var khachHang = db.TKhachHangs.FirstOrDefault(k => k.MaKhachHang == maKhach);
                if (khachHang == null)
                {
                    return NotFound(new { Message = "Khách hàng không tồn tại" });
                }

                khachHang.TrangThai = 1;
                db.SaveChanges();

                return Ok(new { Message = "Khách hàng được mở hoạt động" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi mở hoạt động khách hàng: " + ex.Message });
            }
        }
        [HttpGet]
        [Route("TimKiemKhachHang")]
        public IActionResult TimKiemKhachHang(int? maKhach)
        {
            if (maKhach == null)
            {
                return BadRequest(new { Message = "Vui lòng cung cấp mã khách hàng để tìm kiếm" });
            }

            var khachHang = db.TKhachHangs.FirstOrDefault(k => k.MaKhachHang == maKhach.Value);
            if (khachHang != null)
            {
                return Ok(new { KhachHang = khachHang });
            }

            return NotFound(new { Message = "Không tìm thấy khách hàng với mã này" });
        }
        [HttpGet]
        [Route("DanhMucSP")]
        public IActionResult GetDanhMucSP()
        {
            var lstSanPham = db.TDoAns
                              .AsNoTracking()
                              .OrderBy(x => x.TenDoAn)
                              .Include(x => x.MaMenuNavigation)
                              .Select(x => new DoAnDTO
                              {
                                  MaDoAn = x.MaDoAn,
                                  TenDoAn = x.TenDoAn,
                                  AnhDoAn = x.AnhDoAn,
                                  DonGia = x.DonGia,
                                  MoTaDoAn = x.MoTaDoAn,
                                  MaMenu = x.MaMenu,
                                  TenMenu = x.MaMenuNavigation.TenMenu
                              })
                              .ToList();

            return Ok(lstSanPham);
        }

        // API để lấy danh sách menu (cho ViewBag.MaMenu)
        [HttpGet]
        [Route("GetMenus")]
        public IActionResult GetMenus()
        {
            var menus = db.TMenus
                          .Select(x => new MenuDTO
                          {
                              MaMenu = x.MaMenu,
                              TenMenu = x.TenMenu
                          })
                          .ToList();

            return Ok(menus);
        }

        // API để thêm sản phẩm mới
        [HttpPost]
        [Route("ThemSanPhamMoi")]
        public async Task<IActionResult> ThemSanPhamMoi([FromForm] TDoAn sanPham, IFormFile AnhFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Kiểm tra và lưu ảnh
                if (AnhFile != null && AnhFile.Length > 0)
                {
                    var fileName = Path.GetFileNameWithoutExtension(AnhFile.FileName);
                    var extension = Path.GetExtension(AnhFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImagesMenu", fileName + extension);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhFile.CopyToAsync(stream);
                    }

                    sanPham.AnhDoAn = fileName + extension;
                }

                // Thêm sản phẩm vào database
                db.TDoAns.Add(sanPham);
                await db.SaveChangesAsync();

                return Ok(new { Message = "Sản phẩm đã được thêm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Đã xảy ra lỗi khi lưu sản phẩm: " + ex.Message });
            }
        }

        // API để lấy thông tin sản phẩm để sửa
        [HttpGet]
        [Route("SuaSanPham")]
        public IActionResult GetSuaSanPham(int maSanPham)
        {
            var sanPham = db.TDoAns.Find(maSanPham);
            if (sanPham == null)
            {
                return NotFound(new { Message = "Sản phẩm không tồn tại" });
            }

            var sanPhamDTO = new DoAnDTO
            {
                MaDoAn = sanPham.MaDoAn,
                TenDoAn = sanPham.TenDoAn,
                AnhDoAn = sanPham.AnhDoAn,
                DonGia = sanPham.DonGia,
                MoTaDoAn = sanPham.MoTaDoAn,
                MaMenu = sanPham.MaMenu
            };

            return Ok(sanPhamDTO);
        }

        // API để cập nhật sản phẩm
        [HttpPut]
        [Route("SuaSanPham")]
        public IActionResult UpdateSuaSanPham([FromBody] TDoAn sanPham)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingSanPham = db.TDoAns.Find(sanPham.MaDoAn);
            if (existingSanPham == null)
            {
                return NotFound(new { Message = "Sản phẩm không tồn tại" });
            }

            existingSanPham.TenDoAn = sanPham.TenDoAn;
            existingSanPham.AnhDoAn = sanPham.AnhDoAn;
            existingSanPham.DonGia = sanPham.DonGia;
            existingSanPham.MoTaDoAn = sanPham.MoTaDoAn;
            existingSanPham.MaMenu = sanPham.MaMenu;

            db.TDoAns.Update(existingSanPham);
            db.SaveChanges();

            return Ok(new { Message = "Sản phẩm đã được cập nhật thành công" });
        }

        // API để xóa sản phẩm
        [HttpDelete]
        [Route("XoaSanPham")]
        public IActionResult XoaSanPham(int maSanPham)
        {
            try
            {
                var chiTietHd = db.TChiTietHds.Where(x => x.MaDoAn == maSanPham).ToList();
                if (chiTietHd.Any())
                {
                    return BadRequest(new { Message = "Không xóa được sản phẩm này vì đã có trong hóa đơn" });
                }

                var sanPham = db.TDoAns.Find(maSanPham);
                if (sanPham == null)
                {
                    return NotFound(new { Message = "Sản phẩm không tồn tại" });
                }

                db.TDoAns.Remove(sanPham);
                db.SaveChanges();

                return Ok(new { Message = "Sản phẩm đã được xóa thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
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
        [HttpPut]
        [Route("DungHoatDongNV")]
        public IActionResult DungHoatDongNV(int maNV)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi dừng hoạt động nhân viên: " + ex.Message });
            }
        }

        // API để thêm tài khoản nhân viên
        [HttpPost("ThemTaiKhoanNhanVien")]
        public async Task<IActionResult> ThemTaiKhoanNhanVien([FromBody] RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var acc = await db.TTaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == model.UserName);
            if (acc != null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            }

            var taiKhoan = new TTaiKhoan
            {
                TenDangNhap = model.UserName,
                MatKhau = model.Password, // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
                LoaiTaiKhoan = 1
            };

            db.TTaiKhoans.Add(taiKhoan);
            await db.SaveChangesAsync();

            var nhanVien = new TNhanVien
            {
                HoTen = model.FullName,
                Email = model.Email,
                DiaChi = model.Address,
                SoDienThoai = model.PhoneNumber,
                TenDangNhap = model.UserName,
                ChucVu = model.Position,
                TenHienThi = model.FullName,
                TrangThai = 1
            };

            db.TNhanViens.Add(nhanVien);
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm tài khoản nhân viên thành công" });
        }
    }
}
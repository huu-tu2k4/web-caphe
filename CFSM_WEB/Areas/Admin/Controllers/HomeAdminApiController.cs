using Microsoft.AspNetCore.Mvc;
using CFSM_WEB.Models;
using Microsoft.EntityFrameworkCore;
using CFSM_WEB.ViewModels;
using CFSM_WEB.Helpers;
using CFSM_WEB.Areas.Admin.ModelsAd;
using DocumentFormat.OpenXml.InkML;
using ClosedXML.Excel;
using System.Security.Claims;

namespace CFSM_WEB.Areas.Admin.Controllers
{
   
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
        [HttpGet]
        [Route("DanhSachKhachHang")]
        public IActionResult DanhSachKhachHang(int page = 1, int pageSize = 10)
        {
            var total = db.TKhachHangs.Count();
            var data = db.TKhachHangs
                .OrderBy(k => k.MaKhachHang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                Data = data
            });
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
        public IActionResult TimKiemKhachHang(string tenKhachHang)
        {
            if (string.IsNullOrWhiteSpace(tenKhachHang))
            {
                return BadRequest(new { Message = "Vui lòng cung cấp từ khóa tìm kiếm" });
            }

            var khachHang = db.TKhachHangs
                .Where(k => k.HoTen.Contains(tenKhachHang))
                .ToList();

            if (!khachHang.Any())
            {
                return Ok(new List<TKhachHang>()); // Trả về mảng rỗng thay vì NotFound
            }

            return Ok(khachHang);
        }

        [HttpGet]
        [Route("DanhMucSP")]
        public IActionResult GetDanhMucSP(int page = 1, int pageSize = 10)
        {
            var total = db.TDoAns.Count();
            var data = db.TDoAns
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                Data = data
            });
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
        public IActionResult GetDanhSachHD(int page = 1, int pageSize = 6)
        {
            try
            {
                var total = db.THoaDons.Count();

                var listHD = db.THoaDons
                    .AsNoTracking()
                    .Include(x => x.MaKhachHangNavigation)
                    .Include(x => x.MaNhanVienNavigation)
                    .OrderBy(x => x.MaHoaDon)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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

                return Ok(new
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize),
                    Data = listHD
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi lấy danh sách hóa đơn: " + ex.Message });
            }
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
        public IActionResult GetDanhSachNhanVien(int page = 1, int pageSize = 3)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new { Message = "Page và pageSize phải lớn hơn 0" });
                }

                var total = db.TNhanViens.Count();
                var listNhanVien = db.TNhanViens
                    .AsNoTracking()
                    .OrderBy(x => x.MaNhanVien)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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

                return Ok(new
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize),
                    Data = listNhanVien
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi lấy danh sách nhân viên: " + ex.Message });
            }
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
        public async Task<IActionResult> ThemTaiKhoanNhanVien([FromBody] RegisterEmployeeVM model)
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
                MatKhau = model.Password, 
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

        [HttpGet]
        [Route("TopSanPham")]
        public IActionResult GetTopSanPham()
        {
            var topSanPham = db.TChiTietHds
                .Include(x => x.MaDoAnNavigation)
                .GroupBy(x => x.MaDoAn)
                .Select(g => new
                {
                    TenDoAn = g.First().MaDoAnNavigation.TenDoAn,
                    SoLuong = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            var labels = topSanPham.Select(x => x.TenDoAn).ToList();
            var data = topSanPham.Select(x => x.SoLuong).ToList();

            return Ok(new { labels, data });
        }
        [HttpGet]
        [Route("DashboardStats")]
        public IActionResult GetDashboardStats()
        {
            try
            {
               
                var totalKhachHang = db.TKhachHangs.Count();

            
                var khachHangOn = db.TKhachHangs.Count(k => k.TrangThai == 1);
              
                var totalSanPham = db.TDoAns.Count();           
                var now = DateTime.Now;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var doanhThuThang = db.THoaDons
                    .Where(hd => hd.NgayLap >= firstDayOfMonth && hd.NgayLap <= lastDayOfMonth)
                    .Sum(hd => hd.ThanhTien) ?? 0; // Xử lý null bằng cách dùng 0 nếu không có doanh thu

                return Ok(new
                {
                    totalKhachHang,
                    khachHangOn,
                    totalSanPham,
                    doanhThuThang
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi lấy thống kê: " + ex.Message });
            }
        }
        [HttpGet]
        [Route("DoanhThuTheoThang")]
        public IActionResult DoanhThuTheoThang(int? year = null)
        {
            
            try
            {
                // Lấy danh sách hóa đơn
                var query = db.THoaDons.AsQueryable();

                // Nếu có năm được chỉ định, lọc theo năm
                if (year.HasValue)
                {
                    query = query.Where(hd => hd.NgayLap.Year == year.Value);
                }

                // Nhóm hóa đơn theo tháng và tính tổng doanh thu
                var doanhThuTheoThang = query
                    .GroupBy(hd => new { hd.NgayLap.Year, hd.NgayLap.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        DoanhThu = g.Sum(hd => hd.ThanhTien) ?? 0 
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // Chuẩn bị dữ liệu trả về
                var result = doanhThuTheoThang.Select(x => new
                {
                    Thang = $"{x.Month}/{x.Year}",
                    DoanhThu = x.DoanhThu
                }).ToList();

                return Ok(new
                {
                    Data = result,
                    TotalMonths = result.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi lấy thống kê doanh thu theo tháng: " + ex.Message });
            }
        }
        [HttpGet("ExportChiTietHDExcel")]
        public IActionResult ExportChiTietHDExcel(int maHD)
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

            if (listChiTietHD == null || !listChiTietHD.Any())
                return NotFound(new { message = "Không có dữ liệu để xuất." });

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ChiTietHD");

            // Tiêu đề báo cáo
            worksheet.Cell(1, 1).Value = "CHI TIẾT HÓA ĐƠN";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Thông tin hóa đơn
            worksheet.Cell(2, 1).Value = "Mã hóa đơn:";
            worksheet.Cell(2, 2).Value = maHD;
            worksheet.Cell(2, 1).Style.Font.Bold = true;

            // Tiêu đề cột
            var headerRow = 4;
            worksheet.Cell(headerRow, 1).Value = "STT";
            worksheet.Cell(headerRow, 2).Value = "Mã CTHD";
            worksheet.Cell(headerRow, 3).Value = "Tên sản phẩm";
            worksheet.Cell(headerRow, 4).Value = "Số lượng";
            worksheet.Cell(headerRow, 5).Value = "Đơn giá";
            worksheet.Cell(headerRow, 6).Value = "Thành tiền";
            worksheet.Cell(headerRow, 7).Value = "Tổng tiền";

            // Định dạng tiêu đề cột
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Ghi dữ liệu
            for (int i = 0; i < listChiTietHD.Count; i++)
            {
                var row = i + headerRow + 1;
                var item = listChiTietHD[i];
                var thanhTien = item.SoLuong * item.DonGia;

                worksheet.Cell(row, 1).Value = i + 1; // STT
                worksheet.Cell(row, 2).Value = item.MaCthd;
                worksheet.Cell(row, 3).Value = item.TenDoAn;
                worksheet.Cell(row, 4).Value = item.SoLuong;
                worksheet.Cell(row, 5).Value = item.DonGia;
                worksheet.Cell(row, 6).Value = thanhTien;
                worksheet.Cell(row, 7).Value = item.TongTien;

                // Định dạng số tiền
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
            }

            
            var dataRange = worksheet.Range(headerRow, 1, headerRow + listChiTietHD.Count, 7);
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            
            worksheet.Column(1).Width = 8;  
            worksheet.Column(2).Width = 12; 
            worksheet.Column(3).Width = 30; 
            worksheet.Column(4).Width = 12; 
            worksheet.Column(5).Width = 15; 
            worksheet.Column(6).Width = 15; 
            worksheet.Column(7).Width = 15; 

            
            for (int col = 1; col <= 7; col++)
            {
                if (col == 3) continue; 
                worksheet.Column(col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            
            var totalRow = headerRow + listChiTietHD.Count + 1;
            worksheet.Cell(totalRow, 5).Value = "Tổng cộng:";
            worksheet.Cell(totalRow, 5).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(totalRow, 7).Value = listChiTietHD.Sum(x => x.TongTien);
            worksheet.Cell(totalRow, 7).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 7).Style.NumberFormat.Format = "#,##0";

            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ChiTietHD_{maHD}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [Route("ProfileNhanVien")]
        [HttpGet]
        public IActionResult ProfileNhanVien()
        {
            try
            {
                var NhanVienIdClaim = User.FindFirst(MySetting.CLAIM_EMPLOYEEID);

                if (NhanVienIdClaim == null)
                {
                    return BadRequest("Không tìm thấy thông tin nhân viên.");
                }

                int employeeId = int.Parse(NhanVienIdClaim.Value);

                var employee = db.TNhanViens.FirstOrDefault(e => e.MaNhanVien == employeeId);

                if (employee == null)
                {
                    return NotFound("Không tìm thấy nhân viên với ID đã cho.");
                }

                var profile = new NhanVienDTO
                {
                    MaNhanVien = employee.MaNhanVien,
                    HoTen = employee.HoTen,
                    Email = employee.Email,
                    DiaChi = employee.DiaChi,
                    SoDienThoai = employee.SoDienThoai,
                    ChucVu = employee.ChucVu,
                    TenHienThi = employee.TenHienThi,
                    TenDangNhap = employee.TenDangNhap,
                    TrangThai = 1
                };

                return Ok(profile);  // Trả về dữ liệu dưới dạng JSON
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi lấy thông tin nhân viên: " + ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateProfileNhanVien")]
        public async Task<IActionResult> UpdateProfileNhanVien([FromBody] NhanVienDTO model)
        {
            try
            {
                var NhanVienIdClaim = User.FindFirst(MySetting.CLAIM_EMPLOYEEID);
                if (NhanVienIdClaim == null)
                {
                    return BadRequest(new { Message = "Không tìm thấy thông tin nhân viên." });
                }

                int employeeId = int.Parse(NhanVienIdClaim.Value);

                var employee = await db.TNhanViens.FirstOrDefaultAsync(e => e.MaNhanVien == employeeId);
                if (employee == null)
                {
                    return NotFound(new { Message = "Không tìm thấy nhân viên với ID đã cho." });
                }

                // Cập nhật thông tin
                employee.HoTen = model.HoTen;
                employee.Email = model.Email;
                employee.DiaChi = model.DiaChi;
                employee.ChucVu = model.ChucVu;
                employee.SoDienThoai = model.SoDienThoai;
                employee.TenHienThi = model.TenHienThi;
                employee.TenDangNhap = model.TenDangNhap;

                db.TNhanViens.Update(employee);
                await db.SaveChangesAsync();

                return Ok(new { Message = "Cập nhật thông tin thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống khi cập nhật thông tin.", Error = ex.Message });
            }
        }
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var NhanVienIdClaim = User.FindFirst(MySetting.CLAIM_EMPLOYEEID);
                if (NhanVienIdClaim == null)
                {
                    return BadRequest(new { Message = "Không tìm thấy thông tin nhân viên." });
                }

                int employeeId = int.Parse(NhanVienIdClaim.Value);

                var employee = await db.TNhanViens.FirstOrDefaultAsync(e => e.MaNhanVien == employeeId);
               

                var account = await db.TTaiKhoans.FirstOrDefaultAsync(a => a.TenDangNhap == employee.TenDangNhap);
                if (account == null)
                {
                    return BadRequest(new { Message = "Không tìm thấy tài khoản." });
                }

                if (account.MatKhau != model.MatKhauCu)
                {
                    return BadRequest(new { Message = "Mật khẩu hiện tại không đúng." });
                }

                if (model.MatKhauMoi != model.XacNhanMatKhau)
                {
                    return BadRequest(new { Message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });
                }

                account.MatKhau = model.MatKhauMoi;

                db.TTaiKhoans.Update(account);
                await db.SaveChangesAsync();

                return Ok(new { Message = "Đổi mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống khi đổi mật khẩu.", Error = ex.Message });
            }
        }


    }
}
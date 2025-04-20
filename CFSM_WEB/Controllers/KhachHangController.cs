using CFSM_WEB.Helpers;
using CFSM_WEB.Models;
using CFSM_WEB.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CFSM_WEB.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyQuanCaPheContext db;

        public KhachHangController(QuanLyQuanCaPheContext context)
        {
            db = context;
        }
        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;

            if (ModelState.IsValid)
            {
                // Tìm tài khoản theo tên đăng nhập
                var acc = db.TTaiKhoans.SingleOrDefault(p => p.TenDangNhap == model.UserName);

                if (acc == null)
                {
                    ModelState.AddModelError("UserName", "Không có tài khoản này");
                    return View(model);
                }

                if (acc.LoaiTaiKhoan == 1) // Admin
                {
                    var nhanVien = db.TNhanViens.SingleOrDefault(k => k.TenDangNhap == model.UserName);

                    if (nhanVien == null || nhanVien.TrangThai == 0)
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }

                    if (acc.MatKhau == model.Password)
                    {
                        // Tạo claims cho admin
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, nhanVien.HoTen),
                    new Claim(MySetting.CLAIM_EMPLOYEEID, nhanVien.MaNhanVien.ToString()),
                    new Claim("LoaiTaiKhoan", acc.LoaiTaiKhoan.ToString()) 
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);

                        if (Url.IsLocalUrl(ReturnUrl))
                            return Redirect(ReturnUrl);
                        else
                            return RedirectToAction("Index", "HomeAdmin", new { area = "admin" });
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Sai thông tin đăng nhập");
                        return View(model);
                    }
                }
                else // Khách hàng
                {
                    var salt = Convert.FromBase64String(acc.Salt);
                    var isPasswordValid = PasswordHelper.VerifyPassword(model.Password, acc.MatKhau, salt);

                    if (!isPasswordValid)
                    {
                        ModelState.AddModelError("Password", "Sai thông tin đăng nhập");
                        return View(model);
                    }

                    var khachHang = db.TKhachHangs.SingleOrDefault(k => k.TenDangNhap == model.UserName);

                    if (khachHang == null || khachHang.TrangThai == 0)
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }

                    // Tạo claims cho khách hàng
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, khachHang.HoTen),
                new Claim(MySetting.CLAIM_CUSTOMERID, khachHang.MaKhachHang.ToString()),
                new Claim("LoaiTaiKhoan", acc.LoaiTaiKhoan.ToString()) // dùng claim riêng thay cho Role
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    if (Url.IsLocalUrl(ReturnUrl))
                        return Redirect(ReturnUrl);
                    else
                        return Redirect("/");
                }
            }

            return View(model);
        }



        [Authorize]
        public IActionResult Profile()
        {
            var customerIdClaim = User.FindFirst(MySetting.CLAIM_CUSTOMERID);
            var customerId = int.Parse(customerIdClaim.Value);
            var khachHang = db.TKhachHangs.SingleOrDefault(k => k.MaKhachHang == customerId);
            return View(khachHang);
        }
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var acc = db.TTaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.UserName);
                if (acc != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }
                byte[] salt;
                var hashedPassword = PasswordHelper.HashPassword(model.Password, out salt);


                var taiKhoan = new TTaiKhoan
                {
                    TenDangNhap = model.UserName,
                    MatKhau = hashedPassword,
                    Salt = Convert.ToBase64String(salt),
                    LoaiTaiKhoan = 0
                };
                db.TTaiKhoans.Add(taiKhoan);
                db.SaveChanges();


                var khachHang = new TKhachHang
                {
                    HoTen = model.FullName,
                    TenHienThi = model.UserName,
                    Email = model.Email,
                    DiaChi = model.Address,
                    SoDienThoai = model.PhoneNumber,
                    TenDangNhap = model.UserName


                };
                db.TKhachHangs.Add(khachHang);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }

    }
}

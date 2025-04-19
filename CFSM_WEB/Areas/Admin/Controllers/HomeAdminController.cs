using Microsoft.AspNetCore.Mvc;
using CFSM_WEB.Models;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure;
using X.PagedList.Extensions;
using CFSM_WEB.ViewModels;
using CFSM_WEB.Helpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Net.NetworkInformation;
using System;

namespace CFSM_WEB.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/homeadmin")]
    public class HomeAdminController : Controller
    {
        QuanLyQuanCaPheContext db = new QuanLyQuanCaPheContext();
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            var customerIdClaim = User.FindFirst(MySetting.CLAIM_CUSTOMERID);
            var customerId = int.Parse(customerIdClaim.Value);
            var nhanVien = db.TNhanViens.SingleOrDefault(p => p.MaNhanVien == customerId);

            if (nhanVien == null)
            {
                return NotFound();  
            }     
            return View(nhanVien);  
        }
        [Route("DanhMucSP")]
        public IActionResult DanhMucSP() {         
            return View();
        }

        [Route("ThemSanPhamMoi")]
        [HttpGet]
        public IActionResult ThemSanPhamMoi()
        {
            //ViewBag.MaMenu = new SelectList(db.TMenus.ToList(), "MaMenu", "TenMenu");
            return View();
        }
       


        [Route("SuaSanPham")]
        [HttpGet]
        public IActionResult SuaSanPham()//int maSanPham
        {
            
            return View();
        }
       
        [Route("DanhSachHD")]
        [HttpGet]
        public IActionResult DanhSachHD()
        {
                    return View();//lst
        }

        [Route("DanhMucChiTietHD")]
        [HttpGet]
        public IActionResult DanhMucChiTietHD()//int maHD
        {     
            return View();//listChiTietHD
        }
        [Route("DanhSachKhachHang")]
        [HttpGet]
        public IActionResult DanhSachKhachHang()
        {
            

            return View();//listkh
        }
        [Route("DanhSachKhachHangOn")]
        [HttpGet]
        public IActionResult DanhSachKhachHangOn()//int? page
        {
            
            return View();//danhSachKhachHangOn
        }
        
        [Route("DanhSachNhanVien")]
        [HttpGet]
        public IActionResult DanhSachNhanVien()
        {
            //var listNhanVien = db.TNhanViens.ToList();

            return View();//listNhanVien
        }
       
        [Route("ThemTaiKhoanNhanVien")]
        [HttpGet]

        public IActionResult ThemTaiKhoanNhanVien()
        {
            return View();
        }
        [Route("DoanhThuTheoThang")]
        [HttpGet]
        public IActionResult DoanhThuTheoThang()
        {
            return View();
        }
    }
}

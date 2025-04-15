using CFSM_WEB.Helpers;
using CFSM_WEB.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("QuanLyQuanCaPheContext");
builder.Services.AddDbContext<QuanLyQuanCaPheContext>(x => x.UseSqlServer(connectionString));

//Add Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(option =>
{
    option.IOTimeout = TimeSpan.FromSeconds(15);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});
//Add Authen
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/KhachHang/Login";
        option.AccessDeniedPath = "/AccessDenied";
    });
// đăng ký PaypalClient dạng Singleton() - chỉ có 1 instance duy nhất trong toàn ứng dụng
builder.Services.AddSingleton(x => new PaypalClient(
        builder.Configuration["PaypalOptions:AppId"],
        builder.Configuration["PaypalOptions:AppSecret"],
        builder.Configuration["PaypalOptions:Mode"]
));
// Thêm cấu hình CORS để cho phép gọi API từ các domain khác (nếu cần)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//Use Session
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=IndexHome}/{id?}");

// Định tuyến cho controllers (bao gồm cả API controllers)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.Run();

//using CFSM_WEB.Helpers;
//using CFSM_WEB.Models;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//var connectionString = builder.Configuration.GetConnectionString("QuanLyQuanCaPheContext");
//builder.Services.AddDbContext<QuanLyQuanCaPheContext>(x => x.UseSqlServer(connectionString));

//// Add Session
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(option =>
//{
//    option.IdleTimeout = TimeSpan.FromSeconds(15); // Sửa IOTimeout thành IdleTimeout
//    option.Cookie.HttpOnly = true;
//    option.Cookie.IsEssential = true;
//});

//// Add Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(option =>
//    {
//        option.LoginPath = "/KhachHang/Login";
//        option.AccessDeniedPath = "/AccessDenied";
//    });

//// Đăng ký PaypalClient dạng Singleton
//builder.Services.AddSingleton(x => new PaypalClient(
//    builder.Configuration["PaypalOptions:AppId"],
//    builder.Configuration["PaypalOptions:AppSecret"],
//    builder.Configuration["PaypalOptions:Mode"]
//));
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//// Sử dụng CORS trước khi sử dụng Authentication và Authorization
//app.UseCors("AllowAll");

//// Use Session
//app.UseSession();
//app.UseAuthentication();
//app.UseAuthorization();





//// Đảm bảo API controllers được hỗ trợ


//app.Run();
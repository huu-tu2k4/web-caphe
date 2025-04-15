using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CFSM_WEB.Models;

public partial class QuanLyQuanCaPheContext : DbContext
{
    public QuanLyQuanCaPheContext()
    {
    }

    public QuanLyQuanCaPheContext(DbContextOptions<QuanLyQuanCaPheContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TBlog> TBlogs { get; set; }

    public virtual DbSet<TChiTietHd> TChiTietHds { get; set; }

    public virtual DbSet<TDoAn> TDoAns { get; set; }

    public virtual DbSet<THoaDon> THoaDons { get; set; }

    public virtual DbSet<TKhachHang> TKhachHangs { get; set; }

    public virtual DbSet<TMenu> TMenus { get; set; }

    public virtual DbSet<TNhanVien> TNhanViens { get; set; }

    public virtual DbSet<TTaiKhoan> TTaiKhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-E79KTHN;Initial Catalog=QuanLyQuanCaPhe;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TBlog>(entity =>
        {
            entity.HasKey(e => e.MaBlog).HasName("PK__tBlog__86827C7358D8300F");

            entity.ToTable("tBlog");

            entity.Property(e => e.AnhTieuDe).HasMaxLength(100);
            entity.Property(e => e.NoiDungBlog).HasMaxLength(2000);
            entity.Property(e => e.TieuDeBlog).HasMaxLength(100);
        });

        modelBuilder.Entity<TChiTietHd>(entity =>
        {
            entity.HasKey(e => e.MaCthd).HasName("PK__tChiTiet__1E4FA7716ED073B8");

            entity.ToTable("tChiTietHD", tb => tb.HasTrigger("trg_UpdateThanhTien"));

            entity.Property(e => e.MaCthd).HasColumnName("MaCTHD");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTien)
                .HasComputedColumnSql("([SoLuong]*[DonGia])", true)
                .HasColumnType("decimal(29, 2)");

            entity.HasOne(d => d.MaDoAnNavigation).WithMany(p => p.TChiTietHds)
                .HasForeignKey(d => d.MaDoAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tChiTietH__MaDoA__60A75C0F");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.TChiTietHds)
                .HasForeignKey(d => d.MaHoaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tChiTietH__MaHoa__5FB337D6");
        });

        modelBuilder.Entity<TDoAn>(entity =>
        {
            entity.HasKey(e => e.MaDoAn).HasName("PK__tDoAn__2DCF1067457357A0");

            entity.ToTable("tDoAn");

            entity.Property(e => e.AnhDoAn).HasMaxLength(100);
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MoTaDoAn).HasMaxLength(100);
            entity.Property(e => e.TenDoAn).HasMaxLength(100);

            entity.HasOne(d => d.MaMenuNavigation).WithMany(p => p.TDoAns)
                .HasForeignKey(d => d.MaMenu)
                .HasConstraintName("FK__tDoAn__MaMenu__5812160E");
        });

        modelBuilder.Entity<THoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__tHoaDon__835ED13B9A46D913");

            entity.ToTable("tHoaDon");

            entity.Property(e => e.CachThanhToan).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThaiHoaDon).HasMaxLength(50);

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.THoaDons)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__tHoaDon__GhiChu__5BE2A6F2");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.THoaDons)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__tHoaDon__MaNhanV__5CD6CB2B");
        });

        modelBuilder.Entity<TKhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__tKhachHa__88D2F0E51CB4E8BC");

            entity.ToTable("tKhachHang");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TenHienThi).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.TKhachHangs)
                .HasForeignKey(d => d.TenDangNhap)
                .HasConstraintName("FK__tKhachHan__TenDa__4D94879B");
        });

        modelBuilder.Entity<TMenu>(entity =>
        {
            entity.HasKey(e => e.MaMenu).HasName("PK__tMenu__0EBABE42FB489797");

            entity.ToTable("tMenu");

            entity.Property(e => e.TenMenu).HasMaxLength(100);
        });

        modelBuilder.Entity<TNhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__tNhanVie__77B2CA47037139B0");

            entity.ToTable("tNhanVien");

            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TenHienThi).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasDefaultValue(1);

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.TNhanViens)
                .HasForeignKey(d => d.TenDangNhap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tNhanVien__TenDa__5165187F");
        });

        modelBuilder.Entity<TTaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TenDangNhap).HasName("PK__tTaiKhoa__55F68FC10935E879");

            entity.ToTable("tTaiKhoan");

            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.Salt).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

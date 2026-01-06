using Microsoft.EntityFrameworkCore;
using RealEstateHubAPI.Model;
using RealEstateHubAPI.Models;
using RealEstateHubAPI.Utils;

namespace RealEstateHubAPI.seeds
{
    public static class DataSeeder
    {
        public static void SeedData(ApplicationDbContext context, bool force = false, bool seedImages = true, bool seedNotifications = false)
        {
            // Seed data chỉ khi database đã được tạo (thông qua migrations)

            // Seed Cities
            if (force || !context.Set<City>().Any())
            {
                var cities = new List<City>
                {
                    new City { Name = "Hà Nội" },
                    new City { Name = "TP. Hồ Chí Minh" },
                    new City { Name = "Đà Nẵng" },
                    new City { Name = "Hải Phòng" },
                    new City { Name = "Cần Thơ" },
                    new City { Name = "An Giang" },
                    new City { Name = "Bà Rịa - Vũng Tàu" },
                    new City { Name = "Bắc Giang" },
                    new City { Name = "Bắc Kạn" },
                    new City { Name = "Bạc Liêu" }
                };
                context.Set<City>().AddRange(cities);
                context.SaveChanges();
                
                // Lấy lại Id đã được tạo
                var haNoi = context.Set<City>().FirstOrDefault(c => c.Name == "Hà Nội");
                var hcm = context.Set<City>().FirstOrDefault(c => c.Name == "TP. Hồ Chí Minh");
                var daNang = context.Set<City>().FirstOrDefault(c => c.Name == "Đà Nẵng");
                
                // Seed Districts với CityId từ database
                if (force || !context.Set<District>().Any())
                {
                    var districts = new List<District>
                    {
                        // Hà Nội
                        new District { Name = "Quận Ba Đình", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Hoàn Kiếm", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Tây Hồ", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Long Biên", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Cầu Giấy", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Đống Đa", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Hai Bà Trưng", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Hoàng Mai", CityId = haNoi?.Id ?? 1 },
                        new District { Name = "Quận Thanh Xuân", CityId = haNoi?.Id ?? 1 },
                        
                        // TP. Hồ Chí Minh
                        new District { Name = "Quận 1", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận 2", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận 3", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận 4", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận 5", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận 7", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận Bình Thạnh", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận Tân Bình", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận Tân Phú", CityId = hcm?.Id ?? 2 },
                        new District { Name = "Quận Phú Nhuận", CityId = hcm?.Id ?? 2 },
                        
                        // Đà Nẵng
                        new District { Name = "Quận Hải Châu", CityId = daNang?.Id ?? 3 },
                        new District { Name = "Quận Thanh Khê", CityId = daNang?.Id ?? 3 },
                        new District { Name = "Quận Sơn Trà", CityId = daNang?.Id ?? 3 },
                        new District { Name = "Quận Ngũ Hành Sơn", CityId = daNang?.Id ?? 3 },
                        new District { Name = "Quận Liên Chiểu", CityId = daNang?.Id ?? 3 }
                    };
                    context.Set<District>().AddRange(districts);
                    context.SaveChanges();
                }
            }
            else if (force || !context.Set<District>().Any())
            {
                // Nếu Cities đã có, lấy Id từ database
                var haNoi = context.Set<City>().FirstOrDefault(c => c.Name == "Hà Nội");
                var hcm = context.Set<City>().FirstOrDefault(c => c.Name == "TP. Hồ Chí Minh");
                var daNang = context.Set<City>().FirstOrDefault(c => c.Name == "Đà Nẵng");
                
                if (haNoi != null && hcm != null && daNang != null)
                {
                    var districts = new List<District>
                    {
                        // Hà Nội
                        new District { Name = "Quận Ba Đình", CityId = haNoi.Id },
                        new District { Name = "Quận Hoàn Kiếm", CityId = haNoi.Id },
                        new District { Name = "Quận Tây Hồ", CityId = haNoi.Id },
                        new District { Name = "Quận Long Biên", CityId = haNoi.Id },
                        new District { Name = "Quận Cầu Giấy", CityId = haNoi.Id },
                        new District { Name = "Quận Đống Đa", CityId = haNoi.Id },
                        new District { Name = "Quận Hai Bà Trưng", CityId = haNoi.Id },
                        new District { Name = "Quận Hoàng Mai", CityId = haNoi.Id },
                        new District { Name = "Quận Thanh Xuân", CityId = haNoi.Id },
                        
                        // TP. Hồ Chí Minh
                        new District { Name = "Quận 1", CityId = hcm.Id },
                        new District { Name = "Quận 2", CityId = hcm.Id },
                        new District { Name = "Quận 3", CityId = hcm.Id },
                        new District { Name = "Quận 4", CityId = hcm.Id },
                        new District { Name = "Quận 5", CityId = hcm.Id },
                        new District { Name = "Quận 7", CityId = hcm.Id },
                        new District { Name = "Quận Bình Thạnh", CityId = hcm.Id },
                        new District { Name = "Quận Tân Bình", CityId = hcm.Id },
                        new District { Name = "Quận Tân Phú", CityId = hcm.Id },
                        new District { Name = "Quận Phú Nhuận", CityId = hcm.Id },
                        
                        // Đà Nẵng
                        new District { Name = "Quận Hải Châu", CityId = daNang.Id },
                        new District { Name = "Quận Thanh Khê", CityId = daNang.Id },
                        new District { Name = "Quận Sơn Trà", CityId = daNang.Id },
                        new District { Name = "Quận Ngũ Hành Sơn", CityId = daNang.Id },
                        new District { Name = "Quận Liên Chiểu", CityId = daNang.Id }
                    };
                    context.Set<District>().AddRange(districts);
                    context.SaveChanges();
                }
            }


            // Seed Wards
            if (force || !context.Set<Ward>().Any())
            {
                var baDinh = context.Set<District>().FirstOrDefault(d => d.Name == "Quận Ba Đình");
                var hoanKiem = context.Set<District>().FirstOrDefault(d => d.Name == "Quận Hoàn Kiếm");
                var quan1 = context.Set<District>().FirstOrDefault(d => d.Name == "Quận 1");
                var quan2 = context.Set<District>().FirstOrDefault(d => d.Name == "Quận 2");
                var haiChau = context.Set<District>().FirstOrDefault(d => d.Name == "Quận Hải Châu");
                
                if (baDinh != null && hoanKiem != null && quan1 != null && quan2 != null && haiChau != null)
                {
                    var wards = new List<Ward>
                    {
                        // Quận Ba Đình - Hà Nội
                        new Ward { Name = "Phường Phúc Xá", DistrictId = baDinh.Id },
                        new Ward { Name = "Phường Trúc Bạch", DistrictId = baDinh.Id },
                        new Ward { Name = "Phường Vĩnh Phúc", DistrictId = baDinh.Id },
                        new Ward { Name = "Phường Cống Vị", DistrictId = baDinh.Id },
                        new Ward { Name = "Phường Liễu Giai", DistrictId = baDinh.Id },
                        
                        // Quận Hoàn Kiếm - Hà Nội
                        new Ward { Name = "Phường Phúc Tân", DistrictId = hoanKiem.Id },
                        new Ward { Name = "Phường Đồng Xuân", DistrictId = hoanKiem.Id },
                        new Ward { Name = "Phường Hàng Bạc", DistrictId = hoanKiem.Id },
                        new Ward { Name = "Phường Hàng Buồm", DistrictId = hoanKiem.Id },
                        new Ward { Name = "Phường Hàng Đào", DistrictId = hoanKiem.Id },
                        
                        // Quận 1 - TP. Hồ Chí Minh
                        new Ward { Name = "Phường Bến Nghé", DistrictId = quan1.Id },
                        new Ward { Name = "Phường Bến Thành", DistrictId = quan1.Id },
                        new Ward { Name = "Phường Cô Giang", DistrictId = quan1.Id },
                        new Ward { Name = "Phường Cầu Kho", DistrictId = quan1.Id },
                        new Ward { Name = "Phường Cầu Ông Lãnh", DistrictId = quan1.Id },
                        
                        // Quận 2 - TP. Hồ Chí Minh
                        new Ward { Name = "Phường An Phú", DistrictId = quan2.Id },
                        new Ward { Name = "Phường An Khánh", DistrictId = quan2.Id },
                        new Ward { Name = "Phường Bình An", DistrictId = quan2.Id },
                        new Ward { Name = "Phường Bình Khánh", DistrictId = quan2.Id },
                        new Ward { Name = "Phường Bình Trưng Đông", DistrictId = quan2.Id },
                        
                        // Quận Hải Châu - Đà Nẵng
                        new Ward { Name = "Phường Hải Châu I", DistrictId = haiChau.Id },
                        new Ward { Name = "Phường Hải Châu II", DistrictId = haiChau.Id },
                        new Ward { Name = "Phường Phước Ninh", DistrictId = haiChau.Id },
                        new Ward { Name = "Phường Thuận Phước", DistrictId = haiChau.Id },
                        new Ward { Name = "Phường Thanh Bình", DistrictId = haiChau.Id }
                    };
                    context.Set<Ward>().AddRange(wards);
                    context.SaveChanges();
                }
            }

            // Seed Categories
            if (force || !context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category 
                    { 
                        Name = "Nhà đất", 
                        Description = "Nhà đất, biệt thự, nhà phố",
                        Icon = "home",
                        IsActive = true
                    },
                    new Category 
                    { 
                        Name = "Căn hộ", 
                        Description = "Căn hộ chung cư, apartment",
                        Icon = "apartment",
                        IsActive = true
                    },
                    new Category 
                    { 
                        Name = "Đất nền", 
                        Description = "Đất nền, đất thổ cư, đất dự án",
                        Icon = "land",
                        IsActive = true
                    },
                    new Category 
                    { 
                        Name = "Nhà trọ", 
                        Description = "Phòng trọ, nhà trọ cho thuê",
                        Icon = "room",
                        IsActive = true
                    },
                    new Category 
                    { 
                        Name = "Văn phòng", 
                        Description = "Văn phòng, mặt bằng kinh doanh",
                        Icon = "office",
                        IsActive = true
                    },
                    new Category 
                    { 
                        Name = "Shop house", 
                        Description = "Nhà phố thương mại, shop house",
                        Icon = "shop",
                        IsActive = true
                    }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Seed Users - create 3 regular users (minimal)
            if (force || !context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Name = "Admin",
                        Email = "admin@realestate.com",
                        Phone = "0123456789",
                        Password = "admin123",
                        Role = "Admin",
                        IsLocked = false,
                        Create = DateTimeHelper.GetVietnamNow()
                    },
                    new User
                    {
                        Name = "Nguyễn Văn A",
                        Email = "user1@example.com",
                        Phone = "0987654321",
                        Password = "user123",
                        Role = "Pro_1", // 1 user Pro_1
                        IsLocked = false,
                        Create = DateTimeHelper.GetVietnamNow()
                    },
                    new User
                    {
                        Name = "Trần Thị B",
                        Email = "user2@example.com",
                        Phone = "0912345678",
                        Password = "user123",
                        Role = "Pro_12",
                        IsLocked = false,
                        Create = DateTimeHelper.GetVietnamNow()
                    },
                    new User
                    {
                        Name = "Lê Văn C",
                        Email = "user3@example.com",
                        Phone = "0923456789",
                        Password = "user123",
                        Role = "Pro_3",
                        IsLocked = false,
                        Create = DateTimeHelper.GetVietnamNow()
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Seed Posts (minimal): create 3 users * 5 posts each (only if Posts empty and Categories + Users exist)
            if ((force || !context.Posts.Any()) && context.Users.Any() && context.Categories.Any())
            {
                var usersList = context.Users.Where(u => u.Role != "Admin").Take(3).ToList();
                var categoryList = context.Categories.ToList();

                // simple locations array (reuse subset)
                var locations = new List<(string Street, string Ward, string District, string City, float Lon, float Lat, string FullAddr)>
                {
                    ("Đường Hoàng Hoa Thám", "Phường Phúc Xá", "Quận Ba Đình", "Hà Nội", 105.8342f, 21.0278f, "Đường Hoàng Hoa Thám, Phường Phúc Xá, Quận Ba Đình, Hà Nội"),
                    ("Phố Hàng Đào", "Phường Hàng Đào", "Quận Hoàn Kiếm", "Hà Nội", 105.8500f, 21.0285f, "Phố Hàng Đào, Phường Hàng Đào, Quận Hoàn Kiếm, Hà Nội"),
                    ("Đường Nguyễn Huệ", "Phường Bến Nghé", "Quận 1", "TP. Hồ Chí Minh", 106.6297f, 10.7769f, "Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh"),
                    ("Đường Lê Lợi", "Phường Bến Thành", "Quận 1", "TP. Hồ Chí Minh", 106.699f, 10.775f, "Đường Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh"),
                    ("Đường Nguyễn Thị Định", "Phường An Phú", "Quận 2", "TP. Hồ Chí Minh", 106.7500f, 10.8000f, "Đường Nguyễn Thị Định, Phường An Phú, Quận 2, TP. Hồ Chí Minh")
                };

                if (usersList.Count == 3 && categoryList.Count > 0)
                {
                    var posts = new List<Post>();
                    for (int u = 0; u < usersList.Count; u++)
                    {
                        var user = usersList[u];
                        for (int i = 0; i < 5; i++)
                        {
                            var loc = locations[(u * 5 + i) % locations.Count];
                            var category = categoryList[(u * 5 + i) % categoryList.Count];

                            posts.Add(new Post
                            {
                                Title = $"Bất động sản tại {loc.District}, {loc.City} - {i+1}",
                                Description = $"Mô tả chi tiết cho bất động sản số {i+1} tại {loc.FullAddr}. Vị trí thực, tiện ích xung quanh, pháp lý rõ ràng.",
                                Price = (decimal)(3 + (u + i) % 10) + 0.5m,
                                TransactionType = (i % 2 == 0) ? TransactionType.Sale : TransactionType.Rent,
                                Status = "Pending",
                                Area_Size = (float)(30 + ((u + i) % 5) * 10),
                                Street_Name = loc.Street,
                                UserId = user.Id,
                                CategoryId = category.Id,
                                IsApproved = false,
                                Created = DateTimeHelper.GetVietnamNow().AddDays(-(u + i)),
                                ExpiryDate = null,
                                FullAddress = loc.FullAddr,
                                Longitude = loc.Lon,
                                Latitude = loc.Lat,
                                CityName = loc.City,
                                DistrictName = loc.District,
                                WardName = loc.Ward,
                                SoPhongNgu = (i % 3) + 1,
                                SoPhongTam = (i % 2) + 1,
                                SoTang = (i % 4) + 1,
                                PhapLy = "Sổ đỏ",
                            });
                        }
                    }

                    context.Posts.AddRange(posts);
                    context.SaveChanges();
                }
            }

            // Seed PostImages (chỉ seed nếu đã có Posts)
            // This block can be skipped by passing seedImages = false to SeedData
            if (seedImages && (force || !context.Set<PostImage>().Any()) && context.Posts.Any())
            {
                var posts = context.Posts.OrderBy(p => p.Id).ToList();
                if (posts.Count > 0)
                {
                    var postImages = new List<PostImage>();
                    
                    // Seed images cho các posts có sẵn (tối đa 8 posts đầu tiên)
                    var maxPosts = Math.Min(posts.Count, 8);
                    for (int i = 0; i < maxPosts; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post1_image1.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post1_image2.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post1_image3.jpg" });
                                break;
                            case 1:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post2_image1.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post2_image2.jpg" });
                                break;
                            case 2:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post3_image1.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post3_image2.jpg" });
                                break;
                            case 3:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post4_image1.jpg" });
                                break;
                            case 4:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post5_image1.jpg" });
                                break;
                            case 5:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post6_image1.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post6_image2.jpg" });
                                break;
                            case 6:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post7_image1.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post7_image2.jpg" });
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post7_image3.jpg" });
                                break;
                            case 7:
                                postImages.Add(new PostImage { PostId = posts[i].Id, Url = "/uploads/post8_image1.jpg" });
                                break;
                        }
                    }
                    
                    // Thêm ảnh cho post thứ 9 (nếu có)
                    if (posts.Count > 8)
                    {
                        postImages.Add(new PostImage { PostId = posts[8].Id, Url = "/uploads/post9_image1.jpg" });
                    }
                    
                    context.Set<PostImage>().AddRange(postImages);
                    context.SaveChanges();
                }
            }

            // Seed Favorites (chỉ seed nếu đã có Users và Posts)
            if ((force || !context.Set<Favorite>().Any()) && context.Users.Any() && context.Posts.Any())
            {
                var userNormal = context.Users.FirstOrDefault(u => u.Email == "user1@example.com");
                var userPro1 = context.Users.FirstOrDefault(u => u.Email == "user2@example.com");
                var userPro3 = context.Users.FirstOrDefault(u => u.Email == "user3@example.com");
                var posts = context.Posts.OrderBy(p => p.Id).ToList();
                
                if (userNormal != null && userPro1 != null && userPro3 != null && posts.Count > 0)
                {
                    var favorites = new List<Favorite>();
                    
                    // Seed favorites cho các posts có sẵn (kiểm tra index tồn tại)
                    if (posts.Count > 0)
                    {
                        favorites.Add(new Favorite { UserId = userNormal.Id, PostId = posts[0].Id, CreatedFavorite = DateTime.Now.AddDays(-2) });
                    }
                    if (posts.Count > 2)
                    {
                        favorites.Add(new Favorite { UserId = userNormal.Id, PostId = posts[2].Id, CreatedFavorite = DateTime.Now.AddDays(-1) });
                    }
                    if (posts.Count > 4)
                    {
                        favorites.Add(new Favorite { UserId = userNormal.Id, PostId = posts[4].Id, CreatedFavorite = DateTime.Now.AddDays(-3) });
                    }
                    if (posts.Count > 1)
                    {
                        favorites.Add(new Favorite { UserId = userPro1.Id, PostId = posts[1].Id, CreatedFavorite = DateTime.Now.AddDays(-1) });
                    }
                    if (posts.Count > 3)
                    {
                        favorites.Add(new Favorite { UserId = userPro1.Id, PostId = posts[3].Id, CreatedFavorite = DateTime.Now.AddDays(-2) });
                    }
                    if (posts.Count > 0)
                    {
                        favorites.Add(new Favorite { UserId = userPro3.Id, PostId = posts[0].Id, CreatedFavorite = DateTime.Now.AddDays(-4) });
                    }
                    if (posts.Count > 5)
                    {
                        favorites.Add(new Favorite { UserId = userPro3.Id, PostId = posts[5].Id, CreatedFavorite = DateTime.Now.AddDays(-1) });
                    }
                    if (posts.Count > 6)
                    {
                        favorites.Add(new Favorite { UserId = userPro3.Id, PostId = posts[6].Id, CreatedFavorite = DateTime.Now.AddDays(-2) });
                    }
                    context.Set<Favorite>().AddRange(favorites);
                    context.SaveChanges();
                }
            }

            // Seed Reports (chỉ seed nếu đã có Users và Posts)
            if ((force || !context.Set<Report>().Any()) && context.Users.Any() && context.Posts.Any())
            {
                var userNormal = context.Users.FirstOrDefault(u => u.Email == "user1@example.com");
                var userPro1 = context.Users.FirstOrDefault(u => u.Email == "user2@example.com");
                var userPro3 = context.Users.FirstOrDefault(u => u.Email == "user3@example.com");
                var posts = context.Posts.OrderBy(p => p.Id).ToList();
                
                if (userNormal != null && userPro1 != null && userPro3 != null && posts.Count > 0)
                {
                    var reports = new List<Report>();
                    
                    // Seed reports cho các posts có sẵn (kiểm tra index tồn tại)
                    if (posts.Count > 7)
                    {
                        reports.Add(new Report
                        {
                            UserId = userNormal.Id,
                            PostId = posts[7].Id,
                            Type = ReportType.TrungLap,
                            Other = "Bài đăng này trùng lặp với bài đăng khác",
                            Phone = "0987654321",
                            CreatedReport = DateTime.Now.AddDays(-6),
                            IsHandled = false
                        });
                        reports.Add(new Report
                        {
                            UserId = userPro1.Id,
                            PostId = posts[7].Id,
                            Type = ReportType.ThongTinSaiBatDongSan,
                            Other = "Thông tin về diện tích không chính xác",
                            Phone = "0912345678",
                            CreatedReport = DateTime.Now.AddDays(-5),
                            IsHandled = false
                        });
                        reports.Add(new Report
                        {
                            UserId = userPro3.Id,
                            PostId = posts[7].Id,
                            Type = ReportType.KhongLienLacDuoc,
                            Other = "Không thể liên lạc với người đăng",
                            Phone = "0923456789",
                            CreatedReport = DateTime.Now.AddDays(-4),
                            IsHandled = true
                        });
                    }
                    if (posts.Count > 0)
                    {
                        reports.Add(new Report
                        {
                            UserId = userNormal.Id,
                            PostId = posts[0].Id,
                            Type = ReportType.Other,
                            Other = "Giá cả không phù hợp với thị trường",
                            Phone = "0987654321",
                            CreatedReport = DateTime.Now.AddDays(-3),
                            IsHandled = false
                        });
                    }
                    context.Set<Report>().AddRange(reports);
                    context.SaveChanges();
                }
            }

            // Seed Notifications (chỉ seed nếu đã có Users)
            // Controlled by seedNotifications parameter to avoid creating notifications in dev by default.
            if (seedNotifications && (force || !context.Set<Notification>().Any()) && context.Users.Any())
            {
                var userNormal = context.Users.FirstOrDefault(u => u.Email == "user1@example.com");
                var userPro1 = context.Users.FirstOrDefault(u => u.Email == "user2@example.com");
                var userPro3 = context.Users.FirstOrDefault(u => u.Email == "user3@example.com");
                var posts = context.Posts.OrderBy(p => p.Id).ToList();
                
                if (userNormal != null && userPro1 != null && userPro3 != null && posts.Count > 0)
                {
                    var notifications = new List<Notification>();
                    
                    // Seed notifications cho các posts có sẵn (kiểm tra index tồn tại)
                    if (posts.Count > 0)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = userNormal.Id,
                            PostId = posts[0].Id,
                            Title = "Bài đăng của bạn đã được duyệt",
                            Message = "Bài đăng 'Biệt thự sang trọng tại Ba Đình, Hà Nội' đã được phê duyệt và hiển thị trên hệ thống.",
                            Type = "PostApproved",
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            IsRead = true
                        });
                        notifications.Add(new Notification
                        {
                            UserId = userPro1.Id,
                            PostId = posts[0].Id,
                            Title = "Bạn có tin nhắn mới",
                            Message = "Bạn có tin nhắn mới từ Nguyễn Văn A về bài đăng 'Biệt thự sang trọng tại Ba Đình, Hà Nội'.",
                            Type = "Message",
                            CreatedAt = DateTime.UtcNow.AddDays(-5).AddHours(-2),
                            IsRead = true
                        });
                    }
                    if (posts.Count > 1)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = userPro1.Id,
                            PostId = posts[1].Id,
                            Title = "Có người quan tâm đến bài đăng của bạn",
                            Message = "Nguyễn Văn A đã thêm bài đăng của bạn vào danh sách yêu thích.",
                            Type = "Favorite",
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            IsRead = false
                        });
                    }
                    if (posts.Count > 3)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = userPro1.Id,
                            PostId = posts[3].Id,
                            Title = "Bạn có tin nhắn mới",
                            Message = "Bạn có tin nhắn mới từ Lê Văn C về bài đăng 'Đất nền dự án Quận 2, TP. HCM - Sổ hồng'.",
                            Type = "Message",
                            CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(-3),
                            IsRead = false
                        });
                    }
                    if (posts.Count > 5)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = userPro3.Id,
                            PostId = posts[5].Id,
                            Title = "Bài đăng của bạn đã được duyệt",
                            Message = "Bài đăng 'Văn phòng cho thuê mặt tiền Quận 1' đã được phê duyệt và hiển thị trên hệ thống.",
                            Type = "PostApproved",
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            IsRead = true
                        });
                    }
                    if (posts.Count > 7)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = userNormal.Id,
                            PostId = posts[7].Id,
                            Title = "Bài đăng của bạn cần được xem xét",
                            Message = "Bài đăng 'Căn hộ studio cho thuê ngắn hạn - Quận 1' đang chờ được phê duyệt.",
                            Type = "PostPending",
                            CreatedAt = DateTime.UtcNow.AddDays(-8),
                            IsRead = true
                        });
                    }
                    // Notification welcome không cần PostId
                    notifications.Add(new Notification
                    {
                        UserId = userPro3.Id,
                        PostId = null,
                        Title = "Chào mừng bạn đến với Real Estate Hub",
                        Message = "Cảm ơn bạn đã tham gia Real Estate Hub. Hãy bắt đầu đăng bài để bán/cho thuê bất động sản của bạn!",
                        Type = "Welcome",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        IsRead = true
                    });
                    context.Set<Notification>().AddRange(notifications);
                    context.SaveChanges();
                }
            }

            Console.WriteLine("Database seeding completed successfully!");
        }
    }
}


using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        // Fixed GUIDs for consistent seeding
        var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var regularUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var dubaiCityId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var abuDhabiCityId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var ammanCityId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var hotel1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var hotel2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var hotel3Id = Guid.Parse("88888888-8888-8888-8888-888888888888");

        var roomType1Id = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var roomType2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var roomType3Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var amenity1Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var amenity2Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var amenity3Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var amenity4Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var room1Id = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var room2Id = Guid.Parse("11111111-2222-3333-4444-666666666666");
        var room3Id = Guid.Parse("11111111-2222-3333-4444-777777777777");
        var room4Id = Guid.Parse("11111111-2222-3333-4444-888888888888");

        var now = DateTime.UtcNow;

        // Seed Users
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Seeding Users...");

            // Admin user: admin@gmail.com / admin
            var adminSalt = GenerateSalt();
            var adminHash = await HashPasswordAsync("admin", adminSalt);

            // Regular user: user@gmail.com / user123
            var userSalt = GenerateSalt();
            var userHash = await HashPasswordAsync("user123", userSalt);

            var users = new List<User>
            {
                new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Email = "admin@gmail.com",
                    PasswordHash = adminHash,
                    PasswordSalt = adminSalt,
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "+971500000000",
                    Role = UserRole.Admin,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new User
                {
                    Id = regularUserId,
                    Username = "johndoe",
                    Email = "user@gmail.com",
                    PasswordHash = userHash,
                    PasswordSalt = userSalt,
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "+971501234567",
                    Role = UserRole.User,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("Users seeded successfully");
        }

        // Seed Cities
        if (!await context.Cities.AnyAsync())
        {
            logger.LogInformation("Seeding Cities...");

            var cities = new List<City>
            {
                new City
                {
                    Id = dubaiCityId,
                    Name = "Dubai",
                    Country = "United Arab Emirates",
                    PostOffice = "00000",
                    ThumbnailUrl = "",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new City
                {
                    Id = abuDhabiCityId,
                    Name = "Abu Dhabi",
                    Country = "United Arab Emirates",
                    PostOffice = "00001",
                    ThumbnailUrl = "",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new City
                {
                    Id = ammanCityId,
                    Name = "Amman",
                    Country = "Jordan",
                    PostOffice = "11110",
                    ThumbnailUrl = "",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();
            logger.LogInformation("Cities seeded successfully");
        }

        // Seed Room Types
        if (!await context.RoomTypes.AnyAsync())
        {
            logger.LogInformation("Seeding Room Types...");

            var roomTypes = new List<RoomType>
            {
                new RoomType
                {
                    Id = roomType1Id,
                    Name = "Standard",
                    Description = "A comfortable standard room with essential amenities",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new RoomType
                {
                    Id = roomType2Id,
                    Name = "Deluxe",
                    Description = "A spacious deluxe room with premium amenities",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new RoomType
                {
                    Id = roomType3Id,
                    Name = "Suite",
                    Description = "A luxurious suite with separate living area",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.RoomTypes.AddRangeAsync(roomTypes);
            await context.SaveChangesAsync();
            logger.LogInformation("Room Types seeded successfully");
        }

        // Seed Amenities
        if (!await context.Amenities.AnyAsync())
        {
            logger.LogInformation("Seeding Amenities...");

            var amenities = new List<Amenity>
            {
                new Amenity
                {
                    Id = amenity1Id,
                    Name = "Free WiFi",
                    Description = "High-speed wireless internet access",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Amenity
                {
                    Id = amenity2Id,
                    Name = "Swimming Pool",
                    Description = "Outdoor swimming pool with sun loungers",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Amenity
                {
                    Id = amenity3Id,
                    Name = "Fitness Center",
                    Description = "Fully equipped gym with modern equipment",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Amenity
                {
                    Id = amenity4Id,
                    Name = "Free Parking",
                    Description = "Complimentary parking for hotel guests",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Amenities.AddRangeAsync(amenities);
            await context.SaveChangesAsync();
            logger.LogInformation("Amenities seeded successfully");
        }

        // Seed Hotels
        if (!await context.Hotels.AnyAsync())
        {
            logger.LogInformation("Seeding Hotels...");

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Id = hotel1Id,
                    Name = "Grand Dubai Hotel",
                    Description = "A luxurious 5-star hotel in the heart of Dubai",
                    StarRating = 5,
                    CityId = dubaiCityId,
                    OwnerId = adminUserId,
                    Address = "Sheikh Zayed Road, Downtown Dubai",
                    Latitude = 25.2048m,
                    Longitude = 55.2708m,
                    ThumbnailUrl = "",
                    MinRoomPrice = 500,
                    DiscountPercentage = 10,
                    NearbyAttractions = "Burj Khalifa, Dubai Mall, Dubai Fountain",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Hotel
                {
                    Id = hotel2Id,
                    Name = "Abu Dhabi Palace",
                    Description = "A premium 4-star hotel near the Corniche",
                    StarRating = 4,
                    CityId = abuDhabiCityId,
                    OwnerId = adminUserId,
                    Address = "Corniche Road, Abu Dhabi",
                    Latitude = 24.4539m,
                    Longitude = 54.3773m,
                    ThumbnailUrl = "",
                    MinRoomPrice = 350,
                    DiscountPercentage = 15,
                    NearbyAttractions = "Emirates Palace, Corniche Beach, Heritage Village",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Hotel
                {
                    Id = hotel3Id,
                    Name = "Amman Royal Inn",
                    Description = "A cozy 3-star hotel in downtown Amman",
                    StarRating = 3,
                    CityId = ammanCityId,
                    OwnerId = null,
                    Address = "Rainbow Street, Amman",
                    Latitude = 31.9539m,
                    Longitude = 35.9106m,
                    ThumbnailUrl = "",
                    MinRoomPrice = 100,
                    DiscountPercentage = null,
                    NearbyAttractions = "Roman Amphitheater, Citadel, Rainbow Street",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();
            logger.LogInformation("Hotels seeded successfully");
        }

        // Seed Hotel Amenities
        if (!await context.HotelAmenities.AnyAsync())
        {
            logger.LogInformation("Seeding Hotel Amenities...");

            var hotelAmenities = new List<HotelAmenity>
            {
                // Grand Dubai Hotel - All amenities
                new HotelAmenity { HotelId = hotel1Id, AmenityId = amenity1Id },
                new HotelAmenity { HotelId = hotel1Id, AmenityId = amenity2Id },
                new HotelAmenity { HotelId = hotel1Id, AmenityId = amenity3Id },
                new HotelAmenity { HotelId = hotel1Id, AmenityId = amenity4Id },

                // Abu Dhabi Palace - WiFi, Pool, Gym
                new HotelAmenity { HotelId = hotel2Id, AmenityId = amenity1Id },
                new HotelAmenity { HotelId = hotel2Id, AmenityId = amenity2Id },
                new HotelAmenity { HotelId = hotel2Id, AmenityId = amenity3Id },

                // Amman Royal Inn - WiFi, Parking
                new HotelAmenity { HotelId = hotel3Id, AmenityId = amenity1Id },
                new HotelAmenity { HotelId = hotel3Id, AmenityId = amenity4Id }
            };

            await context.HotelAmenities.AddRangeAsync(hotelAmenities);
            await context.SaveChangesAsync();
            logger.LogInformation("Hotel Amenities seeded successfully");
        }

        // Seed Rooms
        if (!await context.Rooms.AnyAsync())
        {
            logger.LogInformation("Seeding Rooms...");

            var rooms = new List<Room>
            {
                // Grand Dubai Hotel Rooms
                new Room
                {
                    Id = room1Id,
                    HotelId = hotel1Id,
                    RoomTypeId = roomType3Id, // Suite
                    RoomNumber = "101",
                    PricePerNight = 800,
                    AdultCapacity = 2,
                    ChildCapacity = 2,
                    IsAvailable = true,
                    Description = "Luxury suite with city view",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Room
                {
                    Id = room2Id,
                    HotelId = hotel1Id,
                    RoomTypeId = roomType2Id, // Deluxe
                    RoomNumber = "102",
                    PricePerNight = 500,
                    AdultCapacity = 2,
                    ChildCapacity = 1,
                    IsAvailable = true,
                    Description = "Deluxe room with ocean view",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Abu Dhabi Palace Rooms
                new Room
                {
                    Id = room3Id,
                    HotelId = hotel2Id,
                    RoomTypeId = roomType2Id, // Deluxe
                    RoomNumber = "201",
                    PricePerNight = 350,
                    AdultCapacity = 2,
                    ChildCapacity = 1,
                    IsAvailable = true,
                    Description = "Deluxe room with Corniche view",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // Amman Royal Inn Rooms
                new Room
                {
                    Id = room4Id,
                    HotelId = hotel3Id,
                    RoomTypeId = roomType1Id, // Standard
                    RoomNumber = "301",
                    PricePerNight = 100,
                    AdultCapacity = 2,
                    ChildCapacity = 0,
                    IsAvailable = true,
                    Description = "Cozy standard room",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
            logger.LogInformation("Rooms seeded successfully");
        }

        // Seed Reviews
        if (!await context.Reviews.AnyAsync())
        {
            logger.LogInformation("Seeding Reviews...");

            var reviews = new List<Review>
            {
                new Review
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotel1Id,
                    UserId = regularUserId,
                    Rating = 5,
                    Content = "Amazing hotel with excellent service! The rooms are spacious and clean. Highly recommended!",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Review
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotel2Id,
                    UserId = regularUserId,
                    Rating = 4,
                    Content = "Great location and friendly staff. The pool area is beautiful.",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
            logger.LogInformation("Reviews seeded successfully");
        }

        // Seed Cart for regular user
        if (!await context.Carts.AnyAsync())
        {
            logger.LogInformation("Seeding Carts...");

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = regularUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await context.Carts.AddAsync(cart);
            await context.SaveChangesAsync();
            logger.LogInformation("Carts seeded successfully");
        }

        logger.LogInformation("Database seeding completed successfully");
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static async Task<string> HashPasswordAsync(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = 4,
            MemorySize = 65536,
            Iterations = 4
        };

        var hashBytes = await argon2.GetBytesAsync(32);
        return Convert.ToBase64String(hashBytes);
    }
}

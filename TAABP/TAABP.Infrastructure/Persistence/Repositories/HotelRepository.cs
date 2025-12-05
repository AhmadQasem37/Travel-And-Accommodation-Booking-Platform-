using Microsoft.EntityFrameworkCore;
using TAABP.Application.Common;
using TAABP.Application.DTOs.Hotels;
using TAABP.Application.Features.Hotels.Queries.SearchHotels;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Domain.Enums;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class HotelRepository(ApplicationDbContext context) : IHotelRepository
{
    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public void Add(Hotel entity) => context.Hotels.Add(entity);

    public void Update(Hotel entity) => context.Hotels.Update(entity);

    public void Remove(Hotel entity) => context.Hotels.Remove(entity);

    public async Task<PagedResult<SearchHotelDto>> SearchAsync(
        SearchHotelsQuery query,
        CancellationToken cancellationToken = default)
    {
        var checkInDate = query.CheckInDate ?? DateOnly.FromDateTime(DateTime.Today);
        var checkOutDate = query.CheckOutDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var hotelsQuery = context.Hotels
            .AsNoTracking()
            .Include(h => h.City)
            .Include(h => h.HotelAmenities)
                .ThenInclude(ha => ha.Amenity)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.CartItems)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.BookingItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            var searchTerm = query.SearchQuery;
            hotelsQuery = hotelsQuery.Where(h =>
                EF.Functions.Like(h.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(h.City.Name, $"%{searchTerm}%"));
        }

        if (query.StarRatings is { Length: > 0 })
        {
            hotelsQuery = hotelsQuery.Where(h => query.StarRatings.Contains(h.StarRating));
        }

        if (query.MinPrice.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.MinRoomPrice >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            hotelsQuery = hotelsQuery.Where(h => h.MinRoomPrice <= query.MaxPrice.Value);
        }

        if (query.AmenityIds is { Length: > 0 })
        {
            hotelsQuery = hotelsQuery.Where(h =>
                h.HotelAmenities.Any(ha => query.AmenityIds.Contains(ha.AmenityId)));
        }

        if (query.RoomTypeIds is { Length: > 0 })
        {
            hotelsQuery = hotelsQuery.Where(h =>
                h.Rooms.Any(r => query.RoomTypeIds.Contains(r.RoomTypeId)));
        }

        hotelsQuery = hotelsQuery.Where(h =>
            h.Rooms.Any(r =>
                r.AdultCapacity >= query.Adults &&
                r.ChildCapacity >= query.Children &&
                !r.CartItems.Any(ci =>
                    ci.CheckOutDate > checkInDate && ci.CheckInDate < checkOutDate) &&
                !r.BookingItems.Any(bi =>
                    bi.CheckOutDate > checkInDate && bi.CheckInDate < checkOutDate)));

        hotelsQuery = query.SortBy switch
        {
            HotelSortBy.StarRating => query.SortOrder == SortOrder.Desc
                ? hotelsQuery.OrderByDescending(h => h.StarRating)
                : hotelsQuery.OrderBy(h => h.StarRating),
            _ => query.SortOrder == SortOrder.Desc
                ? hotelsQuery.OrderByDescending(h => h.MinRoomPrice)
                : hotelsQuery.OrderBy(h => h.MinRoomPrice)
        };

        var totalCount = await hotelsQuery.CountAsync(cancellationToken);

        var hotels = await hotelsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(h => new SearchHotelDto(
                h.Id,
                h.Name,
                h.City.Name,
                h.City.Country,
                h.StarRating,
                h.ThumbnailUrl,
                h.Description,
                h.MinRoomPrice,
                h.DiscountPercentage.HasValue
                    ? h.MinRoomPrice * (1 - h.DiscountPercentage.Value / 100m)
                    : null,
                h.DiscountPercentage,
                h.HotelAmenities.Select(ha => ha.Amenity.Name).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<SearchHotelDto>(hotels, query.Page, query.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<FeaturedDealDto>> GetFeaturedDealsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .AsNoTracking()
            .Include(h => h.City)
            .Where(h => h.DiscountPercentage.HasValue && h.DiscountPercentage > 0)
            .OrderByDescending(h => h.DiscountPercentage)
            .Take(count)
            .Select(h => new FeaturedDealDto(
                h.Id,
                h.Name,
                h.City.Name,
                h.City.Country,
                h.StarRating,
                h.ThumbnailUrl,
                h.MinRoomPrice,
                h.MinRoomPrice * (1 - h.DiscountPercentage!.Value / 100m),
                h.DiscountPercentage.Value))
            .ToListAsync(cancellationToken);
    }
}

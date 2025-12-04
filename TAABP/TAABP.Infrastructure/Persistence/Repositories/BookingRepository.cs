using Microsoft.EntityFrameworkCore;
using TAABP.Application.Interfaces.Repositories;
using TAABP.Domain.Entities;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(ApplicationDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public void Add(Booking entity) => context.Bookings.Add(entity);

    public void Update(Booking entity) => context.Bookings.Update(entity);

    public void Remove(Booking entity) => context.Bookings.Remove(entity);
}

using Microsoft.EntityFrameworkCore;
using Xunit;
using TAABP.Infrastructure.Persistence.Context;

namespace TAABP.Application.Tests.Integration.Fixtures;

/// <summary>
/// Fixture for integration tests using in-memory EF Core database
/// Provides IAsyncLifetime for proper async setup and teardown
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private ApplicationDbContext? _context;

    public ApplicationDbContext Context => _context ?? throw new InvalidOperationException("Context not initialized. Call InitializeAsync first.");

    /// <summary>
    /// Called before each test that uses this fixture
    /// Creates unique in-memory database and initializes schema
    /// </summary>
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid()) // Unique DB per test
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Called after each test that uses this fixture
    /// Cleans up database and disposes context
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
            _context = null;
        }
    }
}

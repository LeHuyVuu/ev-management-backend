using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Entities;
using OrderService.Extensions.Query;
using OrderService.Model;

namespace OrderService.Infrastructure.Repositories;

public class TestDriveRepository
{
    private readonly MyDbContext _context;

    public TestDriveRepository(MyDbContext context)
    {
        _context = context;
    }


    public async Task<PagedResult<TestDrife>> GetTestDrivesAsync(Guid? dealerId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.TestDrives
                .Include(t => t.Dealer)
                .Include(t => t.Customer)
                .Include(t => t.StaffUser)
                .Include(t => t.VehicleVersion)
                    .ThenInclude(vv => vv.Vehicle)
                .AsNoTracking()
                .OrderByDescending(t => t.DriveDate)
                .AsQueryable();

            if (dealerId.HasValue && dealerId.Value != Guid.Empty)
            {
                query = query.Where(t => t.DealerId == dealerId.Value);
            }

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving TestDrives: {ex.Message}", ex);
        }
    }


    public async Task<TestDrife?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.TestDrives
                .Include(t => t.Dealer)
                .Include(t => t.Customer)
                .Include(t => t.StaffUser)
                .Include(t => t.VehicleVersion)
                    .ThenInclude(vv => vv.Vehicle)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestDriveId == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving TestDrive with ID {id}: {ex.Message}", ex);
        }
    }


    public async Task<TestDrife> CreateAsync(TestDrife entity)
    {
        try
        {
            var testDrive = new TestDrife
            {
                TestDriveId = entity.TestDriveId == Guid.Empty ? Guid.NewGuid() : entity.TestDriveId,
                DealerId = entity.DealerId,
                CustomerId = entity.CustomerId,
                VehicleVersionId = entity.VehicleVersionId,
                StaffUserId = entity.StaffUserId,
                DriveDate = entity.DriveDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : entity.DriveDate,
                TimeSlot = entity.TimeSlot,
                ConfirmSms = entity.ConfirmSms,
                ConfirmEmail = entity.ConfirmEmail,
                Status = string.IsNullOrWhiteSpace(entity.Status) ? "Pending" : entity.Status
            };

            _context.TestDrives.Add(testDrive);
            await _context.SaveChangesAsync();

            // Load navigation properties sau khi tạo
            await _context.Entry(testDrive).Reference(t => t.Dealer).LoadAsync();
            await _context.Entry(testDrive).Reference(t => t.Customer).LoadAsync();
            await _context.Entry(testDrive).Reference(t => t.StaffUser).LoadAsync();
            await _context.Entry(testDrive).Reference(t => t.VehicleVersion).LoadAsync();
            await _context.Entry(testDrive.VehicleVersion).Reference(vv => vv.Vehicle).LoadAsync();

            return testDrive;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating TestDrive: {ex.Message}", ex);
        }
    }


    public async Task<TestDrife?> UpdateStatusAsync(Guid id, string status)
    {
        try
        {
            var testDrive = await _context.TestDrives
                .Include(t => t.Dealer)
                .Include(t => t.Customer)
                .Include(t => t.StaffUser)
                .Include(t => t.VehicleVersion)
                    .ThenInclude(vv => vv.Vehicle)
                .FirstOrDefaultAsync(t => t.TestDriveId == id);

            if (testDrive == null)
                return null;

            testDrive.Status = status;

            _context.TestDrives.Update(testDrive);
            await _context.SaveChangesAsync();

            return testDrive;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating TestDrive status: {ex.Message}", ex);
        }
    }
}

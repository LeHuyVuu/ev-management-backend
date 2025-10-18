using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Entities;
using OrderService.Extensions.Query;
using OrderService.Model;

namespace OrderService.Infrastructure.Repositories;

public class VehicleAllocationRepository
{
    private readonly MyDbContext  _context;

    public VehicleAllocationRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VehicleAllocation>> GetVehicleAllocations(Guid? dealerId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.VehicleAllocations
                .Include(a => a.Dealer)
                .Include(a => a.VehicleVersion)
                .ThenInclude(vv => vv.Vehicle)
                .AsNoTracking()
                .OrderByDescending(a => a.RequestDate)
                .AsQueryable();

            if (dealerId.HasValue && dealerId.Value != Guid.Empty)
            {
                query = query.Where(a => a.DealerId == dealerId.Value);
            }

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving VehicleAllocations: {ex.Message}");
        }
    }

    public async Task<VehicleAllocation> CreateAsync(VehicleAllocation entity)
    {
        var allocation = new VehicleAllocation
        {
            AllocationId = entity.AllocationId == Guid.Empty ? Guid.NewGuid() : entity.AllocationId,
            DealerId = entity.DealerId,
            VehicleVersionId = entity.VehicleVersionId,
            Quantity = entity.Quantity,
            RequestDate = entity.RequestDate == default
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : entity.RequestDate,
            ExpectedDelivery = entity.ExpectedDelivery,
            Status = string.IsNullOrWhiteSpace(entity.Status) ? "Pending" : entity.Status
        };

        _context.VehicleAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        await _context.Entry(allocation).Reference(a => a.Dealer).LoadAsync();
        await _context.Entry(allocation).Reference(a => a.VehicleVersion).LoadAsync();
        await _context.Entry(allocation.VehicleVersion).Reference(vv => vv.Vehicle).LoadAsync();

        return allocation;
    }

    public async Task<VehicleAllocation?> UpdateStatusAsync(Guid id, string status)
    {
        var allocation = await _context.VehicleAllocations
            .Include(a => a.Dealer)
            .Include(a => a.VehicleVersion)
            .ThenInclude(vv => vv.Vehicle)
            .FirstOrDefaultAsync(a => a.AllocationId == id);

        if (allocation == null)
            return null;

        allocation.Status = status;
        _context.VehicleAllocations.Update(allocation);
        await _context.SaveChangesAsync();

        return allocation;
    }
}
using Microsoft.EntityFrameworkCore;
using OrderService.Context;
using OrderService.Entities;
using OrderService.Extensions.Query;
using OrderService.Model;

namespace OrderService.Infrastructure.Repositories
{
    public class VehicleTransferOrderRepository
    {
        private readonly MyDbContext _context;

        public VehicleTransferOrderRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<VehicleTransferOrder>> GetVehicleTransferOrders(Guid? dealerId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.VehicleTransferOrders
                    .Include(o => o.FromDealer)
                    .Include(o => o.ToDealer)
                    .Include(o => o.VehicleVersion)
                    .ThenInclude(vv => vv.Vehicle)
                    .AsNoTracking()
                    .OrderByDescending(o => o.RequestDate)
                    .AsQueryable();

                if (dealerId.HasValue && dealerId.Value != Guid.Empty)
                {
                    query = query.Where(o => o.FromDealerId == dealerId.Value || o.ToDealerId == dealerId.Value);
                }

                return await query.ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving VehicleTransferOrders: {ex.Message}");
            }
        }


        public async Task<VehicleTransferOrder> CreateAsync(VehicleTransferOrder entity)
        {
            try
            {
                var order = new VehicleTransferOrder
                {
                    VehicleTransferOrderId = entity.VehicleTransferOrderId == Guid.Empty
                        ? Guid.NewGuid()
                        : entity.VehicleTransferOrderId,
                    FromDealerId = entity.FromDealerId,
                    ToDealerId = entity.ToDealerId,
                    VehicleVersionId = entity.VehicleVersionId,
                    Quantity = entity.Quantity,
                    RequestDate = entity.RequestDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Status = string.IsNullOrWhiteSpace(entity.Status) ? "Pending" : entity.Status
                };

                _context.VehicleTransferOrders.Add(order);
                await _context.SaveChangesAsync();

                await _context.Entry(order).Reference(o => o.FromDealer).LoadAsync();
                await _context.Entry(order).Reference(o => o.ToDealer).LoadAsync();
                await _context.Entry(order).Reference(o => o.VehicleVersion).LoadAsync();
                await _context.Entry(order.VehicleVersion!).Reference(vv => vv.Vehicle).LoadAsync();

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating VehicleTransferOrder: {ex.Message}");
            }
        }

        public async Task<VehicleTransferOrder?> UpdateStatusAsync(Guid id, string status)
        {
            try
            {
                var order = await _context.VehicleTransferOrders
                    .Include(o => o.FromDealer)
                    .Include(o => o.ToDealer)
                    .Include(o => o.VehicleVersion)
                        .ThenInclude(vv => vv.Vehicle)
                    .FirstOrDefaultAsync(o => o.VehicleTransferOrderId == id);

                if (order == null)
                    return null;

                order.Status = status;
                _context.VehicleTransferOrders.Update(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating VehicleTransferOrder status: {ex.Message}");
            }
        }
        public async Task<VehicleTransferOrder?> GetByIdAsync(Guid id)
        {
            return await _context.VehicleTransferOrders
                .Include(a => a.VehicleVersion)
                .ThenInclude(vv => vv.Vehicle)
                .FirstOrDefaultAsync(a => a.VehicleTransferOrderId == id);
        }
    }
}

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

        // ✅ 1️⃣ Lấy danh sách Transfer Orders có phân trang + lọc
        public async Task<PagedResult<VehicleTransferOrder>> GetVehicleTransferOrders(Guid? fromDealerId, Guid? toDealerId, int pageNumber, int pageSize)
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

                // Lọc theo FromDealer
                if (fromDealerId.HasValue && fromDealerId.Value != Guid.Empty)
                {
                    query = query.Where(o => o.FromDealerId == fromDealerId.Value);
                }

                // Lọc theo ToDealer
                if (toDealerId.HasValue && toDealerId.Value != Guid.Empty)
                {
                    query = query.Where(o => o.ToDealerId == toDealerId.Value);
                }

                return await query.ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving VehicleTransferOrders: {ex.Message}");
            }
        }

        // ✅ 2️⃣ Tạo mới Transfer Order
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

                // Load navigation properties
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

        // ✅ 3️⃣ Cập nhật trạng thái Transfer Order
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
    }
}

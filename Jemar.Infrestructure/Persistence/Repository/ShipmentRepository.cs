using Jemar.Domain.Entities;
using Jemar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ShipmentRepository : IShipmentRepository
{
    private readonly JemarDbContext _shipmentContext;

    public ShipmentRepository(JemarDbContext shipmentContext)
    {
        _shipmentContext = shipmentContext;
    }

    public async Task AddAsync(Shipment shipment)
    {
        await _shipmentContext.Shipments.AddAsync(shipment);
        await _shipmentContext.SaveChangesAsync();
    }

    public async Task<List<Shipment>> GetAllAsync()
    {
        return await _shipmentContext.Shipments
            .Include(x => x.ShipmentType)
            .Include(x => x.ShipmentStatus)
            .ToListAsync();
    }

    public async Task<Shipment?> GetByIdAsync(Guid id)
    {
        return await _shipmentContext.Shipments
            .Include(x => x.ShipmentType)
            .Include(x => x.ShipmentStatus)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        _shipmentContext.Shipments.Update(shipment);
        await _shipmentContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var shipment = await _shipmentContext.Shipments.FindAsync(id);

        if (shipment != null)
        {
            _shipmentContext.Shipments.Remove(shipment);
            await _shipmentContext.SaveChangesAsync();
        }
    }
}
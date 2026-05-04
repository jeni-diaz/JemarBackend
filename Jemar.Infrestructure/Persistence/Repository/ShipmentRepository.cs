using Jemar.Domain.Entities;
using Jemar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ShipmentRepository : IShipmentRepository
{
    private readonly JemarDbContext _context;

    public ShipmentRepository(JemarDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(x => x.ShipmentType)
            .Include(x => x.ShipmentStatus)
            .ToListAsync();
    }

    public async Task<Shipment?> GetByIdAsync(Guid id)
    {
        return await _context.Shipments
            .Include(x => x.ShipmentType)
            .Include(x => x.ShipmentStatus)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var shipment = await _context.Shipments.FindAsync(id);

        if (shipment != null)
        {
            _context.Shipments.Remove(shipment);
            await _context.SaveChangesAsync();
        }
    }
}
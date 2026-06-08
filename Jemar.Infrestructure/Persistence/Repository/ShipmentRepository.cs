using Jemar.Domain.Entities;
using Jemar.Domain.Enums;
using Jemar.Domain.Interfaces;
using Jemar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ShipmentRepository : IShipmentRepository
{
    // Variable privada para conectarnos a las tablas de la base de datos
    private readonly JemarDbContext _shipmentContext;

    // Constructor: Recibe e inyecta la configuración de la base de datos
    public ShipmentRepository(JemarDbContext shipmentContext)
    {
        _shipmentContext = shipmentContext;
    }

    // Método para guardar un nuevo envío en la base de datos
    public async Task AddAsync(Shipment shipment)
    {
        // Prepara el objeto en memoria para ser insertado
        await _shipmentContext.Shipments.AddAsync(shipment);
        // Guarda físicamente los cambios ejecutando el comando INSERT en SQL
        await _shipmentContext.SaveChangesAsync();
    }

    // Método para obtener TODOS los envíos del sistema (Filtrando los que fueron borrados lógicamente)
    public async Task<List<Shipment>> GetAllAsync()
    {
        // Vamos a la tabla de envíos...
        return await _shipmentContext.Shipments
            .Include(x => x.ShipmentType)   // Trae los datos de la tabla relacionada de Tipos de envío (JOIN en SQL)
            .Include(x => x.ShipmentStatus) // Trae los datos de la tabla relacionada de Estados de envío (JOIN en SQL)

            //Filtramos en SQL para traer solo los envíos cuyo estado NO sea igual a Deleted (5)
            .Where(x => x.ShipmentStatusId != (int)ShipmentStatusEnum.Deleted)

            .ToListAsync(); // Ejecuta la consulta final en la base de datos y la transforma en una lista de C#
    }

    public async Task<List<Shipment>> GetByClientIdAsync(Guid clientId)
    {
        // Vamos a la tabla de envíos...
        return await _shipmentContext.Shipments
            .Include(x => x.ShipmentType)   // También traemos los datos de su Tipo de envío
            .Include(x => x.ShipmentStatus) // También traemos los datos de su Estado de envío
            .Where(x => x.ClientId == clientId) // Filtramos en SQL: "Dame solo los registros donde el ClientId coincida con el que pasé por parámetro"
            .ToListAsync(); // Convertimos el filtro en una lista final de C#
    }

    // Método para buscar un solo envío por su clave primaria (ID)
    public async Task<Shipment?> GetByIdAsync(Guid id)
    {
        return await _shipmentContext.Shipments
            .Include(x => x.ShipmentType)   // Incluye los datos del tipo
            .Include(x => x.ShipmentStatus) // Incluye los datos del estado
            .FirstOrDefaultAsync(x => x.Id == id); // Devuelve el primero que encuentre con ese ID, o null si no existe
    }

    // Método para actualizar los datos modificados de un envío existente
    public async Task UpdateAsync(Shipment shipment)
    {
        // Marca la entidad como modificada para que Entity Framework sepa qué actualizar
        _shipmentContext.Shipments.Update(shipment);
        // Guarda los cambios ejecutando el comando UPDATE en SQL
        await _shipmentContext.SaveChangesAsync();
    }

    // Método para eliminar LÓGICAMENTE un envío cambiando su estado por base de datos
    public async Task DeleteAsync(Guid id)
    {
        // Buscamos primero el registro completo por su ID en la base de datos
        var shipment = await _shipmentContext.Shipments.FindAsync(id);

        // Si el envío existe (no es nulo), procedemos a cambiar su estado
        if (shipment != null)
        {
            // Cambiamos su ShipmentStatusId al número que representa "Deleted"
            shipment.ShipmentStatusId = (int)ShipmentStatusEnum.Deleted;

            // Registramos la fecha exacta UTC en la que ocurrió esta "eliminación"
            shipment.UpdatedAt = DateTime.UtcNow;

            // Le avisamos a Entity Framework que actualice este registro modificado
            _shipmentContext.Shipments.Update(shipment);

            // Confirmamos los cambios ejecutando un comando UPDATE en SQL (en lugar de un DELETE)
            await _shipmentContext.SaveChangesAsync();
        }
    }
}
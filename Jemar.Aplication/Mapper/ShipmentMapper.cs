using Jemar.Aplication.Requests; // DTOs de entrada (datos que llegan desde la API)
using Jemar.Aplication.Responses; // DTOs de salida (datos que se devuelven al cliente)
using Jemar.Domain.Entities; // Entidades del dominio

public static class ShipmentMapper // Clase estática que contiene métodos de conversión (mapeo)
{
    public static Shipment ToShipment(this CreateShipmentRequest request, Guid clientId) // Convierte un CreateShipmentRequest en una entidad Shipment. El "this" permite usarlo como método de extensión: request.ToShipment(clientId)
    {
        return new Shipment
        {
            Id = Guid.NewGuid(), // Genera un nuevo ID para el envío
            ClientId = clientId, // Asigna el cliente propietario del envío
            Origin = request.Origin, // Copia los datos recibidos en el request
            Destination = request.Destination, // Copia los datos recibidos en el request
            Price = request.Price, // Copia los datos recibidos en el request
            ShipmentTypeId = request.ShipmentTypeId, // Tipo de envío seleccionado
            ShipmentStatusId = 1, // Estado inicial
            CreatedAt = DateTime.UtcNow, // Fecha de creació
            UpdatedAt = DateTime.UtcNow // Fecha de última actualización
        };
    }

    public static ShipmentResponse ToShipmentResponse(this Shipment shipment) // Convierte una entidad Shipment en un ShipmentResponse para devolver información al frontend o consumidor de la API
    {
        return new ShipmentResponse
        {
            Id = shipment.Id, // Id de envío
            Origin = shipment.Origin, // Origen del envío
            Destination = shipment.Destination, // Destino del envío
            Price = shipment.Price, // Precio del envío
            ShipmentType = shipment.ShipmentType?.Name.ToString()?? string.Empty, // Obtiene el nombre del tipo. Si ShipmentType es null devuelve una cadena vacía
            ShipmentStatus = shipment.ShipmentStatus?.Name.ToString()?? string.Empty // Obtiene el nombre del estado. Si ShipmentStatus es null devuelve una cadena vacía
        };
    }

    public static List<ShipmentResponse> ToShipmentListResponse(this List<Shipment> shipments)   // Convierte una lista de Shipment en una lista de ShipmentResponse
    {
        return shipments.Select(s => s.ToShipmentResponse()).ToList(); // Recorre cada Shipment y lo convierte individualmente
    }
}
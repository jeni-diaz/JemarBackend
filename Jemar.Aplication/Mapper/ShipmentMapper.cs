using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using Jemar.Domain.Entities;

// Una clase estática sirve como caja de herramientas: contiene funciones que se pueden usar en cualquier lado sin crear un objeto "new ShipmentMapper()"
public static class ShipmentMapper
{
    // Método para convertir los datos que llegan de la API (Request) en un objeto de la base de datos (Shipment)
    // El "this" al principio del parámetro hace que C# lo trate como un método de extensión (permite escribir: request.ToShipment(clientId))
    public static Shipment ToShipment(this CreateShipmentRequest request, Guid clientId)
    {
        // Creamos y devolvemos una nueva instancia de la entidad Shipment con sus datos llenos
        return new Shipment
        {
            // Generamos un identificador único global (GUID) único para este envío en la base de datos
            Id = Guid.NewGuid(),

            // Le asignamos el ID del cliente que pasamos por parámetro, para saber a quién le pertenece el paquete
            ClientId = clientId,

            // Copiamos la dirección de origen que el usuario escribió en el formulario de la API
            Origin = request.Origin,

            // Copiamos la dirección de destino que el usuario escribió en el formulario de la API
            Destination = request.Destination,

            // Copiamos el precio base (Nota: recuerda que en tu servicio este valor se sobreescribe según el tipo Standard/Express)
            Price = request.Price,

            // Guardamos el número de ID que representa el tipo de envío (ej: 1 para Standard, 2 para Express)
            ShipmentTypeId = request.ShipmentTypeId,

            // Forzamos a que todo envío nuevo arranque por defecto con el ID de estado número 1 (que corresponde a "Pending")
            ShipmentStatusId = 1,

            // Registramos el momento exacto actual en formato de hora internacional (UTC) como fecha de creación
            CreatedAt = DateTime.UtcNow,

            // Al crearse por primera vez, la fecha de última actualización es exactamente la misma fecha de creación
            UpdatedAt = DateTime.UtcNow
        };
    }

    // Método para convertir un envío de la base de datos (Shipment) en un formato seguro y limpio para enviar al frontend (Response DTO)
    public static ShipmentResponse ToShipmentResponse(this Shipment shipment)
    {
        // Creamos y devolvemos el objeto ShipmentResponse con la información digerida para el usuario
        return new ShipmentResponse
        {
            // Pasamos el ID del envío para que el frontend sepa cuál es
            Id = shipment.Id,

            // Pasamos el origen del envío
            Origin = shipment.Origin,

            // Pasamos el destino del envío
            Price = shipment.Price,

            // Pasamos el precio final del envío
            Destination = shipment.Destination,

            // El signo "?" (Null Conditional) evita que el programa falle si la relación "ShipmentType" vino vacía desde la BD.
            // El signo "??" (Null Coalescing) significa: "Si lo de la izquierda es nulo, entonces devuelve el texto vacío de la derecha (string.Empty)".
            ShipmentType = shipment.ShipmentType?.Name.ToString() ?? string.Empty,

            // Hacemos exactamente lo mismo para el estado: Si la relación ShipmentStatus existe, extraemos su nombre como texto; si es nula, devolvemos un texto vacío
            ShipmentStatus = shipment.ShipmentStatus?.Name.ToString() ?? string.Empty
        };
    }

    // Método para convertir una lista entera de envíos de la base de datos a una lista de respuestas para el frontend
    public static List<ShipmentResponse> ToShipmentListResponse(this List<Shipment> shipments)
    {
        // .Select() es un bucle de LINQ que agarra cada "Shipment" individual de la lista y lo transforma usando el método ToShipmentResponse() de arriba.
        // .ToList() agrupa todos esos nuevos elementos ya transformados y los guarda adentro de la lista final que vamos a retornar.
        return shipments.Select(s => s.ToShipmentResponse()).ToList();
    }
}
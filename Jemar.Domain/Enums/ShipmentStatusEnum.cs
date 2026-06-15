namespace Jemar.Domain.Enums
{
    public enum ShipmentStatusEnum
    {
        Pending = 1,   // El envío está registrado pero aún no ha salido
        InTransit = 2, // El paquete está viajando hacia el destino
        Delivered = 3, // El paquete ya fue entregado con éxito
        Cancelled = 4, // El envío fue cancelado por el usuario o la empresa
        Deleted = 5    // El envío fue eliminado
    }
}
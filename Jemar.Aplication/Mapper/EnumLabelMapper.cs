using Jemar.Domain.Enums;

namespace Jemar.Aplication.Mapper
{
    // Etiquetas en español para los enums que se muestran al usuario final
    // (por ejemplo en el correo de confirmación del envío). La API sigue
    // devolviendo los nombres en inglés y el front los traduce por su cuenta;
    // esto es solo para el texto que el backend genera directamente.
    public static class EnumLabelMapper
    {
        public static string ToSpanish(this ShipmentTypeEnum value) => value switch
        {
            ShipmentTypeEnum.Express => "Expreso",
            ShipmentTypeEnum.Standard => "Estándar",
            _ => value.ToString()
        };

        public static string ToSpanish(this PackageSizeEnum value) => value switch
        {
            PackageSizeEnum.Small => "Pequeño",
            PackageSizeEnum.Medium => "Mediano",
            PackageSizeEnum.Large => "Grande",
            _ => value.ToString()
        };

        public static string ToSpanish(this ShipmentStatusEnum value) => value switch
        {
            ShipmentStatusEnum.Pending => "Pendiente",
            ShipmentStatusEnum.InTransit => "En tránsito",
            ShipmentStatusEnum.Delivered => "Entregado",
            ShipmentStatusEnum.Cancelled => "Cancelado",
            _ => value.ToString()
        };
    }
}

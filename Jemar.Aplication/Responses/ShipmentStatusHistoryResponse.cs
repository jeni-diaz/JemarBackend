using System;

namespace Jemar.Aplication.Responses
{
    public class ShipmentStatusHistoryResponse
    {
        public string Status { get; set; } = string.Empty;
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}

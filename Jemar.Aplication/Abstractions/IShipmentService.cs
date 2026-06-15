using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    // Una interfaz actúa como un plano o contrato: obliga a la clase que la implemente (ShipmentService) a escribir el código real de estos métodos
    public interface IShipmentService
    {
        Task<List<ShipmentResponse>> GetAllAsync(Guid currentUserId, string currentUserRole);
        Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole);
        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid clientId);
        Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole);
        Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole);
    }
}
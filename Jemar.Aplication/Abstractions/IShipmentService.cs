using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    public interface IShipmentService
    {
        Task<List<ShipmentResponse>> GetAllAsync(Guid currentUserId, string currentUserRole);
        Task<ShipmentResponse?> GetByIdAsync(Guid id, Guid currentUserId, string currentUserRole);
        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, Guid currentUserId, string currentUserRole);
        Task<bool> UpdateStatusAsync(Guid id, UpdateShipmentRequest request, Guid currentUserId, string currentUserRole);
        Task<bool> DeleteAsync(Guid id, Guid currentUserId, string currentUserRole);
    }
}
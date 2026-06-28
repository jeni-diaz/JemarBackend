using Jemar.Aplication.Requests;
using Jemar.Aplication.Responses;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    public interface IAuthService
    {
        Task<AuthResponse?> SignInAsync(SignInRequest request);
        Task<AuthResponse> SignUpAsync(SignUpRequest request);
    }
}

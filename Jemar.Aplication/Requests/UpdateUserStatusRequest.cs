namespace Jemar.Aplication.Requests
{
    public class UpdateUserStatusRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

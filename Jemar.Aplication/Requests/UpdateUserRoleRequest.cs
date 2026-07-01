namespace Jemar.Aplication.Requests
{
    public class UpdateUserRoleRequest
    {
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}

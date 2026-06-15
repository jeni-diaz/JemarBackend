namespace Jemar.Presentation.Authorization
{
    public enum AuthorizationPolicy
    {
        SuperAdminOnly,
        EmployeeOrAbove,
        ClientOrAbove
    }

    public static class Policies
    {
        public const string SuperAdminOnly = nameof(AuthorizationPolicy.SuperAdminOnly);
        public const string EmployeeOrAbove = nameof(AuthorizationPolicy.EmployeeOrAbove);
        public const string ClientOrAbove = nameof(AuthorizationPolicy.ClientOrAbove);
    }
}


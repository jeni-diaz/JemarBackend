namespace Jemar.Aplication.Abstractions
{
    public interface IOpenStreetMapService
    {
        Task<List<string>> AutocompletarDireccionAsync(string query);
    }
}

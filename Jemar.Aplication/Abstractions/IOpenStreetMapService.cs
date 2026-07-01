namespace Jemar.Aplication.Abstractions
{
    public interface IOpenStreetMapService
    {
        Task<List<string>> AutocompleteAddressAsync(string query);
    }
}

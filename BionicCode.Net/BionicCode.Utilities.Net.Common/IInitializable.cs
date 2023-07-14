namespace BionicCode.Utilities.Net
{
  using System.Threading.Tasks;

  public interface IInitializable
  {
    Task<bool> InitializeAsync();
    bool IsInitialized { get; }
  }
}
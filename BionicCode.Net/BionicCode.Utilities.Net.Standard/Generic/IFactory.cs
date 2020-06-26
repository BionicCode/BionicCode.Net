namespace BionicCode.Utilities.Net.Standard.Generic
{
  public interface IFactory<out TCreate>
  {
    TCreate Create();
    TCreate Create(params object[] args);
  }
}

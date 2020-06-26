#region Info
// //  
// BionicUtilities.Net.Standard
#endregion
namespace BionicCode.Utilities.Net.Standard.Generic
{
  public abstract class NullObject<TObject> : INullObject where TObject : class
  {
    protected NullObject(bool isNull)
    {
      this.IsNull = isNull;
    }

    protected NullObject(IFactory<TObject> nullInstanceFactory)
    {
      NullObject<TObject>.NullInstanceFactory = nullInstanceFactory;
    }

    public static TObject NullInstance => NullObject<TObject>.NullInstanceFactory?.Create();
    #region Implementation of INullObject

    /// <inheritdoc />
    public bool IsNull { get; private set; }

    public static IFactory<TObject> NullInstanceFactory { get; private set; }

    #endregion
  }
}
namespace BionicCode.Controls.Net.Wpf
{
  public interface IDataConverter<TData, TResult> : IDataConverter
  {
    TResult Convert(TData dataItem);
//#if NET
//    object IDataConverter.Convert(object dataItem) => Convert((TData)dataItem);
//#endif
  }
}
namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  public interface IDataConverter<TData, TResult> : IDataConverter
  {
    TResult Convert(TData dataItem);
//#if NET
//    object IDataConverter.Convert(object dataItem) => Convert((TData)dataItem);
//#endif
  }
}
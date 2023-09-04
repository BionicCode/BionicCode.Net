namespace BionicCode.Utilities.Net
{
  using System;
  using System.CodeDom;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using System.Text.Json;

  internal interface IProfilerLogger
  {
    Task LogAsync(ProfilerBatchResultCollection batchResults);
  }
}
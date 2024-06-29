namespace BionicCode.Utilities.Net
{
  using System.Threading;
  using System.Text.Json;
/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard21)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net472)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net50)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net80)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard20)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net60)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net70)'
Before:
  using System.Text.Json;
  using System.Threading;
After:
  using System.Threading;
  using System.Threading.Tasks;
*/


  internal interface IProfilerLogger
  {
    Task LogAsync(ProfiledTypeResultCollection typeResults, CancellationToken cancellationToken);
  }
}
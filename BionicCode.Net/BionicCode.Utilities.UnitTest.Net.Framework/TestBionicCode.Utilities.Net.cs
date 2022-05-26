namespace BionicCode.Utilities.UnitTest.Net.Framework
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.ViewModel;
  using BionicCode.Utilities.Net.Wpf.ViewModel;

  internal class TestBionicCode
  {
    private void SomeCode()
    {
      var viewModel = new TestViewModel();
      new EventAggregator()
    }
  }

  internal class TestViewModel : ViewModel
  {

  }
}

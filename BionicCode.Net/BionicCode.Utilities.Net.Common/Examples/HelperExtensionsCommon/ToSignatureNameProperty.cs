namespace BionicCode.Utilities.Net.Examples.HelperExtensionsCommon
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;

  internal class ToSignatureNamePropertyExample
  {
    private void Demonstrate()
    {
      string signatureName = typeof(Task<int>).GetProperty(nameof(Task<int>.Result)).ToSignatureName();
      Console.WriteLine(signatureName); // "public bool Result { get; }"
    }
  }

  internal class ToSignatureNameFieldExample
  {
    private void Demonstrate()
    {
      string signatureName = typeof(EventArgs).GetField(nameof(EventArgs.Empty)).ToSignatureName();
      Console.WriteLine(signatureName); // "public static readonly EventArgs Empty;"
    }
  }

  internal class ToSignatureNameMethodExample
  {
    private void Demonstrate()
    {
      string signatureName = typeof(Task<>).GetMethod(nameof(Task.Run)).ToSignatureName();
      Console.WriteLine(signatureName); // "public static Task<TResult> Run<TResult>(Action action);"
    }
  }

  internal class ToSignatureNameConstructorExample
  {
    private void Demonstrate()
    {
      string signatureName = typeof(Task<>).GetConstructor(new[] {typeof(Func<>), typeof(CancellationToken) } ).ToSignatureName();
      Console.WriteLine(signatureName); // "public Task(Func<TResult> function, CancellationToken cancellationToken);"
    }
  }
}

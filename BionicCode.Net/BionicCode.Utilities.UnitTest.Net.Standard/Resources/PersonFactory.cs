namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;

  public class PersonFactory : Factory<Person>
  {
    public string DefaultPersonFirstName => "DefaultFirstName";
    public string DefaultPersonLastName => "DefaultLastName";
    public int DefaultPersonId => -1;
    protected override Person CreateInstance() => new Person(this.DefaultPersonFirstName, this.DefaultPersonLastName, this.DefaultPersonId);
    protected override Person CreateInstance(params object[] args) => new Person(args[0] as string, args[1] as string, (int)args[2]);
  }
}

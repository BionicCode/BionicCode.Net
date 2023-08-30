namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public class Person
  {
    public Person(string firstName, string lastName, int id)
    {
      this.FirstName = firstName;
      this.LastName = lastName;
      this.Id = id;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Id { get; set; }
  }
}

namespace BionicCode.Controls.Net.Core.Wpf
{
  public class Unit
  {
    public Unit(string name, decimal baseFactor)
    {
      this.Name = name;
      this.BaseFactor = baseFactor;
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => this.Name;

    #endregion

    public string Name { get; set; }
    public decimal BaseFactor { get; set; }
  }
}


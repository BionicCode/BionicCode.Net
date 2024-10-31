namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class TestContext
  {
    public const int ItemsCapacity = 50;
    public const int NewItemsCapacity = 10;

    public List<int> Items { get; private set; }
    public ICollection<int> ItemsBackup { get; }
    public Dictionary<int, int> ItemTable { get; private set; }
    public Dictionary<int, int> ItemTableBackup { get; }
    public Dictionary<int, int> NewTableItemsFromDictionary { get; }
    public IList<(int Key, int Value)> NewTableItemsFromTupleCollection { get; }
    public IList<KeyValuePair<int, int>> NewTableItemsFromKeyValuePairCollection { get; }
    public List<int> NewItems { get; }
    public ICollection<int> EmptyItems { get; }
    public Func<int, bool> FailContainsPredicate => item => item > this.Items.Last();
    public Func<int, bool> SuccessContainsPredicate => item => item < 5;

    public TestContext()
    {
      this.Items = new List<int>();
      this.ItemsBackup = new List<int>();
      this.NewItems = new List<int>();
      this.EmptyItems = new List<int>();

      this.ItemTable = new Dictionary<int, int>();
      this.ItemTableBackup = new Dictionary<int, int>();
      this.NewTableItemsFromDictionary = new Dictionary<int, int>();
      this.NewTableItemsFromTupleCollection = new List<(int, int)>();
      this.NewTableItemsFromKeyValuePairCollection = new List<KeyValuePair<int, int>>();

      for (int count = 0; count < ItemsCapacity; count++)
      {
        int key = count;
        int value = count * 10;
        this.ItemTable.Add(key, value);
        this.Items.Add(count);
        this.ItemTableBackup.Add(key, value);
        this.ItemsBackup.Add(count);
      }

      for (int count = ItemsCapacity; count < ItemsCapacity + NewItemsCapacity; count++)
      {
        int key = count;
        int value = count * 10;
        this.NewItems.Add(count);
        this.NewTableItemsFromTupleCollection.Add((key, value));
        this.NewTableItemsFromKeyValuePairCollection.Add(new KeyValuePair<int, int>(key, value));
        this.NewTableItemsFromDictionary.Add(key, value);
      }
    }

    public void Reset()
    {
      this.Items = new List<int>(this.ItemsBackup);
      this.ItemTable = new Dictionary<int, int>(this.ItemTableBackup);
    }
  }
}

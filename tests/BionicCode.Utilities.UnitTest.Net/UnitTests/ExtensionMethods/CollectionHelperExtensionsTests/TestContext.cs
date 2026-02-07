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
        public Func<int, bool> FailContainsPredicate => item => item > Items.Last();
        public Func<int, bool> SuccessContainsPredicate => item => item < 5;

        public TestContext()
        {
            Items = new List<int>();
            ItemsBackup = new List<int>();
            NewItems = new List<int>();
            EmptyItems = new List<int>();

            ItemTable = new Dictionary<int, int>();
            ItemTableBackup = new Dictionary<int, int>();
            NewTableItemsFromDictionary = new Dictionary<int, int>();
            NewTableItemsFromTupleCollection = new List<(int, int)>();
            NewTableItemsFromKeyValuePairCollection = new List<KeyValuePair<int, int>>();

            for (int count = 0; count < ItemsCapacity; count++)
            {
                int key = count;
                int value = count * 10;
                ItemTable.Add(key, value);
                Items.Add(count);
                ItemTableBackup.Add(key, value);
                ItemsBackup.Add(count);
            }

            for (int count = ItemsCapacity; count < ItemsCapacity + NewItemsCapacity; count++)
            {
                int key = count;
                int value = count * 10;
                NewItems.Add(count);
                NewTableItemsFromTupleCollection.Add((key, value));
                NewTableItemsFromKeyValuePairCollection.Add(new KeyValuePair<int, int>(key, value));
                NewTableItemsFromDictionary.Add(key, value);
            }
        }

        public void Reset()
        {
            Items = new List<int>(ItemsBackup);
            ItemTable = new Dictionary<int, int>(ItemTableBackup);
        }
    }
}

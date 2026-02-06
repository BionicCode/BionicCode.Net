namespace BionicCode.Utilities.Net.Profiling;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A list to hold a collection of <see cref="ProfilerBatchResult"/> items.
/// </summary>
public class ProfilerBatchResultGroup : IEnumerable<ProfilerBatchResult?>
{
    /// <summary>
    /// Describes the profiled member that the <see cref="ProfilerBatchResult"/> items represent.
    /// </summary>
    /// <value>The value can differ from the value of the individual items. 
    /// <br/>For example, this collection will have the value <see cref="ProfiledTargetType.Property"/> while the actual <see cref="ProfilerBatchResult.Context"/> has <see cref="ProfilerContext.TargetType"/> return <see cref="ProfiledTargetType.PropertyGet"/> and <see cref="ProfiledTargetType.PropertySet"/>.</value>
    internal ProfiledTargetType TargetType { get; set; }

    /// <summary>
    /// The signature of the profiled type including the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetNamespace { get; set; }
    internal string TargetAssemblyName { get; set; }
    internal string TargetSourceFileName { get; set; }
    internal int TargetSourceFileLineNumber { get; set; }
    internal string TargetSignature { get; set; }
    internal SymbolComponentInfo TargetSignatureComponentInfo { get; set; }

    /// <summary>
    /// The signature of the profiled type without the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetShortSignature { get; set; }

    /// <summary>
    /// The signature name of the profiled type without the declaring type (in case if the target is a member), attributes, generic constraints and inheritance list.
    /// </summary>
    internal string TargetShortCompactSignature { get; set; }

    /// <summary>
    /// The name of the profiled type including the declaring type (in case if the target is a member)..
    /// </summary>
    internal string TargetName { get; set; }

    /// <summary>
    /// The name of the profiled type without the declaring type (in case if the target is a member).
    /// </summary>
    internal string TargetShortName { get; set; }

    internal TimeUnit CommonBaseUnit => Min(result => result.BaseUnit);

    public int Count { get; private set; }
    public bool HasItems => Count > 0;
    public bool IsEmpty => Count == 0;
    public int Slots => _items.Length;
    public bool IsReadOnly => false;

    public ProfilerBatchResult? this[int index]
    {
        get
        {
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(index, 0, nameof(index), "Index cannot be negative.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(index, Slots, nameof(index), $"Index '{index}' exceeds the number of available slots '{Slots}'.");
            return _items[index];
        }
        set
        {
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(index, 0, nameof(index), "Index cannot be negative.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(index, Slots, nameof(index), $"Index '{index}' exceeds the number of available slots '{Slots}'.");

            AddOrReplaceInternal(value, index);
        }
    }

    private readonly ProfilerBatchResult?[] _items;

    internal ProfilerBatchResultGroup(int slots)
        : this(ProfiledTargetType.Undefined, slots)
    {
    }

    internal ProfilerBatchResultGroup(ProfiledTargetType profiledTargetType, int slots)
    {
        TargetType = profiledTargetType;
        _items = new ProfilerBatchResult[slots];
    }

    public void AddOrReplace(ProfilerBatchResult? item, int slot)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(slot, 0, nameof(slot), "Slot index cannot be negative.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(slot, Slots, nameof(slot), $"Slot index '{slot}' exceeds the number of available slots '{Slots}'.");

        AddOrReplaceInternal(item, slot);
    }

    protected virtual void AddOrReplaceInternal(ProfilerBatchResult? item, int slot)
    {
        if (_items[slot] is null && item is not null)
        {
            Count++;
        }
        else if (_items[slot] is not null && item is null)
        {
            Count--;
        }

        _items[slot] = item;
    }

    public void Clear()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = null;
        }

        Count = 0;
    }

    public bool Contains(ProfilerBatchResult item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (ReferenceEquals(_items[i], item))
            {
                return true;
            }
        }

        return false;
    }

    public bool Remove(ProfilerBatchResult item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (ReferenceEquals(_items[i], item))
            {
                _items[i] = null;
                Count--;
                return true;
            }
        }

        return false;
    }

    public bool Remove(int slot)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(slot, 0, nameof(slot), "Slot index cannot be negative.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(slot, Slots, nameof(slot), $"Slot index '{slot}' exceeds the number of available slots '{Slots}'.");

        if (_items[slot] is not null)
        {
            _items[slot] = null;
            Count--;
            return true;
        }

        return false;
    }

    public bool HasItemInSlot(int slot)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(slot, 0, nameof(slot), "Slot index cannot be negative.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(slot, Slots, nameof(slot), $"Slot index '{slot}' exceeds the number of available slots '{Slots}'.");
        return _items[slot] is not null;
    }

    public bool IsSlotEmpty(int slot)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(slot, 0, nameof(slot), "Slot index cannot be negative.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfGreaterThanOrEqual(slot, Slots, nameof(slot), $"Slot index '{slot}' exceeds the number of available slots '{Slots}'.");
        return _items[slot] is null;
    }

    public int IndexOf(ProfilerBatchResult? item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (ReferenceEquals(_items[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    public void CopyTo(ProfilerBatchResult[] array, int arrayIndex)
        => _items.CopyTo(array, arrayIndex);

    public IEnumerator<ProfilerBatchResult?> GetEnumerator()
    {
        foreach (ProfilerBatchResult? item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

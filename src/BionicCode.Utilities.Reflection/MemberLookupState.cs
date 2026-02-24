namespace BionicCode.Utilities.Net.Reflection;

public enum MemberLookupState
{
    Undefined = 0,

    /// <summary>
    /// The member was not found based on the provided criteria.
    /// </summary>
    NotFound,

    /// <summary>
    /// The member was found.
    /// </summary>
    Found,

    /// <summary>
    /// Represents a state or condition where provided information was insufficient and does not yield a distinct member.
    /// </summary>
    Ambiguous,

    /// <summary>
    /// No members declared on the type. This state is used to distinguish between the case where the member was not found because it does not exist and the case where the member was not found because the type does not declare any members at all.
    /// </summary>
    SourceEmpty,
}
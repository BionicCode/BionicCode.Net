namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;
using BionicCode.Utilities.Net;

internal class MemberDataView : SymbolInfoDataView, IMemberDataView
{
    internal MemberDataView(SymbolReflectionInfoCacheKey cacheKey) : base(cacheKey)
    {
    }

    /// <summary>
    /// Returns the access modifier of the member. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetAccessModifier(out AccessModifier)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="AccessModifier"/> of the member.</value>
    public AccessModifier AccessModifier => GetMemberDataOrThrow().AccessModifier;

    /// <summary>
    /// Attempts to retrieve the <see cref="AccessModifier"/> of the member.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="accessModifier"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="AccessModifier"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="accessModifier">When this method returns successfully, contains the <see cref="AccessModifier"/> of the member.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetAccessModifier(out AccessModifier accessModifier)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            accessModifier = memberData!.AccessModifier;
            return true;
        }
        else
        {
            accessModifier = default;
            return false;
        }
    }

    /// <summary>
    /// Gets the <see cref="BindingFlags"> mask that determine the visibility of the member during reflection operations.
    /// </summary>
    /// <remarks>This property computes the visibility <see cref="BindingFlags"> mask based on the current context, which
    /// can affect how a member are accessed through reflection. It is important to note that the visibility mask may
    /// change depending on the context in which it is used.
    /// <para/>This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetBindingFlagsVisibilityMask(out BindingFlags)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="BindingFlags"> mask that determine the visibility of the member during reflection operations.</value>
    public BindingFlags BindingFlagsVisibilityMask => GetMemberDataOrThrow().BindingFlagsVisibilityMask;

    /// <summary>
    /// Attempts to retrieve the <see cref="BindingFlags"> mask that determine the visibility of the member during reflection operations.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="bindingFlagsVisibilityMask"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="BindingFlagsVisibilityMask"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="bindingFlagsVisibilityMask">When this method returns successfully, contains the <see cref="BindingFlags"> mask that determines the visibility of the member during reflection operations.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetBindingFlagsVisibilityMask(out BindingFlags bindingFlagsVisibilityMask)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            bindingFlagsVisibilityMask = memberData!.BindingFlagsVisibilityMask;
            return true;
        }
        else
        {
            bindingFlagsVisibilityMask = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the <see cref="ITypeDataView"/> metadata object of the member's declaring type. 
    /// <br/>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// <br/>To obtain the implementing type for an explicit interface implementation, use the property <see cref="ImplementingType"/> or <see cref="ImplementingTypHandle"/>.
    /// <para/>This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetDeclaringType(out ITypeDataView)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value>A <see cref="ITypeDataView"/> to provide the metadata of the member's declaring type.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public ITypeDataView DeclaringType => GetMemberDataOrThrow().DeclaringTypeData.View;

    /// <summary>
    /// Attempts to retrieve the <see cref="ITypeDataView"/> metadata object of the member's declaring type.
    /// <br/>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// <br/>To obtain the implementing type for an explicit interface implementation, use the method <see cref="TryGetImplementingTypeHandle"/> or <see cref="TryGetImplementingType(out RuntimeTypeHandle)"/>.
    /// <para/>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="declaringTypeDataView"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="DeclaringType"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="declaringTypeDataView">When this method returns successfully, contains a <see cref="ITypeDataView"/> that provides the metadata of the member's declaring type..</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetDeclaringType(out ITypeDataView? declaringTypeDataView)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            declaringTypeDataView = memberData!.DeclaringTypeData.View;
            return true;
        }
        else
        {
            declaringTypeDataView = null;
            return false;
        }
    }

    /// <summary>
    /// Returns the <see cref="RuntimeTypeHandle"/> that points to the member's declaring type.
    /// <br/>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// <br/>To obtain the implementing type for an explicit interface implementation, use the property <see cref="ImplementingTypeHandle"/> or <see cref="ImplementingType"/>.
    /// <para/>This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetDeclaringTypeHandle(out RuntimeTypeHandle)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value>A <see cref="RuntimeTypeHandle"/> that points to the member's declaring type.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public RuntimeTypeHandle DeclaringTypeHandle => GetMemberDataOrThrow().DeclaringTypeHandle;

    /// <summary>
    /// Attempts to retrieve the <see cref="RuntimeTypeHandle"/> that points to the member's declaring type.
    /// <br/>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the declaring type is the interface that declares the member, not the implementing type.
    /// <br/>To obtain the implementing type for an explicit interface implementation, use the method <see cref="TryGetImplementingTypeHandle(out RuntimeTypeHandle)"/> or <see cref="TryGetImplementingTypData(out ITypeDataView)"/>.
    /// <para/>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="declaringTypeHandle"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="DeclaringTypeHandle"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="declaringTypeHandle">When this method returns successfully, contains the <see cref="RuntimeTypeHandle"/> that points to the member's declaring type.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetDeclaringTypeHandle(out RuntimeTypeHandle declaringTypeHandle)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            declaringTypeHandle = memberData!.DeclaringTypeHandle;
            return true;
        }
        else
        {
            declaringTypeHandle = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the <see cref="ITypeDataView"/> that provides the metadata of the member's implementing type.
    /// <br/>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>To obtain the declaring interface type for an explicit interface implementation, use the property <see cref="DeclaringTypeHandle"/> or <see cref="DeclaringType"/>.
    /// <para/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// <para/>This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetImplementingType"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value>A <see cref="ITypeDataView"/> that provides the metadata of the member's implementing type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public ITypeDataView ImplementingType => GetMemberDataOrThrow().ImplementingTypeData.View;

    /// <summary>
    /// Attempts to retrieve the <see cref="ITypeDataView"/> that provides the metadata of the member's implementing type.
    /// <br/>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>To obtain the declaring interface type for an explicit interface implementation, use the method <see cref="TryGetDeclaringTypeHandle(out RuntimeTypeHandle)"/> or <see cref="TryGetDeclaringType(out ITypeDataView)"/>.
    /// <para/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// <para/>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="implementingType"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="ImplementingType"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="implementingType">When this method returns successfully, contains the <see cref="RuntimeTypeHandle"/> that points to the member's implementing type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetImplementingType(out ITypeDataView? implementingType)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            implementingType = memberData!.ImplementingTypeData.View;
            return true;
        }
        else
        {
            implementingType = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the <see cref="RuntimeTypeHandle"/> that points to the member's implementing type.
    /// <br/>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>To obtain the declaring interface type for an explicit interface implementation, use the property <see cref="DeclaringTypeHandle"/> or <see cref="DeclaringType"/>.
    /// <para/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// <para/>This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetImplementingTypeHandle(out RuntimeTypeHandle)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value>A <see cref="RuntimeTypeHandle"/> that points to the member's implementing type.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public RuntimeTypeHandle ImplementingTypeHandle => GetMemberDataOrThrow().ImplementingTypeHandle;

    /// <summary>
    /// Attempts to retrieve the <see cref="RuntimeTypeHandle"/> that points to the member's declaring type.
    /// <br/>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// </summary>
    /// <remarks>For members that are explicit interface implementations, the implementing type is the type that implements the member, not the declaring interface type.
    /// <br/>To obtain the declaring interface type for an explicit interface implementation, use the method <see cref="TryGetDeclaringTypeHandle(out RuntimeTypeHandle)"/> or <see cref="TryGetDeclaringType(out ITypeDataView)"/>.
    /// <para/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.
    /// <para/>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="implementingTypeHandle"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="ImplementingTypeHandle"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="implementingTypeHandle">When this method returns successfully, contains the <see cref="RuntimeTypeHandle"/> that points to the member's implementing type.
    /// <br/>For non-explicit interface implementation members, the implementing type is the same as the declaring type.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetImplementingTypeHandle(out RuntimeTypeHandle implementingTypeHandle)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            implementingTypeHandle = memberData!.ImplementingTypeHandle;
            return true;
        }
        else
        {
            implementingTypeHandle = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="internal". 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsAssembly(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="internal"; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsAssembly => GetMemberDataOrThrow().IsAssembly;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="internal".
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isAssembly"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsAssembly"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isAssembly">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="internal".</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsAssembly(out bool isAssembly)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isAssembly = memberData!.IsAssembly;
            return true;
        }
        else
        {
            isAssembly = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="protected"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsFamily(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="protected"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsFamily => GetMemberDataOrThrow().IsFamily;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="protected"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isFamily"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsFamily"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isFamily">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="protected"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsFamily(out bool isFamily)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isFamily = memberData!.IsAssembly;
            return true;
        }
        else
        {
            isFamily = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="private"/> <see langword="protected"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsFamilyAndAssembly(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="private"/> <see langword="protected"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsFamilyAndAssembly => GetMemberDataOrThrow().IsFamilyAndAssembly;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="private"/> <see langword="protected"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isFamilyAndAssembly"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsFamilyAndAssembly"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isFamilyAndAssembly">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="private"/> <see langword="protected"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsFamilyAndAssembly(out bool isFamilyAndAssembly)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isFamilyAndAssembly = memberData!.IsFamilyAndAssembly;
            return true;
        }
        else
        {
            isFamilyAndAssembly = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="protected"/> <see langword="public"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsFamilyOrAssembly(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="protected"/> <see langword="public"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsFamilyOrAssembly => GetMemberDataOrThrow().IsFamilyOrAssembly;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="protected"/> <see langword="public"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isFamilyOrAssembly"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsFamilyOrAssembly"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isFamilyOrAssembly">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="protected"/> <see langword="public"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsFamilyOrOrAssembly(out bool isFamilyOrAssembly)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isFamilyOrAssembly = memberData!.IsFamilyOrAssembly;
            return true;
        }
        else
        {
            isFamilyOrAssembly = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="private"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsPrivate(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="private"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsPrivate => GetMemberDataOrThrow().IsPrivate;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="private"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isPrivate"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsPrivate"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isPrivate">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="private"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsPrivate(out bool isPrivate)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isPrivate = memberData!.IsPrivate;
            return true;
        }
        else
        {
            isPrivate = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member's visibility is <see langword="public"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsPublic(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member's visibility is <see langword="public"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsPublic => GetMemberDataOrThrow().IsPublic;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the member's visibility is <see langword="public"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isPublic"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsPublic"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isPublic">When this method returns successfully, contains a value indicating whether the member's visibility is <see langword="public"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsPublic(out bool isPublic)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isPublic = memberData!.IsPublic;
            return true;
        }
        else
        {
            isPublic = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member is <see langword="static"/>. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsStatic(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member is <see langword="static"/>; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsStatic => GetMemberDataOrThrow().IsStatic;
    /// <summary>
    /// Attempts to retrieve a value indicating whether the member is <see langword="static"/>.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isStatic"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsStatic"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isStatic">When this method returns successfully, contains a value indicating whether the member is <see langword="static"/>.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsStatic(out bool isStatic)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isStatic = memberData!.IsStatic;
            return true;
        }
        else
        {
            isStatic = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the member is an explicit interface implementation. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsExplicitInterfaceImplementation(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the member is an explicit interface implementation; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsExplicitInterfaceImplementation => GetMemberDataOrThrow().IsExplicitInterfaceImplementation;
    /// <summary>
    /// Attempts to retrieve a value indicating whether the member is an explicit interface implementation.
    /// </summary>
    /// <remarks>If the member data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isExplicitInterfaceImplementation"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsExplicitInterfaceImplementation"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isExplicitInterfaceImplementation">When this method returns successfully, contains a value indicating whether the member is an explicit interface implementation.</param>
    /// <returns>Returns <see langword="true"/> if the member data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsExplicitInterfaceImplementation(out bool isExplicitInterfaceImplementation)
    {
        if (TryGetMemberData(out MemberData? memberData))
        {
            isExplicitInterfaceImplementation = memberData!.IsExplicitInterfaceImplementation;
            return true;
        }
        else
        {
            isExplicitInterfaceImplementation = default;
            return false;
        }
    }
}
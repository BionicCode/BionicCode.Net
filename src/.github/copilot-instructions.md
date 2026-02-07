# Copilot Instructions

## General Guidelines
- Prefer modeling parameters like .NET: `ParameterData` represents method/constructor parameters only (no generic type parameters).
- Remove `TypeData` support from `ParameterData`/`ParameterList`.
- Make `ParameterData.Member` return `MemberData`.
- Plan to introduce a `ParameterizedMember` abstraction implemented by `MethodData`/`ConstructorData`.
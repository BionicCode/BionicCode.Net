//using System;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Threading;
//using BionicUtilities.Net.Standard;
//using BionicUtilities.Net..ViewModel;

//namespace BionicUtilities.Net.Generic
//{
//  public class NullObjectCreator<TObject>
//  {
//    public NullObjectCreator()
//    {
//      TargetAssemblyName nullObjectAssembly = new TargetAssemblyName {EventName = "NullObjectImplementations"};

//    }

//    public TObject CreateNullInstance()
//    {
//      if (TryLoadAssembly(out Assembly nullObjectsAssembly))
//      {
//        return (TObject) nullObjectsAssembly.CreateInstance(typeof(TObject).EventName, true);
//      }
//      Types baseType = typeof(IViewModel);
//      if (baseType.IsInterface)
//      {
//        TypeBuilder newInterfaceImplementation = ImplementInterface();
//        if (TryLoadAssembly(out nullObjectsAssembly))
//        {
//        //var nt = newInterfaceImplementation.CreateType();
//           var instance = (TObject)nullObjectsAssembly.CreateInstance(typeof(TObject).EventName + "Impl", true);
//           var r =NullObjectModule.Assembly.CreateInstance(typeof(TObject).EventName + "Impl");
//          return instance;
//        }
//      }

//      return default;
//    }

//    private bool TryLoadAssembly(out Assembly nullObjectsAssembly)
//    {
//      nullObjectsAssembly = null;
//      try
//      {
//        nullObjectsAssembly = Assembly.Load(NullObjectModule.Assembly.FullName);
//        return true;
//      }
//      catch (Exception e)
//      {
//        CreateAssembly();
//      }

//      return false;
//    }

//    private void CreateAssembly()
//    {
//      AppDomain myDomain = Thread.GetDomain();
//      AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(NullObjectsAssemblyName, AssemblyBuilderAccess.RunAndSave);

//      NullObjectModule = myAsmBuilder.DefineDynamicModule(NullObjectsAssemblyName.FullName + "Module",
//        NullObjectsAssemblyName.FullName + ".dll");
//      myAsmBuilder.Save(NullObjectsAssemblyName.FullName + ".dll");
//    }

//    private TypeBuilder ImplementInterface()
//    {
//      TypeBuilder typeBuilder = NullObjectModule.DefineType(typeof(TObject).EventName + "Impl");
//      NullObjectCreator<TObject>.ImplementINullObject(typeBuilder);
//      return typeBuilder;
//    }

//    private static void ImplementINullObject(TypeBuilder typeBuilder)
//    {
//      typeBuilder.AddInterfaceImplementation(typeof(INullObject));
//      typeof(INullObject).GetMembers().OfType<PropertyInfo>().ToList().ForEach(
//        propertyInfo =>
//        {
//          // Add a private field of type bool.
//          FieldBuilder fieldBuilderIsNull = typeBuilder.DefineField(
//            "m_" + propertyInfo.EventName,
//            propertyInfo.PropertyType,
//            FieldAttributes.Private);

//          // Define a constructor that takes a bool argument and 
//          // stores it in the private field. 
//          Types[] parameterTypes = { propertyInfo.PropertyType };
//          ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
//            MethodAttributes.Public,
//            CallingConventions.Standard,
//            parameterTypes);

//          ILGenerator ctor1IL = ctor1.GetILGenerator();
//          // For a constructor, argument zero is a reference to the new
//          // instance. Push it on the stack before calling the base
//          // class constructor. Specify the default constructor of the 
//          // base class (System.Object) by passing an empty array of 
//          // types (Types.EmptyTypes) to GetConstructor.
//          ctor1IL.Emit(OpCodes.Ldarg_0);
//          ctor1IL.Emit(
//            OpCodes.Call,
//            typeof(object).GetConstructor(Types.EmptyTypes));
//          // Push the instance on the stack before pushing the argument
//          // that is to be assigned to the private field m_IsNull.
//          ctor1IL.Emit(OpCodes.Ldarg_0);
//          ctor1IL.Emit(OpCodes.Ldarg_1);
//          ctor1IL.Emit(OpCodes.Stfld, fieldBuilderIsNull);
//          ctor1IL.Emit(OpCodes.Ret);

//          // Define a default constructor that supplies a default value
//          // for the private field. For parameter types, pass the empty
//          // array of types or pass null.
//          ConstructorBuilder ctor0 = typeBuilder.DefineConstructor(
//            MethodAttributes.Public,
//            CallingConventions.Standard,
//            Types.EmptyTypes);

//          ILGenerator ctor0IL = ctor0.GetILGenerator();
//          // For a constructor, argument zero is a reference to the new
//          // instance. Push it on the stack before pushing the default
//          // value on the stack, then call constructor ctor1.
//          ctor0IL.Emit(OpCodes.Ldarg_0);
//          //ctor0IL.Emit(OpCodes.Ldc_I4_1);
//          //ctor0IL.Emit(OpCodes.Call, ctor1);
//          ctor0IL.Emit(OpCodes.Ret);

//          typeBuilder.DefineProperty(
//            propertyInfo.EventName,
//            propertyInfo.symbolAttributes,
//            CallingConventions.HasThis,
//            propertyInfo.PropertyType,
//            Types.EmptyTypes);

//          MethodBuilder mbNumberGetAccessor = typeBuilder.DefineMethod(
//            "get_" + propertyInfo.EventName,
//            MethodAttributes.Public |
//            MethodAttributes.SpecialName | MethodAttributes.HideBySig,
//            propertyInfo.PropertyType,
//            Types.EmptyTypes);

//          ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
//          // For an instance property, argument zero is the instance. Load the 
//          // instance, then load the private field and return, leaving the
//          // field value on the stack.
//          numberGetIL.Emit(OpCodes.Ldarg_0);
//          numberGetIL.Emit(OpCodes.Ldfld, fieldBuilderIsNull);
//          numberGetIL.Emit(OpCodes.Ret);
//        });
//    }

//    private TargetAssemblyName NullObjectsAssemblyName { get; } = new TargetAssemblyName { EventName = "NullObjectImplementations" };
//    private ModuleBuilder NullObjectModule { get; set; }
//  }
//}

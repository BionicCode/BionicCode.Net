//using System;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Threading;
//using BionicUtilities.Net.Standard;
//using BionicUtilities.Net.Standard.ViewModel;

//namespace BionicUtilities.Net.Generic
//{
//  public class NullObjectCreator<TObject>
//  {
//    public NullObjectCreator()
//    {
//      AssemblyName nullObjectAssembly = new AssemblyName {Name = "NullObjectImplementations"};
      
//    }

//    public TObject CreateNullInstance()
//    {
//      if (TryLoadAssembly(out Assembly nullObjectsAssembly))
//      {
//        return (TObject) nullObjectsAssembly.CreateInstance(typeof(TObject).Name, true);
//      }
//      Type baseType = typeof(IViewModel);
//      if (baseType.IsInterface)
//      {
//        TypeBuilder newInterfaceImplementation = ImplementInterface();
//        if (TryLoadAssembly(out nullObjectsAssembly))
//        {
//        //var nt = newInterfaceImplementation.CreateType();
//           var instance = (TObject)nullObjectsAssembly.CreateInstance(typeof(TObject).Name + "Impl", true);
//           var r =this.NullObjectModule.Assembly.CreateInstance(typeof(TObject).Name + "Impl");
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
//        nullObjectsAssembly = Assembly.Load(this.NullObjectModule.Assembly.FullName);
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
//      AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(this.NullObjectsAssemblyName, AssemblyBuilderAccess.RunAndSave);

//      this.NullObjectModule = myAsmBuilder.DefineDynamicModule(this.NullObjectsAssemblyName.FullName + "Module",
//        this.NullObjectsAssemblyName.FullName + ".dll");
//      myAsmBuilder.Save(this.NullObjectsAssemblyName.FullName + ".dll");
//    }

//    private TypeBuilder ImplementInterface()
//    {
//      TypeBuilder typeBuilder = this.NullObjectModule.DefineType(typeof(TObject).Name + "Impl");
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
//            "m_" + propertyInfo.Name,
//            propertyInfo.PropertyType,
//            FieldAttributes.Private);

//          // Define a constructor that takes a bool argument and 
//          // stores it in the private field. 
//          Type[] parameterTypes = { propertyInfo.PropertyType };
//          ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
//            MethodAttributes.Public,
//            CallingConventions.Standard,
//            parameterTypes);

//          ILGenerator ctor1IL = ctor1.GetILGenerator();
//          // For a constructor, argument zero is a reference to the new
//          // instance. Push it on the stack before calling the base
//          // class constructor. Specify the default constructor of the 
//          // base class (System.Object) by passing an empty array of 
//          // types (Type.EmptyTypes) to GetConstructor.
//          ctor1IL.Emit(OpCodes.Ldarg_0);
//          ctor1IL.Emit(
//            OpCodes.Call,
//            typeof(object).GetConstructor(Type.EmptyTypes));
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
//            Type.EmptyTypes);

//          ILGenerator ctor0IL = ctor0.GetILGenerator();
//          // For a constructor, argument zero is a reference to the new
//          // instance. Push it on the stack before pushing the default
//          // value on the stack, then call constructor ctor1.
//          ctor0IL.Emit(OpCodes.Ldarg_0);
//          //ctor0IL.Emit(OpCodes.Ldc_I4_1);
//          //ctor0IL.Emit(OpCodes.Call, ctor1);
//          ctor0IL.Emit(OpCodes.Ret);

//          typeBuilder.DefineProperty(
//            propertyInfo.Name,
//            propertyInfo.Attributes,
//            CallingConventions.HasThis,
//            propertyInfo.PropertyType,
//            Type.EmptyTypes);

//          MethodBuilder mbNumberGetAccessor = typeBuilder.DefineMethod(
//            "get_" + propertyInfo.Name,
//            MethodAttributes.Public |
//            MethodAttributes.SpecialName | MethodAttributes.HideBySig,
//            propertyInfo.PropertyType,
//            Type.EmptyTypes);

//          ILGenerator numberGetIL = mbNumberGetAccessor.GetILGenerator();
//          // For an instance property, argument zero is the instance. Load the 
//          // instance, then load the private field and return, leaving the
//          // field value on the stack.
//          numberGetIL.Emit(OpCodes.Ldarg_0);
//          numberGetIL.Emit(OpCodes.Ldfld, fieldBuilderIsNull);
//          numberGetIL.Emit(OpCodes.Ret);
//        });
//    }

//    private AssemblyName NullObjectsAssemblyName { get; } = new AssemblyName { Name = "NullObjectImplementations" };
//    private ModuleBuilder NullObjectModule { get; set; }
//  }
//}

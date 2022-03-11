using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using AssetBundles;
#pragma warning disable CS0618
public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    private AppDomain m_AppDomain;
    private const string DLLPATH = "ILRuntimedll/HotFix_Project.dll.bytes";
    private const string PDBPATH = "ILRuntimedll/HotFix_Project.pdb.bytes";

    //资源加载完成回调
    public delegate void OnAsyncObjFinish(string path, UnityEngine.Object obj, object param1 = null, object param2 = null, object param3 = null);

    //实例化对象加载完成回调
    public delegate void OnAsyncFinsih(string path, UnityEngine.Object resObj, object param1 = null, object param2 = null, object param3 = null);
    public AppDomain ILRunAppDomain
    {
        get { return m_AppDomain; }
    }
    public string AssetbundleName
    {
        get;
        protected set;
    }
    public override void Init()
    {
        base.Init();
        string path = AssetBundleUtility.PackagePathToAssetsPath(DLLPATH);
        AssetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(path);
        //InitLuaEnv();
    }

    public void LoadHotFixDll()
    {
        //全局唯一AppDomain
        m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //读取热更资源的dll
        var abloader = AssetBundleManager.Instance.LoadAssetAsync(DLLPATH,typeof(TextAsset));
        TextAsset dllText =abloader.asset as TextAsset;
        MemoryStream ms = new MemoryStream(dllText.bytes);
        m_AppDomain.LoadAssembly(ms, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
//         //读取热更dll
// #if UNITY_ANDROID
//         WWW www = new WWW(Application.streamingAssetsPath + "/" + DLLPATH);
// #else
//         WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/"+DLLPATH);
// #endif
//         while (!www.isDone)
//             yield return null;
//         if (!string.IsNullOrEmpty(www.error))
//             UnityEngine.Debug.LogError(www.error);
//         byte[] dll = www.bytes;
//         www.Dispose();

//         //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
// #if UNITY_ANDROID
//         www = new WWW(Application.streamingAssetsPath + "/" + PDBPATH);
// #else
//         www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.pdb");
// #endif
//         while (!www.isDone)
//             yield return null;
//         if (!string.IsNullOrEmpty(www.error))
//             UnityEngine.Debug.LogError(www.error);
//         byte[] pdb = www.bytes;
//         MemoryStream fs = new MemoryStream(dll);
//         MemoryStream p = new MemoryStream(pdb);
//         try
//         {
//             m_AppDomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
//         }
//         catch
//         {
//             Debug.LogError("加载热更DLL失败，HotFix_Project.sln编译过热更DLL");
//         }

        InitializeIlRuntime();
        OnHotFixLoaded();
    }

    //在这里做一些注册
    void InitializeIlRuntime()
    {
        // #if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //         //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        //         m_AppDomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        // #endif
        //默认委托注册仅仅支持系统自带的Action以及Function
        m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
        m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
        m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
        m_AppDomain.DelegateManager
            .RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>();

        m_AppDomain.DelegateManager
            .RegisterMethodDelegate<System.String, UnityEngine.Object, System.Object, System.Object>();

        //自定义委托或Unity委托注册
        m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMeth>((action) =>
        {
            return new TestDelegateMeth((a) => { ((System.Action<int>)action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
        {
            return new TestDelegateFunction((a) => { return ((System.Func<string, string>)action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<bool>((a) => { ((System.Action<bool>)action)(a); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
        {
            return new UnityEngine.Events.UnityAction(() => { ((System.Action)action)(); });
        });

        m_AppDomain.DelegateManager.RegisterDelegateConvertor<OnAsyncObjFinish>((action) =>
        {
            return new OnAsyncObjFinish((path, obj, param1, param2, param3) =>
            {
                ((System.Action<System.String, UnityEngine.Object, System.Object, System.Object, System.Object>)
                    action)(path, obj, param1, param2, param3);
            });
        });


        //跨域继承的注册
        m_AppDomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
        //注册协程适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
        //注册Mono适配器
        m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        //注册Window适配器
        //m_AppDomain.RegisterCrossBindingAdaptor(new WindowAdapter());

        SetupCLRAddCompontent();
        SetUpCLRGetCompontent();

        //绑定注册 (最后执行)
        ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain);
    }

    void OnHotFixLoaded()
    {
        UIManager.Instance.OnInit();
        //切换场景

        //m_AppDomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);
    }
    unsafe void SetUpCLRGetCompontent()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "GetCompontent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, GetCompontent);
            }
        }
    }

    private unsafe StackObject* GetCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
        CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();

        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                foreach (var clrInstance in clrInstances)
                {
                    if (clrInstance.ILInstance != null)
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe void SetupCLRAddCompontent()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                m_AppDomain.RegisterCLRMethodRedirection(i, AddCompontent);
            }
        }
    }

    private unsafe StackObject* AddCompontent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack,
        CLRMethod __method, bool isNewObj)
    {
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
        {
            throw new System.NullReferenceException();
        }

        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType) //CLRType表示这个类型是Unity工程里的类型   //ILType表示是热更dll里面的类型
            {
                //Unity主工程的类，不需要做处理
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //创建出来MonoTest
                var ilInstance = new ILTypeInstance(type as ILType, false);
                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddCompontent出来的有效实例，所以要替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;

                //补掉Awake
                clrInstance.Awake();
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }
}

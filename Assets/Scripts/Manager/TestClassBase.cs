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


    public abstract class TestClassBase
    {
        public virtual int Value { get; set; }

        public virtual void TestVirtual(string str)
        {
            Debug.Log("TestClassBase TestVirtual   str=" + str);
        }

        public abstract void TestAbstract(int a);
    }

    public class InheritanceAdapter : CrossBindingAdaptor
    {
        public override System.Type BaseCLRType
        {
            get
            {
                //想继承的类
                return typeof(TestClassBase);
            }
        }

        public override System.Type AdaptorType
        {
            get
            {
                //实际的适配器类
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        class Adapter : TestClassBase, CrossBindingAdaptorType
        {
            private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
            private ILTypeInstance m_Instance;
            private IMethod m_TestAbstract;
            private IMethod m_TestVirtual;
            private IMethod m_GetValue;
            private IMethod m_ToString;
            object[] param1 = new object[1];
            private bool m_TestVirtualInvoking = false;
            private bool m_GetValueInvoking = false;

            public Adapter()
            {
            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                m_Appdomain = appdomain;
                m_Instance = instance;
            }

            public ILTypeInstance ILInstance
            {
                get { return m_Instance; }
            }

            //在适配器中重写所有需要在热更脚本重写的方法，并且将控制权转移到脚本里去
            public override void TestAbstract(int a)
            {
                if (m_TestAbstract == null)
                {
                    m_TestAbstract = m_Instance.Type.GetMethod("TestAbstract", 1);
                }

                if (m_TestAbstract != null)
                {
                    param1[0] = a;
                    m_Appdomain.Invoke(m_TestAbstract, m_Instance, param1);
                }
            }

            public override void TestVirtual(string str)
            {
                if (m_TestVirtual == null)
                {
                    m_TestVirtual = m_Instance.Type.GetMethod("TestVirtual", 1);
                }

                //必须要设定一个标识位来表示当前是否在调用中, 否则如果脚本类里调用了base.TestVirtual()就会造成无限循环
                if (m_TestVirtual != null && !m_TestVirtualInvoking)
                {
                    m_TestVirtualInvoking = true;
                    param1[0] = str;
                    m_Appdomain.Invoke(m_TestVirtual, m_Instance, param1);
                    m_TestVirtualInvoking = false;
                }
                else
                {
                    base.TestVirtual(str);
                }
            }

            public override int Value
            {
                get
                {
                    if (m_GetValue == null)
                    {
                        m_GetValue = m_Instance.Type.GetMethod("get_Value", 1);
                    }

                    if (m_GetValue != null && !m_GetValueInvoking)
                    {
                        m_GetValueInvoking = true;
                        int res = (int)m_Appdomain.Invoke(m_GetValue, m_Instance, null);
                        m_GetValueInvoking = false;
                        return res;
                    }
                    else
                    {
                        return base.Value;
                    }
                }
            }

            public override string ToString()
            {
                if (m_ToString == null)
                {
                    m_ToString = m_Appdomain.ObjectType.GetMethod("ToString", 0);
                }

                IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
                if (m == null || m is ILMethod)
                {
                    return m_Instance.ToString();
                }
                else
                {
                    return m_Instance.Type.FullName;
                }
            }
        }
    }

    public delegate void TestDelegateMeth(int a);

    public delegate string TestDelegateFunction(string a);

    public class CLRBindingTestClass
    {
        public static float DoSomeTest(int a, float b)
        {
            return a + b;
        }
    }

    /// <summary>
    /// 携程适配器
    /// </summary>
    public class CoroutineAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType => null;

        public override Type AdaptorType
        {
            get { return typeof(Adaptor); }
        }

        public override Type[] BaseCLRTypes
        {
            get { return new Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(IDisposable) }; }
        }

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        internal class Adaptor : IEnumerator<System.Object>, IEnumerator, IDisposable, CrossBindingAdaptorType
        {
            private ILTypeInstance instance;
            private AppDomain appdomain;

            public Adaptor()
            {
            }

            public Adaptor(AppDomain appDomain, ILTypeInstance instance)
            {
                this.instance = instance;
                this.appdomain = appDomain;
            }

            private IMethod mMoveNextMethod;
            private bool mMoveNextMethodGot;

            public bool MoveNext()
            {
                if (!mMoveNextMethodGot)
                {
                    mMoveNextMethod = instance.Type.GetMethod("MoveNext", 0);
                    mMoveNextMethodGot = true;
                }

                if (mMoveNextMethod != null)
                {
                    return (bool)appdomain.Invoke(mMoveNextMethod, instance, null);
                }
                else
                {
                    return false;
                }
            }

            private IMethod mResetMethod;
            private bool mResetMethodGot;

            public void Reset()
            {
                if (!mResetMethodGot)
                {
                    mResetMethod = instance.Type.GetMethod("Reset", 0);
                    mResetMethodGot = true;
                }

                if (mResetMethod != null)
                {
                    appdomain.Invoke(mResetMethod, instance, null);
                }
            }

            private IMethod mCurrentMethod;
            private bool mCurrentMethodGot;
            object IEnumerator.Current => Current;

            public object Current
            {
                get
                {
                    if (!mCurrentMethodGot)
                    {
                        mCurrentMethod = instance.Type.GetMethod("get_Current", 0);
                        if (mCurrentMethod == null)
                        {
                            mCurrentMethod = instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
                        }

                        mCurrentMethodGot = true;
                    }

                    if (mCurrentMethod != null)
                    {
                        var res = appdomain.Invoke(mCurrentMethod, instance, null);
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            private IMethod mDisposeMethod;
            private bool mDisposeMethodGot;

            public void Dispose()
            {
                if (!mDisposeMethodGot)
                {
                    mDisposeMethod = instance.Type.GetMethod("Dispose", 0);
                    if (mDisposeMethod == null)
                    {
                        mDisposeMethod = instance.Type.GetMethod("System.IDisposable.Dispose", 0);
                    }

                    mDisposeMethodGot = true;
                }

                if (mDisposeMethod != null)
                {
                    appdomain.Invoke(mDisposeMethod, instance, null);
                }
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                {
                    return instance.Type.FullName;
                }
            }

            public ILTypeInstance ILInstance => instance;
        }
    }

    /// <summary>
    /// MonoBehaviour适配器
    /// </summary>
    public class MonoBehaviourAdapter : CrossBindingAdaptor
    {
        public override System.Type BaseCLRType
        {
            get { return typeof(MonoBehaviour); }
        }

        public override System.Type AdaptorType
        {
            get { return typeof(Adaptor); }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
        {
            private ILRuntime.Runtime.Enviorment.AppDomain m_Appdomain;
            private ILTypeInstance m_Instance;
            private IMethod m_AwakeMethod;
            private IMethod m_StartMethod;
            private IMethod m_UpdateMethod;
            private IMethod m_ToString;

            public Adaptor()
            {
            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                m_Appdomain = appdomain;
                m_Instance = instance;
            }

            public ILTypeInstance ILInstance
            {
                get { return m_Instance; }
                set
                {
                    m_Instance = value;
                    m_AwakeMethod = null;
                    m_StartMethod = null;
                    m_UpdateMethod = null;
                }
            }

            public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
            {
                get { return m_Appdomain; }
                set { m_Appdomain = value; }
            }

            public void Awake()
            {
                if (m_Instance != null)
                {
                    if (m_AwakeMethod == null)
                    {
                        m_AwakeMethod = m_Instance.Type.GetMethod("Awake", 0);
                    }

                    if (m_AwakeMethod != null)
                    {
                        m_Appdomain.Invoke(m_AwakeMethod, m_Instance, null);
                    }
                }
            }

            void Start()
            {
                if (m_StartMethod == null)
                {
                    m_StartMethod = m_Instance.Type.GetMethod("Start", 0);
                }

                if (m_StartMethod != null)
                {
                    m_Appdomain.Invoke(m_StartMethod, m_Instance, null);
                }
            }


            void Update()
            {
                if (m_UpdateMethod == null)
                {
                    m_UpdateMethod = m_Instance.Type.GetMethod("Update", 0);
                }

                if (m_UpdateMethod != null)
                {
                    m_Appdomain.Invoke(m_UpdateMethod, m_Instance, null);
                }
            }

            public override string ToString()
            {
                if (m_ToString == null)
                {
                    m_ToString = m_Appdomain.ObjectType.GetMethod("ToString", 0);
                }

                IMethod m = m_Instance.Type.GetVirtualMethod(m_ToString);
                if (m == null || m is ILMethod)
                {
                    return m_Instance.ToString();
                }
                else
                {
                    return m_Instance.Type.FullName;
                }
            }
        }
    }


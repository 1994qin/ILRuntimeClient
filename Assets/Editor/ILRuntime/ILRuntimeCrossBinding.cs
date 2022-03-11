#if UNITY_EDITOR
using System;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCrossBinding
{
    [MenuItem("ILRuntime/���ɿ���̳�������")]
    static void GenerateCrossbindAdapter()
    {
        //���ڿ���̳�������̫�࣬�Զ������޷�ʵ����ȫ�޸��������ɣ����������ṩ�Ĵ����Զ�������Ҫ�Ǹ�������ɸ���ʼģ�棬�򻯴�ҵĹ���
        //��������ֱ��ʹ���Զ����ɵ�ģ�漴�ɣ����������������ֶ�ȥ�޸����ɺ���ļ������������Ҫ������д����Ƿ񸲸ǵ�����

        using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Scripts/Manager/TestClassBasesAdapter.cs"))
        {
            sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(TestClassBase), "ILRuntimeDemo"));
        }

        AssetDatabase.Refresh();
    }
}
#endif

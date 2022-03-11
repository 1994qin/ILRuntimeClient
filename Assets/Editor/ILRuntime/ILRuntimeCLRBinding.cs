using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    [MenuItem("ILRuntime/CLR绑定")]
    static void GenerateCLRBindingByAnakysis()
    {
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream("Assets/StreamingAssets/HotFix_Project.dll", System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/ILRuntime/Generated");
        }
        AssetDatabase.Refresh();
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //������Ҫע�������ȸ�DLL���õ��Ŀ���̳�Adapter�������޷���ȷץȡ����

    }

    [MenuItem("ILRuntime/修改热更dll后缀为.bytes")]
    static void ChangeDllName()
    {
        string DLLPATH = "Assets/AssetsPackage/ILRuntimedll/HotFix_Project.dll";
        string PDBPATH = "Assets/AssetsPackage/ILRuntimedll/HotFix_Project.pdb";
        if (File.Exists(DLLPATH))
        {
            string targetPath = DLLPATH + ".bytes";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            File.Move(DLLPATH, targetPath);
        }

        if (File.Exists(PDBPATH))
        {
            string targetPath = PDBPATH + ".bytes";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            File.Move(PDBPATH, targetPath);
        }

        AssetDatabase.Refresh();
    }
}

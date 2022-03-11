using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AssetBundles;
//资源管理模块
public class ResourceManager : Singleton<ResourceManager>
{
    //异步加载AB 回调
    public void LoadAssetBundleAsync(string assetPath,Action fCallBack)
    {

    }
    //异步加载AB 协程
    public IEnumerable CoLoadAssetBundleAsync(string assetPath)
    {
        if(string.IsNullOrEmpty(assetPath))
        {
            Logger.LogError("异步加载AB 协程 CoLoadAssetBundleAsync  assetPath 为空");
            yield break;
        }
        string assetbundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(assetPath);
        var abloader = AssetBundleManager.Instance.LoadAssetBundleAsync(assetbundleName,null,false);
        yield return abloader;

    }

    //异步加载资源 回调
    public UnityEngine.Object CoLoadAsync(string assetPath,Type assetType,Action fCallBack)
    {
        var loader = AssetBundleManager.Instance.LoadAssetAsync(assetPath,assetType);
        UnityEngine.Object asset = loader.asset;
        loader.Dispose();
        return asset;
    }

    //异步加载资源
    public UnityEngine.Object CoLoadAsync(string assetPath,Type assetType)
    {
        var loader = AssetBundleManager.Instance.LoadAssetAsync(assetPath,assetType);
        UnityEngine.Object asset = loader.asset;
        loader.Dispose();
        return asset;
    }
}

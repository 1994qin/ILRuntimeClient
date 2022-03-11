using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//UI基类
public class BaseUI
{
    //是否是热更类
    public bool IsHotFix { get; set; } = false;
    //热更类名 
    public string HotFixClassName { get; set; }
    public virtual void Awake(object param1 = null, object param2 = null, object param3 = null)
    {
    }

    public virtual void OnShow(object param1 = null, object param2 = null, object param3 = null)
    {
    }

    public virtual void OnDisable()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnClose()
    {

    }
}

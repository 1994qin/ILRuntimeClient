using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILRuntime.CLR.TypeSystem;
//UI管理类
public class UIManager : MonoSingleton<UIManager>
{
    //父级
    private Transform _parent;
    //UI摄像机
    public Camera UICamera;
    //分辨率
    public Vector2 Resolution = new Vector2(1920, 1080);
    //层级管理
    private Dictionary<string, GameObject> _layerList = new Dictionary<string, GameObject>();
    //全部UI
    private List<BaseUI> _baseUIList = new List<BaseUI>();
    public void OnInit()
    {
        _parent = GameObject.Find("UIRoot").transform;
        UICamera = _parent.Find("UICamera").GetComponent<Camera>();
        _layerList.Clear();
        // 初始化TOP层级
        // 场景UI,如血条，点击建筑物查看信息 一般置于场景之上，UI界面之下
        CreateLayer("ScreenLayer", 0, 1000);
        // 背景UI,如主界面，一般用户不能主动关闭，永远处于其他UI的最底层
        CreateLayer("BackgroundLayer", 1000, 900);
        // 普通UI,一级，二级，三级窗口，一般由玩家点击打开的多级窗口
        CreateLayer("ViewLayer", 2000, 800);
        // 信息UI，如广播，跑马灯，一般永远置于用户打开窗户顶层
        CreateLayer("InfoLayer", 3000, 700);
        // 提示UI，如错误弹窗，网络连接弹窗等
        CreateLayer("TipLayer", 4000, 600);
        // 顶层UI，场景加载 Loading图
        CreateLayer("TopLayer", 5000, 500);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _baseUIList.Count; i++)
        {
            BaseUI ui = _baseUIList[i];
            if (ui != null)
            {
                if (ui.IsHotFix)
                {
                    ILRuntimeManager.Instance.ILRunAppDomain.Invoke(ui.HotFixClassName, "OnUpdate", ui);
                }
                else
                {
                    _baseUIList[i].OnUpdate();
                }
            }
        }
    }









    private void CreateLayer(string layerName, int orderInLayer, int planeDistance)
    {
        if (_layerList.ContainsKey(layerName))
        {
            return;
        }
        GameObject go;
        if (layerName != "TopLayer")
        {
            go = new GameObject(layerName);
            go.gameObject.layer = 5;
            //canvas
            Canvas _canvas = go.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = go.AddComponent<Canvas>();
            }
            _canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = UICamera;
            _canvas.planeDistance = planeDistance;
            _canvas.sortingLayerName = "UI";
            _canvas.sortingOrder = orderInLayer;
            //canvas scaler
            CanvasScaler _canvascaler = go.GetComponent<CanvasScaler>();
            if (_canvascaler == null)
            {
                _canvascaler = go.AddComponent<CanvasScaler>();
            }
            _canvascaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvascaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvascaler.referenceResolution = Resolution;

            //raycaster
            GraphicRaycaster _canvasraycaster = go.GetComponent<GraphicRaycaster>();
            if (_canvasraycaster == null)
            {
                _canvasraycaster = go.AddComponent<GraphicRaycaster>();
            }
            go.transform.SetParent(_parent.transform);
        }
        else
        {
            go = GameObject.Find("TopLayer");
            go.transform.GetComponent<Canvas>().planeDistance = planeDistance;
            go.transform.GetComponent<Canvas>().sortingOrder = orderInLayer;
            go.transform.transform.SetAsLastSibling();
        }
        _layerList[layerName] = go;
    }
}

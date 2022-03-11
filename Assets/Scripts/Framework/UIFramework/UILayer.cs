using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UILayer
{
    private GameObject go;
    public void New(GameObject obj)
    {
        this.go = obj;
    }
    private void CreateLayer(string layerName, int orderInLayer, int planeDistance)
    {

        go.gameObject.layer = 5;
        //canvas
        Canvas _canvas = go.GetComponent<Canvas>();
        if (_canvas == null)
        {
            _canvas = go.AddComponent<Canvas>();
        }
        _canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = UIManager.Instance.UICamera;
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
        _canvascaler.referenceResolution = UIManager.Instance.Resolution;

        //raycaster
        GraphicRaycaster _canvasraycaster = go.GetComponent<GraphicRaycaster>();
        if (_canvasraycaster == null)
        {
            _canvasraycaster = go.AddComponent<GraphicRaycaster>();
        }

    }
}
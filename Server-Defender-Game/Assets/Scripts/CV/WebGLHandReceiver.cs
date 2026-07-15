using System;
using System.Runtime.InteropServices;
using System.Globalization;
using UnityEngine;

public struct HandData
{
    public float X;
    public float Y;
    public bool IsPinching;
    public bool IsTracking; // True if we received valid data recently
}

public class WebGLHandReceiver : MonoBehaviour
{
    public static WebGLHandReceiver Instance { get; private set; }

    public HandData CurrentHandData;

    // Use DllImport to map the external JS function in our .jslib
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetHandDataStringJS();

    [DllImport("__Internal")]
    private static extern void SetCameraVisibleJS(bool isVisible);
#endif

    public void SetCameraVisible(bool isVisible)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetCameraVisibleJS(isVisible);
#endif
    }

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        ReceiveDataFromJS();
    }

    private void ReceiveDataFromJS()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // Call the JS function mapped via .jslib
            string rawData = GetHandDataStringJS();
            
            if (string.IsNullOrEmpty(rawData))
            {
                CurrentHandData.IsTracking = false;
                return;
            }

            // Expected format: "X,Y,IsPinching" e.g., "0.45,0.62,1"
            string[] parts = rawData.Split(',');
            
            if (parts.Length >= 3)
            {
                // Use InvariantCulture to avoid parsing errors with commas/decimals in different regions
                CurrentHandData.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
                CurrentHandData.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                CurrentHandData.IsPinching = parts[2] == "1";
                CurrentHandData.IsTracking = true;
            }
            else
            {
                CurrentHandData.IsTracking = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing hand data: " + e.Message);
            CurrentHandData.IsTracking = false;
        }
#else
        // Fallback for Unity Editor testing without WebGL
        CurrentHandData.IsTracking = false;
        
        if (Input.GetMouseButton(0)) 
        {
            Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            CurrentHandData.X = mousePos.x;
            CurrentHandData.Y = 1f - mousePos.y; // Đảo ngược Y để giống chuẩn MediaPipe (0 ở trên, 1 ở dưới)
            CurrentHandData.IsPinching = true;
            CurrentHandData.IsTracking = true;
        }
#endif
    }
}

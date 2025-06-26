using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Lifecycle;
using Android.Content;
using Android.Util;
using Java.Util.Concurrent;
using Android.Runtime;
using Android.Views;
using System;
using AndroidX.Camera.Video;
using AndroidX.Core.Content;
using RTMPStreamerApp.Services;
using RTMPStreamerApp.Helpers;

namespace RTMPStreamerApp
{
    [Activity(Label = "RTMP Streamer", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        PreviewView previewView;
        Spinner cameraSelectorSpinner;
        SeekBar zoomSeekBar;
        Button streamButton;
        TextView statusText;

        ICamera camera;
        ProcessCameraProvider cameraProvider;
        CameraSelector cameraSelector;
        string[] cameraList;
        bool isStreaming = false;

        const string TAG = "MainActivity";
        string rtmpUrl = "rtmp://your-server/live/stream";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            previewView = FindViewById<PreviewView>(Resource.Id.previewView);
            cameraSelectorSpinner = FindViewById<Spinner>(Resource.Id.cameraSelector);
            zoomSeekBar = FindViewById<SeekBar>(Resource.Id.zoomSeekBar);
            streamButton = FindViewById<Button>(Resource.Id.streamButton);
            statusText = FindViewById<TextView>(Resource.Id.statusText);

            rtmpUrl = SettingsHelper.GetSavedUrl(this) ?? rtmpUrl;

            streamButton.Click += ToggleStreaming;
            zoomSeekBar.ProgressChanged += ZoomSeekBar_ProgressChanged;

            StartCamera();
        }

        void StartCamera()
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(this);
            cameraProviderFuture.AddListener(new Runnable(() =>
            {
                cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();
                SetupCameraSelector();
                BindPreview();
            }), ContextCompat.GetMainExecutor(this));
        }

        void SetupCameraSelector()
        {
            var hasBack = cameraProvider.HasCamera(CameraSelector.DefaultBackCamera);
            var hasFront = cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera);

            var options = new System.Collections.Generic.List<string>();
            if (hasBack) options.Add("Back");
            if (hasFront) options.Add("Front");

            cameraList = options.ToArray();
            cameraSelectorSpinner.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, cameraList);
            cameraSelectorSpinner.ItemSelected += CameraSelectorSpinner_ItemSelected;

            var savedCamera = SettingsHelper.GetSavedCamera(this);
            int index = Array.IndexOf(cameraList, savedCamera);
            cameraSelectorSpinner.SetSelection(index >= 0 ? index : 0);
        }

        void CameraSelectorSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string selected = cameraList[e.Position];
            cameraSelector = selected == "Front" ? CameraSelector.DefaultFrontCamera : CameraSelector.DefaultBackCamera;
            SettingsHelper.SaveCamera(this, selected);
            BindPreview();
        }

        void BindPreview()
        {
            cameraProvider.UnbindAll();

            var preview = new Preview.Builder().Build();
            var builder = new VideoCapture.Builder();
            var videoCapture = builder.Build();

            preview.SetSurfaceProvider(previewView.SurfaceProvider);

            camera = cameraProvider.BindToLifecycle((ILifecycleOwner)this, cameraSelector, preview);
            zoomSeekBar.Max = 100;

            float zoomRatio = SettingsHelper.GetSavedZoom(this);
            SetZoom(zoomRatio);
        }

        void ZoomSeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (camera == null) return;
            float zoom = 1f + (e.Progress / 100f) * (camera.CameraInfo.ZoomState.Value.MaxZoomRatio - 1f);
            SetZoom(zoom);
            SettingsHelper.SaveZoom(this, zoom);
        }

        void SetZoom(float zoom)
        {
            try
            {
                camera.CameraControl.SetZoomRatio(zoom);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Zoom error: {ex.Message}");
            }
        }

        void ToggleStreaming(object sender, EventArgs e)
        {
            if (!isStreaming)
            {
                var intent = new Intent(this, typeof(RTMPService));
                intent.PutExtra("url", rtmpUrl);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                    StartForegroundService(intent);
                else
                    StartService(intent);

                streamButton.Text = "Stop Stream";
                statusText.Text = "Streaming...";
            }
            else
            {
                StopService(new Intent(this, typeof(RTMPService)));
                streamButton.Text = "Start Stream";
                statusText.Text = "Idle";
            }

            isStreaming = !isStreaming;
        }
    }
}

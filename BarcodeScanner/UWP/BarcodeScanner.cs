using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Zebble;
using ZXing;


namespace Zebble.Plugin
{
    public partial class BarcodeScanner
    {

        DisplayInformation displayInformation;
        DisplayRequest displayRequest;
        MediaCapture mediaCapture;       
        bool isMediaCaptureInitialized = false;
        Timer timerPreview;
        bool stopping = false;
        volatile bool isAnalyzing = false;
        static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");
        public Action<Result> ScanCallback { get; set; }

       
        public async Task<string> DoScan(Action<ZXing.Result> scanCallback)
        {
            string result = "";

            try
            {
                await Device.UIThread.Run(async () =>
                {
                    result = await StartScanning(scanCallback);
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "No camera available")
                {                   
                    var temp = await Device.Media.PickPhoto();

                    if (temp != null)
                    {
                        await Device.UIThread.Run(async () =>
                        {
                            result = await StartLoadImage(temp);
                        });
                    }
                }
                else
                    throw ex;
            }
            return await Task.FromResult(result);
        }

        async Task<string> StartLoadImage(System.IO.FileInfo temp)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(temp.FullName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                WriteableBitmap image = new WriteableBitmap(1, 1);
                image.SetSource(stream);
                //   WriteableBitmapImage.Source = image;

                var reader = new BarcodeReader();

                var resultDecode = reader.Decode(image);
                // do something with the result
                if (resultDecode != null)
                    return resultDecode.Text;

            }
            return "";
        }

        async Task<string> StartScanning(Action<ZXing.Result> scanCallback)
        {
            if (stopping)
                return "";

            displayInformation = DisplayInformation.GetForCurrentView();
            displayRequest = new DisplayRequest();
            bool processing = false;
            bool isAnalyzing = false;
            // Information about the camera device.


            isAnalyzing = true;
            BarcodeReader reader;
            ZXing.Result result = null;
            ScanCallback = scanCallback;
            // Find which device to use
            var preferredCamera = await GetFilteredCameraOrDefaultAsync();
            if (preferredCamera == null)
            {
                System.Diagnostics.Debug.WriteLine("No camera available");
                isMediaCaptureInitialized = false;

              
                throw new Exception("No camera available");
            }


            mediaCapture = new MediaCapture();

            // Initialize the capture with the settings above
            try
            {
                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    VideoDeviceId = preferredCamera.Id
                });
                isMediaCaptureInitialized = true;
            }
            catch (UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine("Denied access to the camera");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception when init MediaCapture: {0}", ex);
            }

            if (!isMediaCaptureInitialized)
                return null;

            // Set the capture element's source to show it in the UI
            CaptureElement captureElement = new CaptureElement();
            captureElement.Source = mediaCapture;

            // Start the preview
            await mediaCapture.StartPreviewAsync();


            // Get all the available resolutions for preview
            var availableProperties = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
            var availableResolutions = new List<CameraResolution>();
            foreach (var ap in availableProperties)
            {
                var vp = (VideoEncodingProperties)ap;
                System.Diagnostics.Debug.WriteLine("Camera Preview Resolution: {0}x{1}", vp.Width, vp.Height);
                availableResolutions.Add(new CameraResolution { Width = (int)vp.Width, Height = (int)vp.Height });
            }
            CameraResolution previewResolution = null;


            // If the user did not specify a resolution, let's try and find a suitable one
            if (previewResolution == null)
            {
                // Loop through all supported sizes
                foreach (var sps in availableResolutions)
                {
                    // Find one that's >= 640x360 but <= 1000x1000
                    // This will likely pick the *smallest* size in that range, which should be fine
                    if (sps.Width >= 640 && sps.Width <= 1000 && sps.Height >= 360 && sps.Height <= 1000)
                    {
                        previewResolution = new CameraResolution
                        {
                            Width = sps.Width,
                            Height = sps.Height
                        };
                        break;
                    }
                }
            }

            if (previewResolution == null)
                previewResolution = availableResolutions.LastOrDefault();

            // Find the matching property based on the selection, again
            var chosenProp = availableProperties.FirstOrDefault(ap => ((VideoEncodingProperties)ap).Width == previewResolution.Width && ((VideoEncodingProperties)ap).Height == previewResolution.Height);

            await SetupAutoFocus();

            reader = new BarcodeReader();

            timerPreview = new Timer(async (state) =>
            {

                var delay = 150;

                if (stopping || processing || !isAnalyzing
                || (mediaCapture == null || mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming))
                {
                    timerPreview.Change(delay, Timeout.Infinite);
                    return;
                }



                processing = true;

            
                try
                {
                    var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                    var frame = await mediaCapture.GetPreviewFrameAsync(videoFrame);

                    SoftwareBitmap frameBitmap = frame.SoftwareBitmap;

                    WriteableBitmap bitmap = new WriteableBitmap(frameBitmap.PixelWidth, frameBitmap.PixelHeight);

                    frameBitmap.CopyToBuffer(bitmap.PixelBuffer);

                    result = reader.Decode(bitmap);
                }
                catch (Exception ex)
                {

                }



                // Check if a result was found
                if (result != null && !string.IsNullOrEmpty(result.Text))
                {
                    delay = 1000;
                    ScanCallback(result);
                }

                processing = false;

                timerPreview.Change(delay, Timeout.Infinite);

            }, null, 300, Timeout.Infinite);

            return result != null ? result.Text :"";
        }

        async Task<DeviceInformation> GetFilteredCameraOrDefaultAsync()
        {
            var videoCaptureDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            var useFront = false;

            var selectedCamera = videoCaptureDevices.FirstOrDefault(vcd => vcd.EnclosureLocation != null
                && ((!useFront && vcd.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back)
                    || (useFront && vcd.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front)));


            // we fall back to the first camera that we can find.  
            if (selectedCamera == null)
            {
                var whichCamera = useFront ? "front" : "back";
                System.Diagnostics.Debug.WriteLine("Finding " + whichCamera + " camera failed, opening first available camera");
                selectedCamera = videoCaptureDevices.FirstOrDefault();
            }

            return selectedCamera;
        }

        async Task SetupAutoFocus()
        {
            if (IsFocusSupported)
            {
                var focusControl = mediaCapture.VideoDeviceController.FocusControl;

                var focusSettings = new FocusSettings();
                focusSettings.AutoFocusRange = focusControl.SupportedFocusRanges.Contains(AutoFocusRange.FullRange)
                    ? AutoFocusRange.FullRange
                    : focusControl.SupportedFocusRanges.FirstOrDefault();

                var supportedFocusModes = focusControl.SupportedFocusModes;
                if (supportedFocusModes.Contains(FocusMode.Continuous))
                {
                    focusSettings.Mode = FocusMode.Continuous;
                }
                else if (supportedFocusModes.Contains(FocusMode.Auto))
                {
                    focusSettings.Mode = FocusMode.Auto;
                }

                if (focusSettings.Mode == FocusMode.Continuous || focusSettings.Mode == FocusMode.Auto)
                {
                    focusSettings.WaitForFocus = false;
                    focusControl.Configure(focusSettings);
                    await focusControl.FocusAsync();
                }
            }
        }
        bool IsFocusSupported
        {
            get
            {
                return mediaCapture != null
                    && isMediaCaptureInitialized
                    && mediaCapture.VideoDeviceController != null
                    && mediaCapture.VideoDeviceController.FocusControl != null
                    && mediaCapture.VideoDeviceController.FocusControl.Supported;
            }
        }


        public async void StopScanning()
        {
            if (stopping)
                return;

            stopping = true;
            isAnalyzing = false;

            displayRequest.RequestRelease();

            try
            {
                if (IsTorchOn)
                    Torch(false);
                if (isMediaCaptureInitialized)
                    await mediaCapture.StopPreviewAsync();               
            }
            catch { }
            finally
            {
                //second execution from sample will crash if the object is not properly disposed (always on mobile, sometimes on desktop)
                if (mediaCapture != null)
                    mediaCapture.Dispose();
            }

            //this solves a crash occuring when the user rotates the screen after the QR scanning is closed
            ///displayInformation.OrientationChanged -= displayInformation_OrientationChanged;

            if (timerPreview != null)
                timerPreview.Change(Timeout.Infinite, Timeout.Infinite);
            stopping = false;
        }

        public void Torch(bool on)
        {
            if (HasTorch)
                mediaCapture.VideoDeviceController.TorchControl.Enabled = on;
        }

        public bool HasTorch
        {
            get
            {
                return mediaCapture != null
                    && mediaCapture.VideoDeviceController != null
                    && mediaCapture.VideoDeviceController.TorchControl != null
                    && mediaCapture.VideoDeviceController.TorchControl.Supported;
            }
        }

        public bool IsTorchOn
        {
            get
            {
                return HasTorch && mediaCapture.VideoDeviceController.TorchControl.Enabled;
            }
        }
    }

    public class CameraResolution
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

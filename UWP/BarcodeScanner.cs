using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Zebble.Plugin
{
    public partial class BarcodeScanner
    {
        CameraView cameraView;

        CaptureElement captureElement;
        DisplayInformation displayInformation;
        DisplayRequest displayRequest;
        MediaCapture mediaCapture;
        bool isMediaCaptureInitialized = false;
        Timer timerPreview;
        bool stopping = false;

        public Action<BarcodeResult> ScanCallback { get; set; }


        public async Task DoScanAsync(Action<BarcodeResult> scanCallback, bool useCamera = true)
        {

            if (useCamera)
            {
                try
                {
                    await Device.UIThread.Run(async () =>
                    {
                        await StartScanning(scanCallback);
                    });
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("No camera available"))
                    {
                        useCamera = false;
                    }
                    else
                        throw ex;
                }
            }

            if (!useCamera)
            {
                var temp = await Device.Media.PickPhoto();

                if (temp != null)
                {
                    await Device.UIThread.Run(async () =>
                    {
                        var result = await LoadImage(temp);

                        ScanCallback(result);
                    });
                }
            }
        }

        async Task<BarcodeResult> LoadImage(System.IO.FileInfo temp)
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
                {

                    var barcodeResult = new BarcodeResult();

                    barcodeResult.Format = (Format)resultDecode.BarcodeFormat;
                    barcodeResult.Text = resultDecode.Text;
                    return barcodeResult;

                }

            }
            return null;
        }

        async Task StartScanning(Action<BarcodeResult> scanCallback)
        {
            if (stopping)
                return;
            await Add(cameraView = new CameraView());



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

            //  throw new Exception("No camera available");
            if (preferredCamera == null)
            {
                System.Diagnostics.Debug.WriteLine("No camera available");
                isMediaCaptureInitialized = false;

                throw new Exception("No camera available");
            }

            reader = new BarcodeReader();
            await cameraView.WhenShown(() =>
            {
                Device.UIThread.Run(() =>
                {
                    var border = cameraView.Native as Windows.UI.Xaml.Controls.Border;

                    captureElement = border.Child as CaptureElement;

                    mediaCapture = captureElement.Source;

                    timerPreview = new Timer(async state =>
                     {
                         var delay = 150;

                         if (stopping)
                             return;

                         if (processing || !isAnalyzing
                         || (mediaCapture == null || mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming))
                         {
                             timerPreview.Change(delay, Timeout.Infinite);
                             return;
                         }

                         processing = true;

                         try
                         {
                             await Device.UIThread.Run(async () =>
                             {
                                 var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                                 var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                                 var frame = await mediaCapture.GetPreviewFrameAsync(videoFrame);
                                 var frameBitmap = frame.SoftwareBitmap;
                                 var bitmap = new WriteableBitmap(frameBitmap.PixelWidth, frameBitmap.PixelHeight);

                                 frameBitmap.CopyToBuffer(bitmap.PixelBuffer);
                                 result = reader.Decode(bitmap);
                             });
                         }
                         catch (Exception ex)
                         {

                         }


                         // Check if a result was found
                         if (result != null && !string.IsNullOrEmpty(result.Text))
                         {
                             delay = 1000;
                             var barcodeResult = new BarcodeResult()
                             {
                                 Format = (Format)result.BarcodeFormat,
                                 Text = result.Text
                             };
                             ScanCallback(barcodeResult);
                         }

                         processing = false;

                         timerPreview.Change(delay, Timeout.Infinite);

                     }, null, 300, Timeout.Infinite);
                });
            });
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

        public async void StopScanning()
        {
            if (stopping)
                return;

            stopping = true;
            try
            {
                displayRequest.RequestRelease();

                if (IsTorchOn) Torch(false);
                if (isMediaCaptureInitialized)
                    await mediaCapture.StopPreviewAsync();
            }
            catch { }
            finally
            {
                //second execution from sample will crash if the object is not properly disposed (always on mobile, sometimes on desktop)
                mediaCapture?.Dispose();
            }

            //this solves a crash occuring when the user rotates the screen after the QR scanning is closed
            ///displayInformation.OrientationChanged -= displayInformation_OrientationChanged;

            timerPreview?.Change(Timeout.Infinite, Timeout.Infinite);
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
}
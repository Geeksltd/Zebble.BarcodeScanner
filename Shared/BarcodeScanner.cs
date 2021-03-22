namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Essentials;
    using ZXing;
    using ZXing.Mobile;

    public partial class BarcodeScanner
    {
        MobileBarcodeScanner Scanner;
        public List<BarcodeFormat> PossibleFormats { get; set; }
        public View ScanningOverlay;

        public async Task<BarcodeResult> Scan(bool useFrontCamera = false, OnError errorAction = OnError.Alert)
        {
            var tcsResult = new TaskCompletionSource<BarcodeResult>();

            try
            {
                return await Thread.UI.Run(() => DoScan(useFrontCamera));
            }
            catch (Exception ex)
            {
                await errorAction.Apply(ex, "Failed to scan a barcode");
                return null;
            }
        }

        async Task<BarcodeResult> DoScan(bool useFrontCamera)
        {
            InitializeScanner();

            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = PossibleFormats ?? new List<BarcodeFormat> { BarcodeFormat.All_1D },
                UseFrontCameraIfAvailable = useFrontCamera,
                CameraResolutionSelector = SelectLowestResolutionMatchingDisplayAspectRatio
            };

            if (ScanningOverlay != null)
            {
                Scanner.UseCustomOverlay = true;
                var overlay = await ScanningOverlay.Render();
                Scanner.CustomOverlay = overlay.Native();
            }

#if IOS
            var result = await Scanner.Scan(options, useAVCaptureEngine: true);
#else
            var result = await Scanner.Scan(options);
#endif

            if (result == null) return null;

            return new BarcodeResult
            {
                Format = result.BarcodeFormat,
                Text = result.Text
            };
        }

        public class BarcodeResult
        {
            public string Text { get; set; }

            public BarcodeFormat Format { get; set; }
        }

        static CameraResolution SelectLowestResolutionMatchingDisplayAspectRatio(List<CameraResolution> availableResolutions)
        {
            CameraResolution result = null;

            // A tolerance of 0.1 should not be visible to the user
            var aspectTolerance = 0.1;
            var display = DeviceDisplay.MainDisplayInfo;
            var isPortrait = display.Orientation == DisplayOrientation.Portrait;
            var displayOrientationHeight = isPortrait ? display.Height : display.Width;
            var displayOrientationWidth = isPortrait ? display.Width : display.Height;

            // Calculating our targetRatio
            var targetRatio = displayOrientationHeight / displayOrientationWidth;
            var targetHeight = displayOrientationHeight;
            var minDiff = double.MaxValue;

            // Camera API lists all available resolutions from highest to lowest, perfect for us
            // making use of this sorting, following code runs some comparisons to select the lowest resolution that matches the screen aspect ratio and lies within tolerance
            // selecting the lowest makes Qr detection actual faster most of the time
            foreach (var r in availableResolutions.Where(r => Math.Abs(((double)r.Width / r.Height) - targetRatio) < aspectTolerance))
            {
                // Slowly going down the list to the lowest matching solution with the correct aspect ratio
                if (Math.Abs(r.Height - targetHeight) < minDiff)
                    minDiff = Math.Abs(r.Height - targetHeight);

                result = new CameraResolution { Width = r.Width, Height = r.Height };
            }

            return result;
        }
    }
}
﻿namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
                UseFrontCameraIfAvailable = useFrontCamera
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
    }
}
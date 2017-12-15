namespace Zebble
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
        public View ScanningOlerlay;

        public async Task<BarcodeResult> Scan(bool useCamera = true, OnError errorAction = OnError.Alert)
        {
            var tcsResult = new TaskCompletionSource<BarcodeResult>();

            try
            {
                return await Device.UIThread.Run(() => DoScan(useCamera));
            }
            catch (Exception ex)
            {
                await errorAction.Apply(ex, "Failed to scan a barcode");
                return null;
            }
        }

        async Task<BarcodeResult> DoScan(bool useCamera)
        {
            InitializeScanner();

            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = PossibleFormats ?? new List<ZXing.BarcodeFormat>() { ZXing.BarcodeFormat.All_1D }
            };

            if (ScanningOlerlay != null)
            {
                Scanner.UseCustomOverlay = true;
                var overlay = await ScanningOlerlay.Render();
                Scanner.CustomOverlay = overlay.Native();
            }

            var result = await Scanner.Scan(options);

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
namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZXing;
    using ZXing.Mobile;

    public partial class BarcodeScanner
    {
        public List<BarcodeFormat> PossibleFormats { get; set; }
        public MobileBarcodeScanner Scanner;
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

            Scanner = new MobileBarcodeScanner();

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

        // [Flags]
        // public enum BarcodeFormat
        // {
        //    AZTEC = 1,
        //    CODABAR = 2,
        //    CODE_39 = 4,
        //    CODE_93 = 8,
        //    CODE_128 = 16,
        //    DATA_MATRIX = 32,
        //    EAN_8 = 64,
        //    EAN_13 = 128,
        //    ITF = 256,
        //    MAXICODE = 512,
        //    PDF_417 = 1024,
        //    QR_CODE = 2048,
        //    RSS_14 = 4096,
        //    RSS_EXPANDED = 8192,
        //    UPC_A = 16384,
        //    UPC_E = 32768,
        //    All_1D = 61918,
        //    UPC_EAN_EXTENSION = 65536,
        //    MSI = 131072,
        //    PLESSEY = 262144,
        //    IMB = 524288
        // }
    }
}
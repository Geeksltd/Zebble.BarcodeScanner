namespace Zebble.Plugin
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ZXing.Mobile;

    public partial class BarcodeScanner
    {
        async Task<BarcodeResult> DoScan(bool useCamera)
        {
            MobileBarcodeScanner.Initialize(UIRuntime.CurrentActivity.Application);

            var options = new MobileBarcodeScanningOptions()
            {
                PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.EAN_8, ZXing.BarcodeFormat.EAN_13 }
            };

            var scanner = new MobileBarcodeScanner();

            var result = await scanner.Scan();

            if (result == null) return null;

            return new BarcodeResult
            {
                Format = (Format)result.BarcodeFormat,
                Text = result.Text
            };
        }

        public async void StopScanning()
        {
        }
    }
}
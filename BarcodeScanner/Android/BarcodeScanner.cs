using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Zebble;
using System.Threading.Tasks;
using ZXing.Mobile;

namespace Zebble.Plugin
{
    public partial class BarcodeScanner
    {

        public async Task<BarcodeResult> DoScanAsync( Action<ZXing.Result> scanCallback , bool useCamera = true)
        {
            Android.App.Application app = new Android.App.Application();
            MobileBarcodeScanner scanner;
            MobileBarcodeScanner.Initialize(app);


            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>() {
                  ZXing.BarcodeFormat.EAN_8, ZXing.BarcodeFormat.EAN_13
             };
            try
            {
                string rs = string.Empty;
                scanner = new ZXing.Mobile.MobileBarcodeScanner();

                var result = await scanner.Scan();
                if (result != null)
                {

                    var barcodeResult = new BarcodeResult();

                    barcodeResult.Format = (Format)result.BarcodeFormat;
                    barcodeResult.Text = result.Text;
                    return barcodeResult;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }


   

        public async void StopScanning()
        {

        }
    }
}
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

        public Task<string> DoScan(Action<ZXing.Result> scanCallback)
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
                //scanner.ScanContinuously((result) =>
                //{
                //    if (result != null)
                //    {
                //        //Alert.Show("Result : ", result.ToString());                        
                //        rs=result.Text;
                //        return;
                //    }
                //});
                var result= scanner.Scan();
                if (result.Result != null)
                {
                    rs = result.Result.Text;                  
                }
                if (!string.IsNullOrEmpty(rs))
                    return Task.FromResult(rs);
            }
            catch (Exception ex)
            {

            }
           // if (result.Result != null)
          //      return Task.FromResult(result.Result.Text);

            return Task.FromResult("");
        }

    }
}
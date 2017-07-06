namespace Zebble.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Zebble;

    public partial class BarcodeScanner : CustomRenderedView<BarcodeScannerRenderer>
    {
        public async Task<string> Scan(OnError errorAction = OnError.Alert)
        {
            try
            {
                return await DoScan(r =>
                {
                    StopScanning();
                    //rrr = r.Text;
                });
            }
            catch (Exception ex)
            {
                await errorAction.Apply(ex, "Failed to scan a barcode");
                return null;
            }
        }
    }
}
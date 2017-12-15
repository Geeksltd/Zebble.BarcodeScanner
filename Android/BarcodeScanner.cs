namespace Zebble
{
    using ZXing.Mobile;

    public partial class BarcodeScanner
    {
        void InitializeScanner()
        {
            MobileBarcodeScanner.Initialize(UIRuntime.CurrentActivity.Application);
            Scanner = new MobileBarcodeScanner();
        }
    }
}
namespace Zebble
{
    using ZXing.Mobile;
    public partial class BarcodeScanner
    {
        void InitializeScanner()
        {
            Scanner = new MobileBarcodeScanner(UIRuntime.Window.RootViewController);
        }
    }
}
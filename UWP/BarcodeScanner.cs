namespace Zebble
{
    public partial class BarcodeScanner
    {
        void InitializeScanner()
        {
            ZXing.Net.Mobile.Forms.WindowsUniversal.ZXingScannerViewRenderer.Init();
            Scanner = new MobileBarcodeScanner();
        }
    }
}
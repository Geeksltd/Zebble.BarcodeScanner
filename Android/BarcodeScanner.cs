namespace Zebble
{
    using ZXing.Mobile;

    public partial class BarcodeScanner
    {
        void InitializeScanner()
        {
            Xamarin.Essentials.Platform.Init(UIRuntime.CurrentActivity.Application);
            Scanner = new MobileBarcodeScanner();
        }
    }
}
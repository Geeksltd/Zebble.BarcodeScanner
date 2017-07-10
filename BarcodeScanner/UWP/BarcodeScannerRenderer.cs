namespace Zebble.Plugin
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Windows.Graphics.Display;
    using Windows.Media.Capture;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Windows.System.Display;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;
    using Zebble;
    using ZXing;
   
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BarcodeScannerRenderer : ICustomRenderer
    {
        Windows.UI.Xaml.Controls.Canvas Result;


        // BarcodeScanner View;
        View View;

        public object Render(object view)
        {
            View = (View)view;
            Result = new Windows.UI.Xaml.Controls.Canvas();
            
            return Result;
        }
 

        public void Dispose() {

        }
        /* => Result.Dispose(); */
    }



}
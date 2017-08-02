namespace Zebble.Plugin.Renderer
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Android.Views;
    using Zebble;
    

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BarcodeScannerRenderer : INativeRenderer
    {
        BarcodeScanner View;
        TheNativeType Result;

        public Task<Android.Views.View> Render(Renderer renderer)
        {
            View = (BarcodeScanner)renderer.View
        }

        public object Render(object view)
        {
            View = (View)view;
            Result = new Canvas();
            return Result;
        }

        public void Dispose()
        {
        }  //=> Result.Dispose();


    }
}
namespace Zebble.Plugin
{
    using System;
    using System.ComponentModel;
    using Zebble;
    using TheNativeType = Canvas; //TODO: replace with the native Android type;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BarcodeScannerRenderer : ICustomRenderer
    {
        View View;
        TheNativeType Result;

        public object Render(object view)
        {
            View = (View)view;
            Result = new TheNativeType();      
            return Result;
        }

        public void Dispose() {
        }  //=> Result.Dispose();
    }
}
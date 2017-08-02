namespace Zebble.Plugin
{
    using System;
    using System.ComponentModel;
    using Zebble;
    using TheNativeType = System.Object; //TODO: replace with the native iOS type;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BarcodeScannerRenderer : ICustomRenderer
    {
        BarcodeScanner View;
        TheNativeType Result;

        public object Render(object view)
        {
            View = (BarcodeScanner)view;
            Result = new TheNativeType();

            // TODO: Map the methods to the native component:
            //View.MyMethod1Implementation = () => Result.EquivalentForMethod1();
            //View.MyMethod2Implementation = p1 => Result.EquivalentForMethod2(p1);
            //// ...

            //// TODO: Map the native component's events to the Zebble abstraction:
            //Result.EquivalentNativeEvent1 += (s, e) => View.MyEvent1.Raise();
            //Result.EquivalentNativeEvent2 += (s, e) => View.MyEvent2.Raise(e.UsefulArg);
            //// ...

            //// TODO: Map the property getter and setters to the native component:
            //View.GetMyPropertyImplementation = () => Result.MyPropertyEquivalent;
            //View.SetMyPropertyImplementation = v => Result.MyPropertyEquivalent = v;
            // ...

            return Result;
        }

        public void Dispose() { }// => Result.Dispose();
    }
}
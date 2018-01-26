[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.BarcodeScanner/master/Shared/NuGet/Icon.png "Zebble.BarcodeScanner"


## Zebble.BarcodeScanner

![logo]

Zebble.BarcodeScanner is a Zebble plugin based on the open source Barcode Library: ZXing (Zebra Crossing).


[![NuGet](https://img.shields.io/nuget/v/Zebble.BarcodeScanner.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.BarcodeScanner/)

<br>

### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.BarcodeScanner/](https://www.nuget.org/packages/Zebble.BarcodeScanner/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage
You can scan all 1D barcodes as simple as:
```csharp
var result = await new BarcodeScanner().Scan();
await Alert.Show(result.Text);
```
Also, when you need a custome layout for your scan page.
```csharp
async Task Scan()
{
	var bs = new BarcodeScanner();
	bs.ScanningOlerlay = await GetOverlay();
	var result = await bs.Scan();
	await Alert.Show(result.Text);
}

async Task<View> GetOverlay()
{
	var result = new Stack(RepeatDirection.Vertical);

	result.Width.BindTo(Root.Width);
	result.Height.BindTo(Root.Height);

	var canvasTop = GetCanvas();
	var canvasBottom = GetCanvas();

	canvasTop.Width.BindTo(Root.Width);
	canvasBottom.Width.BindTo(Root.Width);

	SetHeight(canvasTop, 0.3F);
	SetHeight(canvasBottom, 0.3F);

	var middleStack = new Stack(RepeatDirection.Horizontal);

	var canvasLeft = GetCanvas();
	var canvasRight = GetCanvas();
	var canvasCenter = new Canvas();

	canvasCenter.Border(all: 2, color: Colors.LawnGreen);

	canvasLeft.Width.BindTo(Root.Width, Root.Height, (w, h) => (w - (h * 0.4F)) / 2);
	canvasRight.Width.BindTo(Root.Width, Root.Height, (w, h) => (w - (h * 0.4F)) / 2);
	canvasCenter.Width.BindTo(Root.Height, h => h * 0.4F);

	SetHeight(canvasLeft, 0.4F);
	SetHeight(canvasRight, 0.4F);
	SetHeight(canvasCenter, 0.4F);
	SetHeight(middleStack, 0.4F);

	await middleStack.Add(canvasLeft);
	await middleStack.Add(canvasCenter);
	await middleStack.Add(canvasRight);

	await result.Add(canvasTop);
	await result.Add(middleStack);
	await result.Add(canvasBottom);

	return result;
}

Canvas GetCanvas() => new Canvas { BackgroundColor = Colors.Black, Opacity = 0.25F };
void SetHeight(View view, float scale) => view.Height.BindTo(Root.Height, h => h * scale);
```

<br>


### Properties
| Property     | Type               | Android | iOS | Windows |
| :----------- | :-----------       | :------ | :-- | :------ |
| PossibleFormats | PossibleFormats | x       | x   | x       |
| ScanningOlerlay | Zebble.View     | x      | x   | x       |



<br>

### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| Scan         | Task&lt;BarcodeResult> | bool => useCamera = true,<br> OnError => errorAction = OnError.Alert| x       | x   | x       |

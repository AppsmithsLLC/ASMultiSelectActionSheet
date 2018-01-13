# Project Name
ASMultiSelectActionSheet
## Description
ASMultiSelectionActionSheet is a control built for Xamarin.iOS.  The style roughly resembles an UIAlertController with cancel action, while the functionality includes that of an Android MultiChoice AlertDialog.  

The control presents the user with one or more options.  Each option can be toggled on or off as the user requires.  If all options are toggled off, the action button is also disabled.  Optionally, the action button can be bound to any Action or ICommand object that you pass into the action sheet constructor.  Otherwise, you can respond to the action sheet manually from the delegate.

The dialog title, message and "OK" button titles are all customizable from the API. The control includes a very basic check-mark icon which can be changed to suit your project, but changing the icon programmatically is not currently exposed in the API.

## Demonstration
![Demo](https://github.com/AppsmithsLLC/ASMultiSelectActionSheet/blob/master/2018-01-12_13-14-03.gif "Demo")

## Requirements
This control requires Xamarin.iOS and iOS 7.0+.

## Installation
1. Copy the ASMultiSelectionActionSheetControl folder to your project.

* Note that Nuget installation  is forthcoming.
## Usage
After copying the folder:
1. Include `using ASMultiSelectionActionSheet` wherever you intend to launch the control.
2. Implement `IASMultiSelectActionSheet` and pass `this` as the caller delegate.

## Contributing
This control is new and it isn't perfect for all situations yet, but it has the potential to be robust and reusable.  Please consider contributing. 
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## History
1-12-18: 1.0.0 Initial commit

## Credits
Created by Will Smith ©2018.

The concept for this control is loosely based on [JGActionSheet](https://github.com/JonasGessner/JGActionSheet) by [Jonas Gessner](https://github.com/JonasGessner), however that control did not provide multi-selection functionality. This repo is not a fork since the original project is for iOS native and written in Objective-C.

## License
MIT License.
©2018 Appsmiths, LLC

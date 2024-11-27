# wpf-custom-window-snap
Test project in (.NET framework 4.8) WPF, including window with full windows snap support on custom caption buttons

If you don't want to manage all this features/issues by yourself, I recommend using [ControlzEx](https://github.com/ControlzEx/ControlzEx/) or any other library.
I've created this project as test for implementing these features to our legacy application, where I cannot simply change base classes and use any other library.

![snap-preview](https://github.com/user-attachments/assets/be5a75d1-48fc-4844-91da-fed239cbef6d)
*NOTE: In Debug build the resize can be a bit flickery, but in Release build everything is smooth*

This project features code that enables this features, when using `WindowStyle.None`:
- using Windows Snap feature (automatically snap window to all sides of screen or maximizing/restoring the window)
    - this is done using the `WindowChrome` feature (available from .NET Framework 4.5)
    - also, when using the `WindowChrome` object, you don't need to use `DragMove()` and handle resizing by yourself anymore - it's all there
- using Windows Snap preview feature (Windows 11+)
    - this is done by identifying Maximize button in `NCHITTEST (0x0084)` message as `HTMAXBUTTON`
    - see file `MainWindow.MaximizeSnap.cs`
    - this code also includes fix for maximize button effects not being triggered (hover and press) by explicitly setting internal dependency properties
- the Maximize feature of `WindowsStyle.None` requires fixing window size and position
    - this is done by using the code in `MainWindow.Fullscreen.cs`
    - also, there is need to refresh `WindowChrome` and set `ResizeBorderThickness` to zero (disable resizing), as it partially breaks the ability to put the window back "down"

The code in this repo is mix of various sources:
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.shell.windowchrome?view=netframework-4.8
- https://stackoverflow.com/questions/19280016/windowchrome-resizeborderthickness-issue
- https://stackoverflow.com/questions/69797178/support-windows-11-snap-layout-in-wpf-app
- https://github.com/dotnet/wpf/issues/4825
- https://gitlab.com/jplibraries/windowsextensionlibrary/
- and possibly some other places I've already forgot

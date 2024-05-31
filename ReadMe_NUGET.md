## Avalonia.NLogViewer

Inspired by https://github.com/erizet/NlogViewer, a viewer for WPF. I modified it to use it on Avalonia.

An improvement is that now it can display more than 100K messages per second.

Check more information here: https://github.com/Yang-RZ/Avalonia.NLogViewer

### Notes:  
* ```<StyleInclude Source="avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml"/>```should be added into the project or the viewer UI will be blank.  
* ```NLog.config``` file must be in the output directory or the program will throw an exception.
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using NLog.Common;
using NLog.Targets;
using NLog;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Security;
using System.Reflection;
using System.Resources;

namespace Avalonia.NLogViewer;

internal static class SR
{
    private static ResourceManager _resourceManager = new ResourceManager("ExceptionStringTable", typeof(SR).Assembly);

    internal static ResourceManager ResourceManager => _resourceManager;

    internal static string Get(string id)
    {
        string @string = _resourceManager.GetString(id);
        if (@string == null)
        {
            @string = _resourceManager.GetString("Unavailable");
        }

        return @string;
    }

    internal static string Get(string id, params object[] args)
    {
        string text = _resourceManager.GetString(id);
        if (text == null)
        {
            text = _resourceManager.GetString("Unavailable");
        }
        else if (args != null && args.Length != 0)
        {
            text = string.Format(CultureInfo.CurrentCulture, text, args);
        }

        return text;
    }
}

public class LengthConverter : TypeConverter
{
    private static string[] PixelUnitStrings = new string[4] { "px", "in", "cm", "pt" };

    private static double[] PixelUnitFactors = new double[4] { 1.0, 96.0, 37.795275590551178, 1.3333333333333333 };

    //
    // Summary:
    //     Determines whether conversion is possible from a specified type to a System.Double
    //     that represents an object's length.
    //
    // Parameters:
    //   typeDescriptorContext:
    //     Provides contextual information about a component.
    //
    //   sourceType:
    //     Identifies the data type to evaluate for conversion.
    //
    // Returns:
    //     true if conversion is possible; otherwise, false.
    public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
    {
        TypeCode typeCode = Type.GetTypeCode(sourceType);
        if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
        {
            return true;
        }

        return false;
    }

    //
    // Summary:
    //     Determines whether conversion is possible to a specified type from a System.Double
    //     that represents an object's length.
    //
    // Parameters:
    //   typeDescriptorContext:
    //     Provides contextual information about a component.
    //
    //   destinationType:
    //     Identifies the data type to evaluate for conversion.
    //
    // Returns:
    //     true if conversion to the destinationType is possible; otherwise, false.
    public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
    {
        if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
        {
            return true;
        }

        return false;
    }

    //
    // Summary:
    //     Converts instances of other data types into instances of System.Double that represent
    //     an object's length.
    //
    // Parameters:
    //   typeDescriptorContext:
    //     Provides contextual information about a component.
    //
    //   cultureInfo:
    //     Represents culture-specific information that is maintained during a conversion.
    //
    //
    //   source:
    //     Identifies the object that is being converted to System.Double.
    //
    // Returns:
    //     An instance of System.Double that is the value of the conversion.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     Occurs if the source is null.
    //
    //   T:System.ArgumentException:
    //     Occurs if the source is not null and is not a valid type for conversion.
    public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
    {
        if (source != null)
        {
            if (source is string)
            {
                return FromString((string)source, cultureInfo);
            }

            return Convert.ToDouble(source, cultureInfo);
        }

        throw GetConvertFromException(source);
    }

    //
    // Summary:
    //     Converts other types into instances of System.Double that represent an object's
    //     length.
    //
    // Parameters:
    //   typeDescriptorContext:
    //     Describes context information of a component, such as its container and System.ComponentModel.PropertyDescriptor.
    //
    //
    //   cultureInfo:
    //     Identifies culture-specific information, including the writing system and the
    //     calendar that is used.
    //
    //   value:
    //     Identifies the System.Object that is being converted.
    //
    //   destinationType:
    //     The data type that this instance of System.Double is being converted to.
    //
    // Returns:
    //     A new System.Object that is the value of the conversion.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     Occurs if the value is null.
    //
    //   T:System.ArgumentException:
    //     Occurs if the value is not null and is not a System.Windows.Media.Brush, or the
    //     destinationType is not valid.
    [SecurityCritical]
    public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
    {
        if (destinationType == null)
        {
            throw new ArgumentNullException("destinationType");
        }

        if (value != null && value is double num)
        {
            if (destinationType == typeof(string))
            {
                if (double.IsNaN(num))
                {
                    return "Auto";
                }

                return Convert.ToString(num, cultureInfo);
            }

            if (destinationType == typeof(InstanceDescriptor))
            {
                ConstructorInfo constructor = typeof(double).GetConstructor(new Type[1] { typeof(double) });
                return new InstanceDescriptor(constructor, new object[1] { num });
            }
        }

        throw GetConvertToException(value, destinationType);
    }

    internal static double FromString(string s, CultureInfo cultureInfo)
    {
        string text = s.Trim();
        string text2 = text.ToLowerInvariant();
        int length = text2.Length;
        int num = 0;
        double num2 = 1.0;
        if (text2 == "auto")
        {
            return double.NaN;
        }

        for (int i = 0; i < PixelUnitStrings.Length; i++)
        {
            if (text2.EndsWith(PixelUnitStrings[i], StringComparison.Ordinal))
            {
                num = PixelUnitStrings[i].Length;
                num2 = PixelUnitFactors[i];
                break;
            }
        }

        text = text.Substring(0, length - num);
        try
        {
            return Convert.ToDouble(text, cultureInfo) * num2;
        }
        catch (FormatException)
        {
            throw new FormatException(SR.Get("LengthFormatError", text));
        }
    }

    internal static string ToString(double l, CultureInfo cultureInfo)
    {
        if (double.IsNaN(l))
        {
            return "Auto";
        }

        return Convert.ToString(l, cultureInfo);
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Windows.LengthConverter class.
    public LengthConverter()
    {
    }
}

public partial class AvaNLogViewer : UserControl, INotifyPropertyChanged
{
    public DataGrid LogView { get { return logView; } }
    public event EventHandler ItemAdded = delegate { };
    public BufferedObservableCollection<LogEventViewModel> LogEntries { get; private set; }
    public ObservableCollection<NLog.LogLevel> LogLevels { get; private set; }

    //private NLog.LogLevel _SelectedLogLevel;
    //public NLog.LogLevel SelectedLogLevel
    //{
    //    get { return _SelectedLogLevel; }
    //    set
    //    {
    //        UpdateProperty(ref _SelectedLogLevel, value);
    //        //LogEntriesSource.
    //        LogEntriesSource.View.Refresh();
    //    }
    //}

    //private ObservableCollection<NLog.LogLevel> _SelectedLogLevels;
    //public ObservableCollection<NLog.LogLevel> SelectedLogLevels
    //{
    //    get { return _SelectedLogLevels; }
    //    set
    //    {
    //        UpdateProperty(ref _SelectedLogLevels, value);
    //        SelectedLogLevels.CollectionChanged += (s, e) =>
    //        {
    //            LogEntriesSource.View.Refresh();
    //        };
    //        //LogEntriesSource.
    //    }
    //}

    #region Binding
    protected bool UpdateProperty<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "", bool bForceUpdate = false)
    {
        if (object.Equals(properValue, newValue) && !bForceUpdate)
            return false;
        properValue = newValue;
        OnPropertyChanged(properName);
        return true;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    //public CollectionViewSource LogEntriesSource { get; private set; }
    public bool IsTargetConfigured { get; private set; }

    private double _TimeWidth = 120;
    [Description("Width of time column in pixels"), Category("Data")]
    [TypeConverter(typeof(LengthConverter))]
    public double TimeWidth
    {
        get { return _TimeWidth; }
        set { _TimeWidth = value; }
    }

    private double _LoggerNameWidth = 100;
    [Description("Width of Logger column in pixels, or auto if not specified"), Category("Data")]
    [TypeConverter(typeof(LengthConverter))]
    public double LoggerNameWidth
    {
        get { return _LoggerNameWidth; }
        set { _LoggerNameWidth = value; }
    }

    private double _LevelWidth = 75;
    [Description("Width of Level column in pixels"), Category("Data")]
    [TypeConverter(typeof(LengthConverter))]
    public double LevelWidth
    {
        get { return _LevelWidth; }
        set { _LevelWidth = value; }
    }

    private double _MessageWidth = 200;
    [Description("Width of Message column in pixels"), Category("Data")]
    [TypeConverter(typeof(LengthConverter))]
    public double MessageWidth
    {
        get { return _MessageWidth; }
        set { _MessageWidth = value; }
    }

    private double _ExceptionWidth = 100;
    [Description("Width of Exception column in pixels"), Category("Data")]
    [TypeConverter(typeof(LengthConverter))]
    public double ExceptionWidth
    {
        get { return _ExceptionWidth; }
        set { _ExceptionWidth = value; }
    }

    private int _MaxRowCount = 50;
    [Description("The maximum number of row count. The oldest log gets deleted. Set to 0 for unlimited count."), Category("Data")]
    [TypeConverter(typeof(Int32Converter))]
    public int MaxRowCount
    {
        get { return _MaxRowCount; }
        set { _MaxRowCount = value; }
    }

    private bool _autoScrollToLast = true;
    [Description("Automatically scrolls to the last log item in the viewer. Default is true."), Category("Data")]
    [TypeConverter(typeof(BooleanConverter))]
    public bool AutoScrollToLast
    {
        get { return _autoScrollToLast; }
        set { _autoScrollToLast = value; }
    }

    public AvaNLogViewer()
    {
        IsTargetConfigured = false;

        LogLevels = new ObservableCollection<LogLevel>();

        LogEntries = new BufferedObservableCollection<LogEventViewModel>(Avalonia.Threading.Dispatcher.UIThread);
        //LogEntriesSource = new CollectionViewSource();
        //LogEntriesSource.Source = LogEntries;
        //LogEntriesSource.Filter += LogEntriesSource_Filter;

        //SelectedLogLevels = new ObservableCollection<LogLevel>();
        //LogLevels.Add(NLog.LogLevel.Off);
        LogLevels.Add(NLog.LogLevel.Trace);
        LogLevels.Add(NLog.LogLevel.Debug);
        LogLevels.Add(NLog.LogLevel.Info);
        LogLevels.Add(NLog.LogLevel.Warn);
        LogLevels.Add(NLog.LogLevel.Error);
        LogLevels.Add(NLog.LogLevel.Fatal);

        //foreach (var item in LogLevels)
        //{
        //    SelectedLogLevels.Add(item);
        //}

        //SelectedLogLevel = LogLevels[0];


        //BindingOperations.EnableCollectionSynchronization(LogEntries, LogEntries.LockUpdate);
        LogEntries.ItemAdded += LogEntries_ItemAdded;
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            foreach (AvaNlogViewerTarget target in LogManager.Configuration.AllTargets.Where(t => t is AvaNlogViewerTarget).Cast<AvaNlogViewerTarget>())
            {
                IsTargetConfigured = true;
                target.LogReceived += LogReceived;
            }
        }

        DataContext = this;
    }


    //private void LogEntriesSource_Filter(object sender, FilterEventArgs e)
    //{
    //    e.Accepted = SelectedLogLevels.Select(l => l.Name).ToArray().Contains((e.Item as LogEventViewModel).Level);
    //}

    private void LogEntries_ItemAdded(object? sender, EventArgs e)
    {
        try
        {
            if (AutoScrollToLast) ScrollToLast();
            ItemAdded(this, (NLogEvent)((LogEventViewModel)sender).EventInfo);
        }
        catch (Exception ex)
        {

        }
    }

    protected void LogReceived(AsyncLogEventInfo log)
    {
        LogEventViewModel vm = new LogEventViewModel(log.LogEvent);

        if (MaxRowCount > 0)
            LogEntries.SetMaxLimit(MaxRowCount);
        LogEntries.AddToBuffer(vm);

        //Dispatcher.BeginInvoke(new Action(() =>
        //{
        //    if (AutoScrollToLast) ScrollToLast();
        //    //ItemAdded(this, (NLogEvent)log.LogEvent);
        //}));
    }
    public void Clear()
    {
        LogEntries.Clear();
    }

    public void ScrollToFirst()
    {
        if (LogEntries.Count <= 0) return;
        LogView.SelectedIndex = 0;
        ScrollToItem(LogView.SelectedItem);
    }
    public void ScrollToLast()
    {
        if (LogEntries.Count <= 0) return;
        LogView.SelectedIndex = LogEntries.Count - 1;
        ScrollToItem(LogView.SelectedItem);
    }

    private void ScrollToItem(object item)
    {
        LogView.ScrollIntoView(item, null);
    }

    private void MenuItem_Clear_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LogEntries.Clear();
    }
}

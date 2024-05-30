using DynamicData.Binding;
using NLog;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Avalonia.NLogViewer.Example.ViewModels;

public class MainViewModel : ViewModelBase
{
    NLog.Logger logger_ = NLog.LogManager.GetCurrentClassLogger();

    private ICommand? LogMessagesCommand_;
    public ICommand? LogMessagesCommand
    {
        get
        {
            if (LogMessagesCommand_ is null)
            {
                LogMessagesCommand_ = ReactiveCommand.Create(async (int count) =>
                {
                    InLogging = true;
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < count; i++)
                        {
                            switch (i % 5)
                            {
                                case 0:
                                    logger_.Trace($"Message {i}");
                                    break;
                                case 1:
                                    logger_.Info($"Message {i}");
                                    break;
                                case 2:
                                    logger_.Warn($"Message {i}");
                                    break;
                                case 3:
                                    logger_.Error($"Message {i}");
                                    break;
                                case 4:
                                    logger_.Fatal($"Message {i}");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }).ConfigureAwait(true);

                    InLogging = false;
                }, this.WhenAnyValue(x => x.InLogging, (l) => !l));
            }
            return LogMessagesCommand_;
        }
    }

    private bool InLogging_ = false;
    public bool InLogging
    {
        get => InLogging_;
        set
        {
            this.RaiseAndSetIfChanged(ref InLogging_, value);
        }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinEnv.Models;

/// <summary>
/// 路径项模型（用于分号分隔的值）
/// </summary>
public class PathItem : INotifyPropertyChanged
{
    private string _path = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public PathItem() { }

    public PathItem(string path)
    {
        _path = path;
    }

    /// <summary>
    /// 路径值
    /// </summary>
    public string Path
    {
        get => _path;
        set
        {
            if (_path != value)
            {
                _path = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

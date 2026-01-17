using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinEnv.Models;

/// <summary>
/// 环境变量数据模型
/// </summary>
public class EnvironmentVariable : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private string _originalName = string.Empty;
    private bool _isNew;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 环境变量名称
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 环境变量值
    /// </summary>
    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 原始名称（用于跟踪重命名操作）
    /// </summary>
    public string OriginalName
    {
        get => _originalName;
        set
        {
            if (_originalName != value)
            {
                _originalName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否为新添加的变量
    /// </summary>
    public bool IsNew
    {
        get => _isNew;
        set
        {
            if (_isNew != value)
            {
                _isNew = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

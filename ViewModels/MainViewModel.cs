using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using WinEnv.Models;
using WinEnv.Services;

namespace WinEnv.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly EnvironmentVariableService _service;
    private EnvironmentVariable? _selectedVariable;
    private string _searchText = string.Empty;
    private ObservableCollection<EnvironmentVariable> _filteredVariables = new();
    private bool _isSystemMode = false;
    private readonly bool _isAdmin;
    private ObservableCollection<PathItem> _pathItems = new();
    private PathItem? _selectedPathItem;

    public MainViewModel()
    {
        _service = new EnvironmentVariableService();
        Variables = new ObservableCollection<EnvironmentVariable>();
        
        // 检查是否以管理员身份运行
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        _isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        
        // 初始化命令
        RefreshCommand = new RelayCommand(Refresh);
        AddCommand = new RelayCommand(Add);
        EditCommand = new RelayCommand(Edit, () => SelectedVariable != null);
        DeleteCommand = new RelayCommand(Delete, () => SelectedVariable != null);
        SaveCommand = new RelayCommand(Save);
        SwitchToUserCommand = new RelayCommand(SwitchToUser);
        SwitchToSystemCommand = new RelayCommand(SwitchToSystem);
        AddPathItemCommand = new RelayCommand(AddPathItem);
        RemovePathItemCommand = new RelayCommand(RemovePathItem, () => SelectedPathItem != null);
        MovePathItemUpCommand = new RelayCommand(MovePathItemUp, () => SelectedPathItem != null && PathItems.IndexOf(SelectedPathItem) > 0);
        MovePathItemDownCommand = new RelayCommand(MovePathItemDown, () => SelectedPathItem != null && PathItems.IndexOf(SelectedPathItem) < PathItems.Count - 1);

        // 加载数据
        Refresh();
    }

    /// <summary>
    /// 所有环境变量
    /// </summary>
    public ObservableCollection<EnvironmentVariable> Variables { get; }

    /// <summary>
    /// 过滤后的环境变量
    /// </summary>
    public ObservableCollection<EnvironmentVariable> FilteredVariables
    {
        get => _filteredVariables;
        set => SetProperty(ref _filteredVariables, value);
    }

    /// <summary>
    /// 当前选中的环境变量
    /// </summary>
    public EnvironmentVariable? SelectedVariable
    {
        get => _selectedVariable;
        set
        {
            if (SetProperty(ref _selectedVariable, value))
            {
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 搜索文本
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    /// <summary>
    /// 编辑模式下的变量
    /// </summary>
    private EnvironmentVariable? _editingVariable;
    public EnvironmentVariable? EditingVariable
    {
        get => _editingVariable;
        private set => SetProperty(ref _editingVariable, value);
    }

    /// <summary>
    /// 是否正在编辑
    /// </summary>
    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    /// <summary>
    /// 编辑对话框标题
    /// </summary>
    private string _editDialogTitle = string.Empty;
    public string EditDialogTitle
    {
        get => _editDialogTitle;
        set => SetProperty(ref _editDialogTitle, value);
    }

    // 命令
    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SwitchToUserCommand { get; }
    public ICommand SwitchToSystemCommand { get; }
    public ICommand AddPathItemCommand { get; }
    public ICommand RemovePathItemCommand { get; }
    public ICommand MovePathItemUpCommand { get; }
    public ICommand MovePathItemDownCommand { get; }

    /// <summary>
    /// 路径项集合
    /// </summary>
    public ObservableCollection<PathItem> PathItems
    {
        get => _pathItems;
        set => SetProperty(ref _pathItems, value);
    }

    /// <summary>
    /// 选中的路径项
    /// </summary>
    public PathItem? SelectedPathItem
    {
        get => _selectedPathItem;
        set
        {
            if (SetProperty(ref _selectedPathItem, value))
            {
                ((RelayCommand)RemovePathItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)MovePathItemUpCommand).RaiseCanExecuteChanged();
                ((RelayCommand)MovePathItemDownCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// 是否为多值模式（包含分号）
    /// </summary>
    private bool _isMultiValue;
    public bool IsMultiValue
    {
        get => _isMultiValue;
        set => SetProperty(ref _isMultiValue, value);
    }

    /// <summary>
    /// 是否为单值模式
    /// </summary>
    public bool IsSingleValue => !_isMultiValue;

    /// <summary>
    /// 是否为系统变量模式
    /// </summary>
    public bool IsSystemMode
    {
        get => _isSystemMode;
        set
        {
            if (SetProperty(ref _isSystemMode, value))
            {
                _service.IsSystemMode = value;
                OnPropertyChanged(nameof(IsUserMode));
                OnPropertyChanged(nameof(ModeText));
                Refresh();
            }
        }
    }

    /// <summary>
    /// 是否为用户变量模式
    /// </summary>
    public bool IsUserMode => !_isSystemMode;

    /// <summary>
    /// 是否以管理员身份运行
    /// </summary>
    public bool IsAdmin => _isAdmin;

    /// <summary>
    /// 模式文本
    /// </summary>
    public string ModeText => _isSystemMode ? "系统变量" : "用户变量";

    /// <summary>
    /// 切换到用户变量
    /// </summary>
    private void SwitchToUser()
    {
        IsSystemMode = false;
    }

    /// <summary>
    /// 切换到系统变量
    /// </summary>
    private void SwitchToSystem()
    {
        IsSystemMode = true;
    }

    /// <summary>
    /// 刷新环境变量列表
    /// </summary>
    private void Refresh()
    {
        Variables.Clear();
        foreach (var variable in _service.GetAll())
        {
            Variables.Add(variable);
        }
        ApplyFilter();
    }

    /// <summary>
    /// 应用过滤器
    /// </summary>
    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Variables
            : new ObservableCollection<EnvironmentVariable>(
                Variables.Where(v => 
                    v.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        
        FilteredVariables = filtered;
    }

    /// <summary>
    /// 添加新环境变量
    /// </summary>
    private void Add()
    {
        EditingVariable = new EnvironmentVariable { IsNew = true };
        EditDialogTitle = "添加环境变量";
        IsMultiValue = false;
        PathItems = new ObservableCollection<PathItem>();
        OnPropertyChanged(nameof(IsSingleValue));
        IsEditing = true;
    }

    /// <summary>
    /// 编辑选中的环境变量
    /// </summary>
    private void Edit()
    {
        if (SelectedVariable == null) return;

        EditingVariable = new EnvironmentVariable
        {
            Name = SelectedVariable.Name,
            Value = SelectedVariable.Value,
            OriginalName = SelectedVariable.Name,
            IsNew = false
        };
        EditDialogTitle = "编辑环境变量";
        
        // 检查是否包含分号，决定使用哪种编辑模式
        IsMultiValue = SelectedVariable.Value.Contains(';');
        OnPropertyChanged(nameof(IsSingleValue));
        
        if (IsMultiValue)
        {
            // 拆分为路径列表
            var paths = SelectedVariable.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            PathItems = new ObservableCollection<PathItem>(paths.Select(p => new PathItem(p.Trim())));
        }
        else
        {
            PathItems = new ObservableCollection<PathItem>();
        }
        
        IsEditing = true;
    }

    /// <summary>
    /// 删除选中的环境变量
    /// </summary>
    private void Delete()
    {
        if (SelectedVariable == null) return;

        var result = MessageBox.Show(
            $"确定要删除环境变量 \"{SelectedVariable.Name}\" 吗？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _service.Delete(SelectedVariable.Name);
                Refresh();
                MessageBox.Show("环境变量已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 保存编辑
    /// </summary>
    public void Save()
    {
        if (EditingVariable == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(EditingVariable.Name))
            {
                MessageBox.Show("环境变量名称不能为空", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 如果是多值模式，合并路径列表
            string valueToSave;
            if (IsMultiValue)
            {
                valueToSave = string.Join(";", PathItems.Select(p => p.Path).Where(p => !string.IsNullOrWhiteSpace(p)));
            }
            else
            {
                valueToSave = EditingVariable.Value;
            }

            if (EditingVariable.IsNew)
            {
                _service.Add(EditingVariable.Name, valueToSave);
            }
            else
            {
                _service.Update(EditingVariable.OriginalName, EditingVariable.Name, valueToSave);
            }

            IsEditing = false;
            Refresh();
            MessageBox.Show("保存成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    public void CancelEdit()
    {
        IsEditing = false;
        EditingVariable = null;
    }

    /// <summary>
    /// 添加路径项
    /// </summary>
    private void AddPathItem()
    {
        PathItems.Add(new PathItem());
    }

    /// <summary>
    /// 删除路径项
    /// </summary>
    private void RemovePathItem()
    {
        if (SelectedPathItem != null)
        {
            PathItems.Remove(SelectedPathItem);
        }
    }

    /// <summary>
    /// 上移路径项
    /// </summary>
    private void MovePathItemUp()
    {
        if (SelectedPathItem == null) return;
        
        int index = PathItems.IndexOf(SelectedPathItem);
        if (index > 0)
        {
            PathItems.Move(index, index - 1);
        }
    }

    /// <summary>
    /// 下移路径项
    /// </summary>
    private void MovePathItemDown()
    {
        if (SelectedPathItem == null) return;
        
        int index = PathItems.IndexOf(SelectedPathItem);
        if (index < PathItems.Count - 1)
        {
            PathItems.Move(index, index + 1);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IEnvironmentVariableService _service;
    private EnvironmentVariable? _selectedVariable;
    private string _searchText = string.Empty;
    private ObservableCollection<EnvironmentVariable> _filteredVariables = new();

    public MainViewModel()
    {
        _service = new EnvironmentVariableService();
        Variables = new ObservableCollection<EnvironmentVariable>();
        
        // 初始化命令
        RefreshCommand = new RelayCommand(Refresh);
        AddCommand = new RelayCommand(Add);
        EditCommand = new RelayCommand(Edit, () => SelectedVariable != null);
        DeleteCommand = new RelayCommand(Delete, () => SelectedVariable != null);
        SaveCommand = new RelayCommand(Save);

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

            if (EditingVariable.IsNew)
            {
                _service.Add(EditingVariable.Name, EditingVariable.Value);
            }
            else
            {
                _service.Update(EditingVariable.OriginalName, EditingVariable.Name, EditingVariable.Value);
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
}

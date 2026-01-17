using System.Windows;
using System.Windows.Input;
using WinEnv.ViewModels;

namespace WinEnv;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 标题栏拖动
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_Click(sender, e);
        }
        else
        {
            DragMove();
        }
    }

    /// <summary>
    /// 最小化按钮
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 最大化/还原按钮
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeButton.Content = "☐";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeButton.Content = "❐";
        }
    }

    /// <summary>
    /// 关闭按钮
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 双击编辑
    /// </summary>
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.SelectedVariable != null)
        {
            vm.EditCommand.Execute(null);
        }
    }

    /// <summary>
    /// 点击遮罩取消编辑
    /// </summary>
    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.CancelEdit();
        }
    }

    /// <summary>
    /// 取消按钮
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.CancelEdit();
        }
    }
}

using WinEnv.Models;
using System.Collections.Generic;

namespace WinEnv.Services;

/// <summary>
/// 环境变量服务接口
/// </summary>
public interface IEnvironmentVariableService
{
    /// <summary>
    /// 获取所有用户环境变量
    /// </summary>
    IEnumerable<EnvironmentVariable> GetAll();

    /// <summary>
    /// 添加环境变量
    /// </summary>
    void Add(string name, string value);

    /// <summary>
    /// 更新环境变量
    /// </summary>
    void Update(string originalName, string newName, string value);

    /// <summary>
    /// 删除环境变量
    /// </summary>
    void Delete(string name);

    /// <summary>
    /// 检查环境变量是否存在
    /// </summary>
    bool Exists(string name);
}

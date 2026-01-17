using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WinEnv.Models;

namespace WinEnv.Services;

/// <summary>
/// 环境变量服务实现
/// </summary>
public class EnvironmentVariableService : IEnvironmentVariableService
{
    private const EnvironmentVariableTarget Target = EnvironmentVariableTarget.User;

    /// <summary>
    /// 获取所有用户环境变量
    /// </summary>
    public IEnumerable<EnvironmentVariable> GetAll()
    {
        var variables = Environment.GetEnvironmentVariables(Target);
        var result = new List<EnvironmentVariable>();

        foreach (DictionaryEntry entry in variables)
        {
            result.Add(new EnvironmentVariable
            {
                Name = entry.Key?.ToString() ?? string.Empty,
                Value = entry.Value?.ToString() ?? string.Empty,
                OriginalName = entry.Key?.ToString() ?? string.Empty,
                IsNew = false
            });
        }

        return result.OrderBy(v => v.Name);
    }

    /// <summary>
    /// 添加环境变量
    /// </summary>
    public void Add(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("环境变量名称不能为空", nameof(name));

        if (Exists(name))
            throw new InvalidOperationException($"环境变量 '{name}' 已存在");

        Environment.SetEnvironmentVariable(name, value, Target);
    }

    /// <summary>
    /// 更新环境变量
    /// </summary>
    public void Update(string originalName, string newName, string value)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("环境变量名称不能为空", nameof(newName));

        // 如果名称改变，需要删除旧的并创建新的
        if (!string.Equals(originalName, newName, StringComparison.OrdinalIgnoreCase))
        {
            if (Exists(newName))
                throw new InvalidOperationException($"环境变量 '{newName}' 已存在");

            Delete(originalName);
        }

        Environment.SetEnvironmentVariable(newName, value, Target);
    }

    /// <summary>
    /// 删除环境变量
    /// </summary>
    public void Delete(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("环境变量名称不能为空", nameof(name));

        Environment.SetEnvironmentVariable(name, null, Target);
    }

    /// <summary>
    /// 检查环境变量是否存在
    /// </summary>
    public bool Exists(string name)
    {
        return Environment.GetEnvironmentVariable(name, Target) != null;
    }
}

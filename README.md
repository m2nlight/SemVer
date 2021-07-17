# SemVer

[语义化版本](https://semver.org) 2.0.0 的一个实现

## 代码

- `SemVer/SemanticVersion.cs`：主要的代码逻辑
- `SemVer/SemanticVersionFormat.cs`：仅为了支持 `ToString("N")` 去显示标准版本号

## 格式

下面格式都是合法的

```txt
1.0.0
1.0.0-alpha
1.0.0-alpha.1
1.0.0+exp.sha.5114f85
1.0.0-alpha.1+exp.sha.5114f85
1.2.3-alpha.1-1.0+exp.sha.5114f85-001
```

对 `1.2.3-alpha.1-1.0+exp.sha.5114f85-001` 各部分的解释

- 主版本号（major）：`1`
- 次版本号（minor）：`2`
- 修订号（patch）: `3`
- 先行版本号（pre-release）：`alpha.1-1.0`
- 版本编译信息（build）：`exp.sha.5114f85-001`

其中 `1.2.3` 是标准版本号（normal version），而且是必须的。

使用 `ToString("N")` 可获得标准版本号。

## 比较和HashCode

1. 仅对 `major`、`minor`、`patch` 和 `pre-release` 依次比较。
2. 数值按数值大小比较
3. 在 `pre-release` 比较时：
    - 先按英文句点分隔为多个标识符
    - 将标识符按顺序比较
    - 数值按数值大小比较
    - 非数值按 `ASCII` 排序比较。*注：虽然规范没有写，这里会忽略大小写*
    - 数值总比非数值要小
    - 标识符都相等，但是还有更多标识符的版本高

相等的语义化版本的 `GetHashCode()` 将会相同。

## 单元测试和代码覆盖率

当前是完整的代码覆盖，并全部测试通过。

```sh
dotnet test -p:CollectCoverage=true -p:Exclude="*.Tests/*" -p:CoverletOutput="../" -p:MergeWith="../coverage.json" -p:CoverletOutputFormat=\"lcov,json\" -m:1
```

VSCode 支持

安装扩展：

- C#
- .NET Core Test Explorer
- Coverlet
- Coverage Gutters

添加下面到 `.vscode/tasks.json`

```json
{
    "label": "test with coverage",
    "command": "dotnet",
    "type": "process",
    "args": [
        "test",
        "-p:CollectCoverage=true",
        "-p:Exclude=\"*.Tests/*\"",
        "-p:CoverletOutput=\"../\"",
        "-p:MergeWith=\"../coverage.json\"",
        "-p:CoverletOutputFormat=\"lcov,json\"",
        "-m:1"
    ],
    "problemMatcher": "$msCompile",
    "group": {
        "kind": "test",
        "isDefault": true
    }
}
```

添加设置到 `vscode/settings.json`

```json
"coverage-gutters.lcovname": "coverage.info"
```

打开 `Coverage Gutters` 的监视功能，运行 `test with coverage` 任务，代码文件行号位置会出现覆盖率色块。

#
# @Author       : Chr_
# @Date         : 2024-01-18 14:25:15
# @LastEditors  : Chr_
# @LastEditTime : 2024-01-18 20:15:18
# @Description  : 生成依赖服务信息
#

# 工作目录
$workDir = (Get-Location).Path;
$outputPath = Join-Path -Path (Join-Path -Path $workDir -ChildPath "XinjingdailyBot.WebAPI") -ChildPath "Properties";

# 包名列表
$libNames = 
"XinjingdailyBot.Model",
"XinjingdailyBot.Repository",
"XinjingdailyBot.Interface",
"XinjingdailyBot.Service",
"XinjingdailyBot.Tasks",
"XinjingdailyBot.Command";

# 输出路径
$appServicePath = Join-Path -Path $outputPath -ChildPath "appService.json";
$taskPath = Join-Path -Path $outputPath -ChildPath "schedule.json";
$tablePath = Join-Path -Path $outputPath -ChildPath "dbtable.json";

# 扫描源文件列表
function GetSourceFileList {
    param (
        [string]$Path
    )

    $result = @();

    # 去除 bin obj 目录
    $dirs = Get-ChildItem -Path $Path -Directory -Exclude "bin", "obj"

    # 遍历所有 .cs 文件
    foreach ($dir in $dirs) {
        $files = Get-ChildItem -Path $dir -Filter "*.cs" -File -Recurse

        # 遍历子目录文件
        foreach ($file in $files) {
            $result += $file
        }
    }

    $files = Get-ChildItem -Path $Path -Filter "*.cs" -File

    # 遍历所有 .cs 文件
    foreach ($file in $files) {
        $result += $file
    }

    Write-Output $result

    $result;
}

# 读取命名空间
function GetNamespace {
    param (
        [string] $Content
    )

    if ($Content -match "namespace\s+([^;{]+)") {
        $Matches[1];
    }
}

# 读取类名/接口名
function GetClassName {
    param (
        [string] $Content
    )

    if ($Content -match "(?:class|interface|record)\s+([^({:\s]+)") {
        $Matches[1];
    }
}

# 扫描AppServiceAttribute
function ScanAppServiceAttribute {
    param (
        [string] $Content
    )

    if ($Content -match "\[AppService(?:Attribute)?\(([^\]]+)\)\]") {
        $result = @{}

        $arguments = $Matches[1].Split(",");
        foreach ($argument in $arguments) {
            if ($argument -match "LifeTime\.(Singleton|Transient|Scoped)") {
                $result["LifeTime"] = $Matches[1];
            }
            elseif ($argument -match "typeof\(([^()]+)\)") {
                $result["Interface"] = $Matches[1];
            }
            elseif ($argument -match "true|false") {
                $result["AutoInterface"] = $Matches[1];
            }
            else {
                Write-Output $argument;
            }
        }

        $result;
    }
}

# 扫描JobAttribute
function ScanScheduleAttribute {
    param (
        [string] $Content
    )

    if ($Content -match "\[Schedule(?:Attribute)?\(""([^\]]+)""\)\]") {
        $Matches[1];
    }
}

function ScanSugarTableAttribute {
    param (
        [string] $Content
    )

    $Content -match "\[SugarTable(?:Attribute)?\(([^\]]+)\)\]";
}

# 扫描QueryCmdAttribute ToDo
# function ScanQueryCmdAttribute {
#     param (
#         [string] $Content
#     )

#     if ($Content -match "\[QueryCmd(?:Attribute)?\(([^\]]+)\)\]") {
#         $result = @{}

#         $arguments = $Matches[1].Split(",");
#         foreach ($argument in $arguments) {
#             if ($argument -match "EUserRights\.\S+") {
#                 $result["LifeTime"] = $Matches[0];
#             }
#             elseif ($argument -match "typeof\(([^()]+)\)") {
#                 $result["Interface"] = $Matches[1];
#             }
#             elseif ($argument -match "true|false") {
#                 $result["AutoInterface"] = $Matches[1];
#             }
#             else {
#                 Write-Output $argument;
#             }
#         }

#         $result;
#     }
# }

# 类全名表
$clsNamespaces = @{};
$appServiceAttributes = @{};
$scheduleAttributes = @{};
$sugarTableAttributes = @{};

foreach ($libName in $libNames) {
    # 库文件路径
    $libPath = Join-Path -Path $workDir -ChildPath $libName;
    # 获取所有源文件路径
    $filePaths = GetSourceFileList -Path $libPath;

    # 读取Service信息和类命名空间;
    foreach ($filePath in $filePaths) {
        $fileContent = Get-Content -Path $filePath.FullName -Raw;

        $namespace = GetNameSpace -Content $fileContent;
        if ($null -eq $namespace -or $namespace -eq "" ) {
            continue;
        }

        $clsName = GetClassName -Content $fileContent;
        if ($null -eq $namespace -or $namespace -eq "" ) {
            continue;
        }
        
        $clsNamespaces[$clsName] = $namespace;

        $appService = ScanAppServiceAttribute -Content $fileContent;
        if ($null -ne $appService) {
            $appServiceAttributes[$clsName] = $appService;
        }

        $task = ScanScheduleAttribute -Content $fileContent;
        if ($null -ne $task) {
            $scheduleAttributes[$clsName] = $task;
        }

        if (ScanSugarTableAttribute -Content $fileContent) {
            $sugarTableAttributes[$clsName] = $null;
        }
    }
}

$services = $appServiceAttributes.Clone();
# 生成服务信息
foreach ($service in $services.Keys) {
    $serviceInfo = $appServiceAttributes[$service];
    $namespace = $clsNamespaces[$service];

    $serviceInfo["Class"] = "$namespace.$service";

    $interface = $serviceInfo["Interface"];
    if ($null -ne $interface) {
        $ns = $clsNamespaces[$interface];
        $serviceInfo["Interface"] = "$ns.$interface";
    }
    
    $appServiceAttributes[$service] = $serviceInfo;
}

$tasks = $scheduleAttributes.Clone();
# 生成服务信息
foreach ($task in $tasks.Keys) {
    $schedule = $scheduleAttributes[$task];
    $namespace = $clsNamespaces[$task];

    $clsName = "$namespace.$task";

    $scheduleAttributes[$task] = @{
        "Class"    = $clsName 
        "Schedule" = $schedule
    };
}

$tables = $sugarTableAttributes.Clone();
# 生成数据表信息
foreach($table in $tables.Keys){
    $namespace = $clsNamespaces[$table];
    $clsName = "$namespace.$table";
    $sugarTableAttributes[$table] = $clsName
}

Write-Output "扫描到 $($appServiceAttributes.Count) 个服务";
$appServiceAttributes | ConvertTo-Json | Out-File -FilePath $appServicePath
Write-Output "文件保存至 $appServicePath"

Write-Output "扫描到 $($scheduleAttributes.Count) 个定时任务";
$scheduleAttributes | ConvertTo-Json | Out-File -FilePath $taskPath
Write-Output "文件保存至 $taskPath"

Write-Output "扫描到 $($sugarTableAttributes.Count) 个数据表";
$sugarTableAttributes | ConvertTo-Json | Out-File -FilePath $tablePath
Write-Output "文件保存至 $tablePath"

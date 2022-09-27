# 何をしたいのか
# アルゴリズムによる性能の変化を、一定の要素数ごとに調べたい。
$OutputEncoding = [System.Text.Encoding]::GetEncoding('utf-8') # with BOM
$capSrc = './BTree.cs'
$capDecReg = 'int CAPACITY = '
$testSrc = './Program.cs'
$testNReg = 'var N = '
$Algos = @('BTree', 'BTree', 'Pancake', 'Swap')
$times = 0..4
$resultPool = './Comparison1.csv'
if (Test-Path $resultPool) {
    Remove-Item $resultPool 
}
Set-Content $resultPool -Encoding UTF8 -Value "N, algorithm, operation time[ms]"
# ここ改善したい
# CAP=4, CAP=32, pancake, roll
# remark: オプション -NoNewline により、Set-Contentの改行を抑制できる
for ($j = 6; $j -le 14; $j++) {
    $N = [math]::Floor([math]::pow(10, $j / 2.0))
    $times[0] = $N
    (Get-Content $testSrc -Encoding UTF8) -replace "${testNReg}.*", "${testNReg}${N};" `
    | Set-Content $testSrc -Encoding UTF8
    for ($i = 0; $i -le 3; $i++) {
        $algoRepStr = $Algos[$i]
        if ($i -le 1) {
            $rank = [math]::pow(2, 4 * $i + 2)
            $algoRepStr = $algoRepStr + $rank
            (Get-Content $capSrc -Encoding UTF8) -replace "${capDecReg}.*", "${capDecReg}${rank};" `
            | Set-Content $capSrc -Encoding UTF8
        }
        (Get-Content $testSrc -Encoding UTF8) -replace "Benchmark\w*", "Benchmark$($Algos[$i])" `
        | Set-Content $testSrc -Encoding UTF8
        $times[$i+1] = dotnet run -c Release
    }
    $times -join "," | Out-File $resultPool -Encoding UTF8 -Append
}
(Get-Content $capSrc -Encoding UTF8) -replace "${capDecReg}.*", "${capDecReg}128;" `
| Set-Content $capSrc -Encoding UTF8
(Get-Content $testSrc -Encoding UTF8) -replace "${testNReg}.*", "${testNReg}100000;" `
| Set-Content $testSrc -Encoding UTF8

# 処理が終わったらスリープする
# Add-Type -Assembly System.Windows.Forms; [System.Windows.Forms.Application]::SetSuspendState('Suspend', $false, $false);
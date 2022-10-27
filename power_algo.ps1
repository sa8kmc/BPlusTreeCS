# 何をしたいのか
# アルゴリズムによる性能の変化を、一定の要素数ごとに調べたい。
$OutputEncoding = [System.Text.Encoding]::GetEncoding('utf-8') # with BOM
$capSrc = './BTree.cs'
$capDecReg = 'int CAPACITY = '
$testSrc = './Program.cs'
$testNReg = 'var N = '
$Algos = @('RBT', 'BTree', 'BTree', 'Pancake', 'Swap')
$times = 0..5
$resultPool = './Comparison1.csv'
if (Test-Path $resultPool) {
    Remove-Item $resultPool 
}
Set-Content $resultPool -Encoding UTF8 -Value "N, algorithm, operation time[s]"
for ($j = 6; $j -le 15; $j++) {
    $N = [math]::Floor([math]::pow(10, $j / 2.0))
    $times[0] = $N
    (Get-Content $testSrc -Encoding UTF8) -replace "${testNReg}.*", "${testNReg}${N};" `
    | Set-Content $testSrc -Encoding UTF8
    for ($i = 0; $i -le 4; $i++) {
        $algoRepStr = $Algos[$i]
        if ($i -ge 1 -and $i -le 2) {
            # B-Tree
            $rank = [math]::pow(2, 4 * ($i - 1) + 2)
            $algoRepStr = $algoRepStr + $rank
            (Get-Content $capSrc -Encoding UTF8) -replace "${capDecReg}.*", "${capDecReg}${rank};" `
            | Set-Content $capSrc -Encoding UTF8
        }
        (Get-Content $testSrc -Encoding UTF8) -replace "Benchmark\w*", "Benchmark$($Algos[$i])" `
        | Set-Content $testSrc -Encoding UTF8
        # compile and run
        $times[$i + 1] = dotnet run -c Release
    }
    $times -join "," | Out-File $resultPool -Encoding UTF8 -Append
}
(Get-Content $capSrc -Encoding UTF8) -replace "${capDecReg}.*", "${capDecReg}32;" `
| Set-Content $capSrc -Encoding UTF8
(Get-Content $testSrc -Encoding UTF8) -replace "${testNReg}.*", "${testNReg}100000;" `
| Set-Content $testSrc -Encoding UTF8
(Get-Content $testSrc -Encoding UTF8) -replace "Benchmark\w*", "BenchmarkBTree" `
| Set-Content $testSrc -Encoding UTF8

# 処理が終わったらスリープする
# Add-Type -Assembly System.Windows.Forms; [System.Windows.Forms.Application]::SetSuspendState('Suspend', $false, $false);
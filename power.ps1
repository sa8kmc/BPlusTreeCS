# 何をしたいのか
# 木の階数パラメータによる性能の変化を、一定の要素数ごとに調べたい。
# 
$OutputEncoding = [System.Text.Encoding]::GetEncoding('utf-8') # with BOM
$rankSrc = './BTree.cs'
$DeclareRank = 'int CAPACITY = '
$caseSrc = './Program.cs'
$DeclareCase = 'var N = '
$resultPool = './Comparison2.csv'
if (Test-Path $resultPool) {
    Remove-Item $resultPool 
}
Set-Content $resultPool -Encoding UTF8 -Value ("N,capacity,operation time[sec]")
$testSrc = './Program.cs'
(Get-Content $testSrc -Encoding UTF8) -replace "Benchmark\w*", "BenchmarkBTree" `
| Set-Content $testSrc -Encoding UTF8
for ($j = 6; $j -le 14; $j++) {
    for ($i = 2; $i -le 10; $i++) {
        $rank = [math]::pow(2, $i)
        $N = [math]::Floor([math]::pow(10, $j / 2.0))
        (Get-Content $rankSrc -Encoding UTF8) -replace "${DeclareRank}.*", "${DeclareRank}${rank};" `
        | Set-Content $rankSrc -Encoding UTF8
        (Get-Content $caseSrc -Encoding UTF8) -replace "${DeclareCase}.*", "${DeclareCase}${N};" `
        | Set-Content $caseSrc -Encoding UTF8
        $time = dotnet run -c Release
        @($N, $rank, $time) -join ',' | Out-File $resultPool -Encoding UTF8 -Append
    }
}
(Get-Content $rankSrc -Encoding UTF8) -replace "${DeclareRank}.*", "${DeclareRank}128;" `
| Set-Content $rankSrc -Encoding UTF8
(Get-Content $caseSrc -Encoding UTF8) -replace "${DeclareCase}.*", "${DeclareCase}100000;" `
| Set-Content $caseSrc -Encoding UTF8

# 処理が終わったらスリープする
# Add-Type -Assembly System.Windows.Forms; [System.Windows.Forms.Application]::SetSuspendState('Suspend', $false, $false);
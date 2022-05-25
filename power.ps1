# 何をしたいのか
# 木の階数パラメータによる性能の変化を、一定の要素数ごとに調べたい。
# 
$rankSrc = './BTree.cs'
$DeclareRank = 'public static readonly int CHILD_CAPACITY = '
$caseSrc = './Program.cs'
$DeclareCase = 'var N = '
$resultPool = './Comparison.dat'
if (Test-Path $resultPool) {
    Remove-Item $resultPool 
}
for ($j = 8; $j -le 13; $j++) {
    for ($i = 2; $i -le 10; $i++) {
        $rank = [math]::pow(2, $i)
        $case = [math]::Floor([math]::pow(10, $j / 2.0))
        (Get-Content $rankSrc -Encoding UTF8) -replace "${DeclareRank}.*", "${DeclareRank}${rank};" `
        | Set-Content $rankSrc -Encoding UTF8
        (Get-Content $caseSrc -Encoding UTF8) -replace "${DeclareCase}.*", "${DeclareCase}${case};" `
        | Set-Content $caseSrc -Encoding UTF8
        dotnet run >> $resultPool
    }
}
(Get-Content $rankSrc -Encoding UTF8) -replace "${DeclareRank}.*", "${DeclareRank}128;" `
| Set-Content $rankSrc -Encoding UTF8
(Get-Content $caseSrc -Encoding UTF8) -replace "${DeclareCase}.*", "${DeclareCase}100000;" `
| Set-Content $caseSrc -Encoding UTF8

# 処理が終わったらスリープする
Add-Type -Assembly System.Windows.Forms; [System.Windows.Forms.Application]::SetSuspendState('Suspend', $false, $false);
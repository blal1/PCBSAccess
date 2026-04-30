$files = Get-ChildItem -Path PCBSAccess -Filter *.cs -Recurse | Where-Object { $_.FullName -notmatch '\\obj\\' }
foreach ($f in $files) {
    $md = "llm-scratchpad/code-index/$($f.Name).md"
    $content = @("# Code Index for $($f.Name)", "")
    $matches = Select-String -Path $f.FullName -Pattern "((?:public|private|protected|internal|static|sealed|abstract|partial|\s)*(class|struct|interface|enum)\s+\w+)|(^\s*(?:public|private|protected|internal|static)\s+(?!class|struct|interface|enum)[^\=\;\{]+?\()|(^\s*///)" | Select-Object LineNumber, Line
    foreach ($m in $matches) {
        $content += "- Line $($m.LineNumber): $($m.Line.Trim())"
    }
    $content | Set-Content $md -Encoding UTF8
}

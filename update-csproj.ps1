$csproj = "C:\Git\Nestify\Nestify\Nestify.csproj"
$content = Get-Content $csproj -Raw

$oldBlock = @"
    <Compile Include="Commands\AutoNestCommand.cs" />
    <Compile Include="Commands\NestFilesCommand.cs" />
    <Compile Include="Commands\UnnestFilesCommand.cs" />
    <Compile Include="Dialogs\ParentFilePickerDialog.cs" />
    <Compile Include="FileNestingHelper.cs" />
    <Compile Include="NestifyPackage.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
    <Compile Include="Services\AutoNestRuleEngine.cs" />
    <Compile Include="Services\FileNestingService.cs" />
"@

$newBlock = @"
    <Compile Include="Abstractions\IAutoNestRuleEngine.cs" />
    <Compile Include="Abstractions\IDialogService.cs" />
    <Compile Include="Abstractions\IDirectoryScanner.cs" />
    <Compile Include="Abstractions\IFileNestingService.cs" />
    <Compile Include="Abstractions\IFileValidator.cs" />
    <Compile Include="Abstractions\INestingRule.cs" />
    <Compile Include="Abstractions\ISiblingFileProvider.cs" />
    <Compile Include="Commands\AutoNestCommand.cs" />
    <Compile Include="Commands\NestFilesCommand.cs" />
    <Compile Include="Commands\UnnestFilesCommand.cs" />
    <Compile Include="Dialogs\ParentFilePickerDialog.cs" />
    <Compile Include="NestifyPackage.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
    <Compile Include="Rules\CSharpInterfaceNestingRule.cs" />
    <Compile Include="Rules\JavaScriptBundleMinNestingRule.cs" />
    <Compile Include="Rules\JavaScriptBundleNestingRule.cs" />
    <Compile Include="Rules\JavaScriptMinNestingRule.cs" />
    <Compile Include="Services\AutoNestRuleEngine.cs" />
    <Compile Include="Services\DialogService.cs" />
    <Compile Include="Services\DirectoryScanner.cs" />
    <Compile Include="Services\FileNestingService.cs" />
    <Compile Include="Services\FileValidator.cs" />
    <Compile Include="Services\SiblingFileProvider.cs" />
    <Compile Include="Utilities\PathUtilities.cs" />
"@

$content = $content.Replace($oldBlock, $newBlock)
Set-Content $csproj -Value $content -NoNewline
Write-Host "Done"

param (
    [string]$inputDirectory,
    [string]$outputDirectory
)

try
{
    if (-not($inputDirectory -and $outputDirectory))
    {
        throw "Invalid input"
    }

    $workloadManifest = "WorkloadManifest.xml"
    $workloadXmlPath = "$($inputDirectory)$($workloadManifest)"
    $workloadXml = [xml](Get-Content -Path $workloadXmlPath)
    $inputXsd = "WorkloadDefinition.xsd"

    $manifestValidatorPath = Join-Path $PSScriptRoot "ManifestValidator.ps1"
    . $manifestValidatorPath -inputDirectory $inputDirectory -inputXml $workloadManifest -inputXsd $inputXsd -outputDirectory $outputDirectory

    $workloadName = $workloadXml.WorkloadManifestConfiguration.Workload.WorkloadName
    $aadApp = $workloadXml.SelectSingleNode("//AADApp")

    if ((-not ($aadApp.ResourceId -clike "*$($workloadName)")) -and (-not ($aadApp.ResourceId -clike "*$($workloadName)/*")))
    {
        $scriptPath = Join-Path $PSScriptRoot "WriteErrorsToFile.ps1"
        . $scriptPath -errors "AADApp.resourceId: $($aadApp.ResourceId), should contain the exact WorkloadName: $($workloadName)" -outputDirectory $outputDirectory
    }
}
catch
{
    Write-Host "An error occurred:"
    Write-Host $_
}
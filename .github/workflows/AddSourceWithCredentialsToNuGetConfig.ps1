[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][string]$ConfigFile,
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Username,
    [Parameter(Mandatory = $true)][string]$Password
)
$doc = New-Object System.Xml.XmlDocument
$filename = (Get-Item $ConfigFile).FullName
$doc.Load($filename)

$packageSources = $doc.DocumentElement.SelectSingleNode("packageSources")
if ($packageSources -eq $null)
{
    $packageSources = $doc.CreateElement("packageSources")
    $doc.DocumentElement.AppendChild($packageSources) | Out-Null
}

$packageSource = $packageSources.SelectSingleNode("add[@key='$Source']")
if ($packageSource -eq $null)
{
    $packageSource = $doc.CreateElement("add")
    $packageSource.SetAttribute("key", $Source)
    $packageSources.AppendChild($packageSource) | Out-Null
}
$packageSource.SetAttribute("value", "https://nuget.pkg.github.com/$Username/index.json")

$creds = $doc.DocumentElement.SelectSingleNode("packageSourceCredentials")
if ($creds -eq $null)
{
    $creds = $doc.CreateElement("packageSourceCredentials")
    $doc.DocumentElement.AppendChild($creds) | Out-Null
}

$sourceElement = $creds.SelectSingleNode($Source)
if ($sourceElement -eq $null)
{
    $sourceElement = $doc.CreateElement($Source)
    $creds.AppendChild($sourceElement) | Out-Null
}

$usernameElement = $sourceElement.SelectSingleNode("add[@key='Username']")
if ($usernameElement -eq $null)
{
    $usernameElement = $doc.CreateElement("add")
    $usernameElement.SetAttribute("key", "Username")
    $sourceElement.AppendChild($usernameElement) | Out-Null
}
$usernameElement.SetAttribute("value", $Username)

$passwordElement = $sourceElement.SelectSingleNode("add[@key='ClearTextPassword']")
if ($passwordElement -eq $null)
{
    $passwordElement = $doc.CreateElement("add")
    $passwordElement.SetAttribute("key", "ClearTextPassword")
    $sourceElement.AppendChild($passwordElement) | Out-Null
}
$passwordElement.SetAttribute("value", $Password)

$doc.Save($filename)

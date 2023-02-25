param (
    [switch]$WhatIf = $false
)
$certsInMy = Get-ChildItem Cert:\LocalMachine\My
$certsInMy | Where-Object {$_.Issuer -match "Taito Arcade Machine CA"} | Remove-Item -WhatIf:$WhatIf

$certsInRoot = Get-ChildItem Cert:\LocalMachine\Root
$certsInRoot | Where-Object {$_.Issuer -match "Taito Arcade Machine CA"} | Remove-Item -WhatIf:$WhatIf

Get-ChildItem -Path root.pfx | Import-PfxCertificate -CertStoreLocation Cert:\LocalMachine\My -WhatIf:$WhatIf
Get-ChildItem -Path cert.pfx | Import-PfxCertificate -CertStoreLocation Cert:\LocalMachine\My -WhatIf:$WhatIf

Get-ChildItem -Path root.pfx | Import-PfxCertificate -CertStoreLocation Cert:\LocalMachine\Root -WhatIf:$WhatIf
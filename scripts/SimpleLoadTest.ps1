[CmdletBinding()]
Param(
    [Int32]$iterations = 1
)

# NB: this is very hacky, and won't work across different machines due to
#     differing GUIDs. It would be a good idea to write a proper PS Module
#     around the email API that can take configurable GUIDs, and use the API
#     to look up templates by name, etc.
$endpoint = "http://localhost:5000/api/v1/FECFD324-F99A-461A-AD12-A07F68C81117/messages"
$params = @{
    "To[0]"="kwilliams@sunbrandingsolutions.com"
    "To[1]"="kevans@sunbrandingsolutions.com";
    "To[2]"="jkay@sunbrandingsolutions.com";
    Template="0fc258cb-076d-4743-6af9-08d3d1aeb9e1";
    Data="{`"Name`":`"Keith`"}";
}

For ($i = 1; $i -le $iterations; $i++) {
    Write-Host "Sending request $i"
    Invoke-WebRequest $endpoint -Body $params -Method POST | Out-Null 
}

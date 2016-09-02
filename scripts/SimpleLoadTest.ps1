$endpoint = "http://localhost:5000/api/v1/FECFD324-F99A-461A-AD12-A07F68C81117/messages"
$params = @{
    To="kwilliams@sunbrandingsolutions.com";
    Template="0fc258cb-076d-4743-6af9-08d3d1aeb9e1";
    Data="{`"Name`":`"Keith`"}";
}

$iterations = 100;

For ($i = 0; $i -lt $iterations; $i++) {
    Write-Host "Sending request $i"
    Invoke-WebRequest $endpoint -Body $params -Method POST | Out-Null 
}

$baseUrl = "https://localhost:5001"

function Test-Endpoint {
    param($method, $path, $body)
    $url = "$baseUrl$path"
    Write-Host "Testing $method $url..." -ForegroundColor Cyan
    try {
        if ($method -eq "POST") {
            $response = Invoke-RestMethod -Uri $url -Method Post -Body ($body | ConvertTo-Json) -ContentType "application/json"
        }
        else {
            $response = Invoke-RestMethod -Uri $url -Method Get
        }
        Write-Host "Success!" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "Failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $details = $reader.ReadToEnd()
            Write-Host "Details: $details" -ForegroundColor Yellow
        }
    }
}

Write-Host "--- API Integration Test ---" -ForegroundColor White

# 1. Register Vendor
$vendorBody = @{
    name         = "Acme Corp"
    taxNumber    = "TR123456"
    contactEmail = "contact@acme.com"
}
$vendor = Test-Endpoint "POST" "/api/vendors" $vendorBody

if ($vendor) {
    # 2. Create Invoice
    $invoiceBody = @{
        vendorId      = $vendor.id
        invoiceNumber = "INV-2026-001"
        amount        = 1500.50
        dueDate       = "2026-02-01"
    }
    Test-Endpoint "POST" "/api/invoices" $invoiceBody
}

# 3. Test Validation (Should Fail)
Write-Host "`nTesting Validation..." -ForegroundColor Magenta
$invalidBody = @{
    vendorId = $vendor.id
    amount   = -100
}
Test-Endpoint "POST" "/api/invoices" $invalidBody

Write-Host "`nTests Completed." -ForegroundColor White

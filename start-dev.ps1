# E-commerce Full Stack Startup Script

Write-Host "Starting E-commerce API and Frontend..." -ForegroundColor Green

# Start API in background
Write-Host "Starting API on https://localhost:7155..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'C:\Users\hamad\Documents\GitHub\EcommerceAPI\EcommerceAPI'; dotnet run --urls 'https://localhost:7155'"

# Wait a moment for API to start
Start-Sleep -Seconds 3

# Start Frontend in background  
Write-Host "Starting Frontend on https://localhost:7045..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'C:\Users\hamad\Documents\GitHub\EcommerceAPI\EcommerceFrontend'; dotnet run --urls 'https://localhost:7045'"

Write-Host "Both services starting..." -ForegroundColor Green
Write-Host "API: https://localhost:7155/swagger" -ForegroundColor Cyan
Write-Host "Frontend: https://localhost:7045" -ForegroundColor Cyan
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

@echo off
echo Starting Loccar Auth API with Docker...
echo.
echo This will start:
echo - PostgreSQL database on port 5433
echo - .NET API on port 5002
echo.
echo Press any key to continue...
pause > nul

echo Building and starting containers...
docker-compose up --build

echo.
echo Services stopped. Press any key to exit...
pause > nul

#!/bin/bash
echo "Starting Zebra IoT Connector API..."
echo "Swagger UI will be available at: http://localhost:5000 or https://localhost:5001"
echo "Press Ctrl+C to stop"
cd ZebraIoTConnector.Backend.API
dotnet run

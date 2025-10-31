#!/bin/bash
echo "Starting Loccar Auth API with Docker..."
echo ""
echo "This will start:"
echo "- PostgreSQL database on port 5433"
echo "- .NET API on port 5002"
echo ""
echo "Building and starting containers..."
docker-compose up --build

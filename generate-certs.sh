#!/bin/bash

# Create certs directory if it doesn't exist
mkdir -p certs

# Generate self-signed certificate for .NET API
echo "Generating self-signed certificate for .NET API..."
openssl req -x509 -newkey rsa:4096 -keyout certs/aspnetapp.key -out certs/aspnetapp.crt \
  -days 365 -nodes -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost,DNS:dnd-api"

# Convert to PFX format (required by .NET)
openssl pkcs12 -export -in certs/aspnetapp.crt -inkey certs/aspnetapp.key \
  -out certs/aspnetapp.pfx -name aspnetapp -passout pass:crypticpassword

echo "✓ Generated aspnetapp.pfx"
echo "Certificate location: certs/aspnetapp.pfx"
echo ""

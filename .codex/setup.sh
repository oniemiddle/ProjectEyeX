#!/usr/bin/env sh
set -e

echo "Installing .NET 8 SDK..."

wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Persist environment variables
echo 'export DOTNET_ROOT=/usr/share/dotnet' >> ~/.profile
echo 'export PATH=$PATH:$DOTNET_ROOT' >> ~/.profile

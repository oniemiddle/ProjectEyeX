#!/usr/bin/env bash
set -e

echo "Installing .NET 8 SDK..."

# 安装微软源
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安装 dotnet 8
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# 验证
dotnet --version

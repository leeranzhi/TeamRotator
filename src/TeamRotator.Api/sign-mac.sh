#!/bin/bash

# 获取证书名称
CERT_NAME=$(security find-identity -v -p codesigning | head -n 1 | cut -d '"' -f 2)

if [ -z "$CERT_NAME" ]; then
    echo "错误：未找到 Mac Developer 证书"
    echo "请先创建一个开发证书"
    exit 1
fi

# 构建项目
dotnet build -c Release

# 获取构建输出路径
BUILD_PATH="./bin/Release/net8.0"

# 对所有动态库和可执行文件进行签名
find "$BUILD_PATH" -type f \( -name "*.dll" -o -name "*.dylib" -o -name "TeamRotator.Api" \) -exec codesign --force --sign "$CERT_NAME" --options runtime {} \;

echo "签名完成" 
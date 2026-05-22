# ============================================================================
# STAGE 1: Build llama.cpp with CUDA Support
# ============================================================================
FROM nvidia/cuda:12.2.0-devel-ubuntu22.04 AS llama-builder

RUN apt-get update && apt-get install -y \
    git \
    build-essential \
    cmake \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /build

RUN git clone https://github.com/ggerganov/llama.cpp.git && \
    cd llama.cpp && \
    cmake -B build -DLLAMA_CUDA=ON && \
    cmake --build build --config Release -j$(nproc) && \
    cp build/bin/llama-server /usr/local/bin/

# ============================================================================
# STAGE 2: Base Runtime Image with CUDA Runtime
# ============================================================================
FROM nvidia/cuda:12.2.0-runtime-ubuntu22.04 AS base-runtime

# Added libgomp1 and libc6 for ParquetSharp native dependencies
RUN apt-get update && apt-get install -y \
    wget \
    ca-certificates \
    libgomp1 \
    libc6 \
    && rm -rf /var/lib/apt/lists/*

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime dotnet --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    rm dotnet-install.sh

COPY --from=llama-builder /usr/local/bin/llama-server /usr/local/bin/llama-server

WORKDIR /app

# ============================================================================
# STAGE 3: Build .NET Application
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReadabilityConsoleApp/ReadabilityConsoleApp.csproj", "ReadabilityConsoleApp/"]
COPY ["CodeReadabilityLib/CodeReadabilityLib.csproj", "CodeReadabilityLib/"]
RUN dotnet restore "./ReadabilityConsoleApp/ReadabilityConsoleApp.csproj"
COPY . .
WORKDIR /src/ReadabilityConsoleApp
RUN dotnet build "ReadabilityConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ============================================================================
# STAGE 4: Publish .NET Application
# ============================================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ReadabilityConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ============================================================================
# STAGE 5: Final Runtime Image
# ============================================================================
FROM base-runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /models /output /datasets

ENV NVIDIA_VISIBLE_DEVICES=all
ENV NVIDIA_DRIVER_CAPABILITIES=compute,utility

ENTRYPOINT ["dotnet", "ReadabilityConsoleApp.dll"]
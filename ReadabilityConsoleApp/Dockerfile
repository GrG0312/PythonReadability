# ============================================================================
# STAGE 1: Build llama.cpp with CUDA Support
# ============================================================================
FROM nvidia/cuda:12.2.0-devel-ubuntu22.04 AS llama-builder

# Install necessary dependencies
# git: Clone llama.cpp repo
# build-essential: C/C++ compiler
# cmake: Build system generator
# curl: For downloading files
RUN apt-get update && apt-get install -y \
    git \
    build-essential \
    cmake \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /build

# Clone the llama.cpp repository and compile with CUDA support
RUN git clone https://github.com/ggerganov/llama.cpp.git && \
    cd llama.cpp && \
    # Configure the build with CUDA support
    cmake -B build -DLLAMA_CUDA=ON && \
    # Compile the project
    # -j$(nproc) uses all available CPU cores for faster compilation
    cmake --build build --config Release -j$(nproc) && \
    # Copy the compiled binary to a known location
    cp build/bin/llama-server /usr/local/bin/

# Result: We now have llama-server binary compiled with GPU support


# ============================================================================
# STAGE 2: Base Runtime Image with CUDA Runtime
# ============================================================================
# This stage sets up a lightweight runtime environment with CUDA support
FROM nvidia/cuda:12.2.0-runtime-ubuntu22.04 AS base-runtime

# Install necessary runtime dependencies
# wget: For downloading .NET installer
# ca-certificates: CA certificates for HTTPS
RUN apt-get update && apt-get install -y \
    wget \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install .NET 10 Runtime
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime dotnet --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    rm dotnet-install.sh

# Copy the compiled llama-server from Stage 1
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

RUN mkdir -p /models /output

ENV NVIDIA_VISIBLE_DEVICES=all
ENV NVIDIA_DRIVER_CAPABILITIES=compute,utility

ENTRYPOINT ["dotnet", "ReadabilityConsoleApp.dll"]
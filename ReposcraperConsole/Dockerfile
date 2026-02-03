# ============================================================================
# STAGE 1: Build llama.cpp with CUDA Support
# ============================================================================
FROM nvidia/cuda:12.2.0-devel-ubuntu22.04 AS llama-builder

# Install necessary dependencies
RUN apt-get update && apt-get install -y \
    git \                           # Clone llama.cpp repo
    build-essential \               # C/C++ compiler
    cmake \                         # Build system generator
    curl \                          # For downloading files
    && rm -rf /var/lib/apt/lists/*  # Clean up apt cache to recude image size

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
    cp build/bin/llama-server /user/local/bin/

# Result: We now have llama-server binary compiled with GPU support


# ============================================================================
# STAGE 2: Base Runtime Image with CUDA Runtime
# ============================================================================
# This stage sets up a lightweight runtime environment with CUDA support
FROM nvidia/cuda:12.2.0-runtime-ubuntu22.04 as base-runtime

# Install necessary runtime dependencies
RUN apt-get update && apt-get install -y \
    wget \                          # For downloading files (.NET installer)
    ca-certificates \               # CA certificates for HTTPS
    && rm -rf /var/lib/apt/lists/*  # Clean up apt cache to reduce image size

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    # -channel 10.0 : .NET 10
    # -runtime dotnet : Install only the .NET runtime (smaller)
    ./dotnet-install.sh --channel 10.0 --runtime dotnet --install-dir /user/share/dotnet && \
    # Create a symbolic link to make 'dotnet' command globally accessible
    ln -s /user/share/dotnet/dotnet /usr/bin/dotnet
    rm dotnet-install.sh

# Copy the compiled llama-server from Stage 1
COPY --from=llama-builder /user/local/bin/llama-server /usr/local/bin/llama-server


WORKDIR /app

# Result: Image with .NET 10 + CUDA runtime + llama-server binary


# ============================================================================
# STAGE 3: Build .NET Application
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release # Can be overridden: --build-arg BUILD_CONFIGURATION=Debug

WORKDIR /src
# Copy project files first (for Docker layer caching optimization)
# If source code changes but dependencies don't, Docker reuses the 'dotnet restore' layer (faster rebuilds)
# -> Copy csproj files and restore dependencies
COPY ["ReposcraperConsole/ReposcraperConsole.csproj", "ReposcraperConsole/"]
COPY ["Reposcraper/Reposcraper.csproj", "Reposcraper/"]

RUN dotnet restore "./ReposcraperConsole/ReposcraperConsole.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/ReposcraperConsole

RUN dotnet build "ReposcraperConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Result: Compiled .NET assemblies (DLLs)


# ============================================================================
# STAGE 4: Publish .NET Application
# ============================================================================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release # Can be overridden: --build-arg BUILD_CONFIGURATION=Debug

RUN dotnet publish ".ReposcraperConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false # Don't create native executable

# Result: Ready-to-run .NET application


# ============================================================================
# STAGE 5: Final Runtime Image
# ============================================================================
FROM base-runtime AS final

WORKDIR /app

COPY --from=publish /app/publish .

# Create directories for models and data
RUN mkdir -p /models && mkdir -p /output

# Environment variables for NVIDIA GPU support
# These tell Docker to expose GPUs to the container (???)
ENV NVIDIA_VISIBLE_DEVICES=all                  # All GPUs available
ENV NVIDIA_DRIVER_CAPABILITIES=compute,utility  # GPU compute + utilities

ENTRYPOINT ["dotnet", "ReposcraperConsole.dll"]

# Result: Final image with .NET app + CUDA + llama-server
# Total size: ~3GB (mainly due to CUDA runtime)
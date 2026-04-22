# .NET SDK Installation (macOS)

## Install a supported version (8, 9, 10)

Use Homebrew casks:

```bash
brew install --cask dotnet-sdk@9
brew install --cask dotnet-sdk@8
# dotnet-sdk (no suffix) installs the latest stable
```

## Install an EOL version (7 and below)

Use the official install script with `sudo` (required to write to the Homebrew-managed path):

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | sudo bash -s -- --channel 7.0 --install-dir /usr/local/share/dotnet
```

## Verify installed SDKs

```bash
dotnet --list-sdks
```

## Pin SDK version per project

Add a `global.json` to the project root:

```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMinor"
  }
}
```

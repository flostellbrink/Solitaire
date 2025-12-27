# Solitaire

Command line version of the Solitaire Game from [Shenzhen I/O](http://www.zachtronics.com/shenzhen-io/).

This tool solves boards automatically and prints a step by step solution.

## Server version

There is also a server version that receives screenshots and helps out with the next move. Start it with:

```bash
docker compose up
```

### GHCR image

The GitHub Actions workflow publishes the server image to GHCR:

- `ghcr.io/flostellbrink/solitaire-server:latest`
- `ghcr.io/flostellbrink/solitaire-server:<git-sha>`

### iOS Shortcut

Just take a screenshot in-game and share it with [this shortcut](https://www.icloud.com/shortcuts/85f5d6f726fd462797cc94c32c309a05) :)

## Screenshots

![GIF of solver](Screenshots/solver.gif)

## Getting Started

 - Clone repository.
 - Use your IDE of choice, OR
 - Use `dotnet run` in the project folder.

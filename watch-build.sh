#!/bin/bash

echo "Starting auto-build watcher for MultiplayerHelper mod..."
echo "Press Ctrl+C to stop"
echo ""

# Use dotnet watch to automatically rebuild on file changes
dotnet watch build
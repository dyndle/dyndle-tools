#!/bin/bash

curl -X POST https://content.dropboxapi.com/2/files/upload \
    --header "Authorization: Bearer $DROPBOX_TOKEN" \
    --header "Dropbox-API-Arg: {\"path\": \"/dyndle.com/dyndle.exe\",\"mode\": \"overwrite\",\"autorename\": false,\"mute\": true,\"strict_conflict\": false}" \
    --header "Content-Type: application/octet-stream" \
    --data-binary $BITBUCKET_CLONE_DIR/src/Dyndle.Tools.CLI/bin/Release/artifacts/dyndle.exe
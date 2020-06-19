#!/bin/bash

zip $BITBUCKET_CLONE_DIR/src/Dyndle.Tools.CLI/bin/Release/artifacts/dyndle.zip $BITBUCKET_CLONE_DIR/src/Dyndle.Tools.CLI/bin/Release/artifacts/dyndle.exe

curl -X POST https://content.dropboxapi.com/2/files/upload \
    --header "Authorization: Bearer $DROPBOX_TOKEN" \
    --header "Dropbox-API-Arg: {\"path\": \"/www/dyndle.com/dyndle.exe\",\"mode\": \"overwrite\",\"autorename\": false,\"mute\": true,\"strict_conflict\": false}" \
    --header "Content-Type: application/octet-stream" \
    --data-binary $BITBUCKET_CLONE_DIR/src/Dyndle.Tools.CLI/bin/Release/artifacts/dyndle.exe
	
curl -X POST https://content.dropboxapi.com/2/files/upload \
    --header "Authorization: Bearer $DROPBOX_TOKEN" \
    --header "Dropbox-API-Arg: {\"path\": \"/www/dyndle.com/dyndle.zip\",\"mode\": \"overwrite\",\"autorename\": false,\"mute\": true,\"strict_conflict\": false}" \
    --header "Content-Type: application/octet-stream" \
    --data-binary $BITBUCKET_CLONE_DIR/src/Dyndle.Tools.CLI/bin/Release/artifacts/dyndle.zip
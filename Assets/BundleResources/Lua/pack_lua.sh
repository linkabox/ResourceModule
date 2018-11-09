#!/bin/bash
set -e
CODE_DIR=./LuaCode

echo $OUT_PATH
for FILE_PATH in $(find $CODE_DIR -type f -name "*.lua"); do
    OUT_PATH=${FILE_PATH/LuaCode/LuaPackedCode}
    OUT_PATH=${OUT_PATH/.lua/.bytes}
    OUT_DIR=${OUT_PATH%/*}
    echo ================================================
    mkdir -p $OUT_DIR
    ./luac -o $OUT_PATH $FILE_PATH
    echo $FILE_PATH
    echo $OUT_PATH
done
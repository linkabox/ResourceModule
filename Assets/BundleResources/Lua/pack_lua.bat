@echo off 

set CODE_DIR=%cd%\LuaCode
SETLOCAL ENABLEDELAYEDEXPANSION

for /r %CODE_DIR% %%f in (*.lua) do ( 
set FILE_PATH=%%f
set OUT_PATH=!FILE_PATH:LuaCode=LuaPackedCode!
set OUT_PATH=!OUT_PATH:.lua=.bytes!
set OUT_DIR=%%~dpf
set OUT_DIR=!OUT_DIR:LuaCode=LuaPackedCode!
echo ================================================
if not exist "!OUT_DIR!" mkdir !OUT_DIR!
luac -o !OUT_PATH! !FILE_PATH!
echo !FILE_PATH!
echo !OUT_PATH!
)
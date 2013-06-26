@echo off

setlocal
set error=0
set vs_profile=vs_5_0
set ps_profile=ps_5_0
set compiler_dir=..\..\..\Libra\Tools.CompileShader\bin\Debug

::
:: ChunkEffect
::		VS/PS
::		OcclusionVS/OcclusionPS
::		WireframeVS/WireframePS
::
call :CompileVS Chunk VS %vs_profile%
call :CompilePS Chunk PS %ps_profile%
call :CompileVS Chunk OcclusionVS %vs_profile%
call :CompilePS Chunk OcclusionPS %ps_profile%
call :CompileVS Chunk WireframeVS %vs_profile%
call :CompilePS Chunk WireframePS %ps_profile%
call :CompileVS SkySphere VS %vs_profile%
call :CompilePS SkySphere PS %ps_profile%

:: TODO

echo.

if %error% == 0 (
    echo Shaders compiled ok
) else (
    echo There were shader compilation errors!
)

endlocal
exit /b

:CompileShader
set compiler=%compiler_dir%\Tools.CompileShader.exe %1.fx Compiled\%1%2.bin %2 %3
echo.
echo %compiler%
%compiler% || set error=1
exit /b

:CompileVS
set compiler=%compiler_dir%\Tools.CompileShader.exe %1.vs Compiled\%1%2.bin %2 %3
echo.
echo %compiler%
%compiler% || set error=1
exit /b

:CompilePS
set compiler=%compiler_dir%\Tools.CompileShader.exe %1.ps Compiled\%1%2.bin %2 %3
echo.
echo %compiler%
%compiler% || set error=1
exit /b

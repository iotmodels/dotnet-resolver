.\dtdl2-validator\bin\Debug\netcoreapp3.1\dtdl2-validator.exe /f=test/demodevice.json /resolver=local /baseFolder=../registry/dtmi
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)
./dtdl2-validator/bin/Debug/netcoreapp3.1/dtdl2-validator /f=test/demodevice.json /resolver=local /baseFolder=../registry /Logging:LogLevel:Default=Information
if [ $? -eq 0 ]
then
  echo "validation ok"
else
  echo "error validating model"
  exit 1
fi
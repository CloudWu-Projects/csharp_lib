set slnName=%~dp0\Codes\wu_jiaxing20220115.sln
set PublishDir=%~dp0\Codes\bin\publish\
rd /s  /Q %PublishDir%

dotnet publish %slnName% -c Release -p:PublishDir=%PublishDir% -r win-x86 --self-contained false -p:PublishSingleFile=true

python sendMail3.py %PublishDir%
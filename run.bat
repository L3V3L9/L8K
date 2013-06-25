del /F /S /Q public
xcopy src\app\thewall public  /E
xcopy vendor\thewall public\js\  /E
xcopy vendor\underscore public\js\ /E
python src\server\bottle\server.py localhost 3000

PAUSE
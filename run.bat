del /F /S /Q public
xcopy src\app\thewall  public  /E
python src\server\bottle\server.py
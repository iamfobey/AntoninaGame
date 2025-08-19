import subprocess
import os

path = (os.path.dirname(os.path.realpath(__file__)) + '\\').replace('\\', '/')

print("Build.")

subprocess.Popen("dotnet restore",
    shell=True,
    cwd=path + "../",
    stdout=None,
    stderr=None,
    encoding='utf-8',
    errors='replace'
).wait()

subprocess.Popen("dotnet build -c Debug AntoninaGame.csproj",
    shell=True,
    cwd=path + "../",
    stdout=None,
    stderr=None,
    encoding='utf-8',
    errors='replace'
).wait()

print("End Build.")
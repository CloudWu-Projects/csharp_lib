

from sendMailBase import sendFolder

from git import Repo
import pathlib

baseFolder=r'Codes\bin\Release\net6.0\win-x86'

import os

# 从此文件所在目录 往上查找 .git 文件夹
# 如果找到了就停止 返回 .git 文件夹所在目录

def findGitFolder():
    path = os.path.dirname(os.path.realpath(__file__))
    
    while True:        
        if os.path.exists(os.path.join(path,'.git')):
            if 'csharp_lib' not in path:
                return path
        path = os.path.dirname(path)
        if len(path)<3:
            break
    return None   
        
gitFolder = findGitFolder()
print("gitFolder",gitFolder)
# search *.sln from gitFolder and it subfolders
def findSlnFolder():
    for root, dirs, files in os.walk(gitFolder):
        for file in files:
            if file.endswith(".sln"):
                return root
    return None

slnFolder = findSlnFolder()
print("slnFolder",slnFolder)


repo=Repo(gitFolder)
shortHash = repo.head.commit.hexsha[:8]
projectName = repo.head.ref.name

#projectName= os.path.basename(gitBaseFolder)

PublishDir=os.path.join(slnFolder,'bin','publish')

print(PublishDir)
import shutil
try:
    shutil.rmtree(PublishDir)
except:
    pass

dotnetCommand=f'dotnet publish {slnFolder} -c Release -p:PublishDir={PublishDir},AssemblyName={projectName} --version-suffix {shortHash} -r win-x86 --self-contained false -p:PublishSingleFile=true'

a =os.system(dotnetCommand)
print(a)

if a==0:
    print('publish success')
    sendFolder(PublishDir,gitFolder)

    print('send success')

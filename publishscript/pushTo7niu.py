

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

print(projectName)
#projectName= os.path.basename(gitBaseFolder)

PublishDir=os.path.join(slnFolder,'bin','publish')

print(PublishDir)
print(shortHash)

def Build():
    import shutil
    try:
        shutil.rmtree(PublishDir)
    except:
        pass
    #form systemtime  get  yyyy mm dd
    import datetime
    now = datetime.datetime.now()
    curversion=now.strftime("%Y.%m%d.%H%M")
    #curversion="1.0.0"
    print(curversion)
    dotnetCommand=f'dotnet publish {slnFolder} -c Release -p:PublishDir={PublishDir},AssemblyName={projectName} -p:VersionPrefix={curversion}   -r win-x86 --self-contained false -p:PublishSingleFile=true'
    print(dotnetCommand)
    
    a =os.system(dotnetCommand +"> nul 2>&1")
    print("version:",curversion)
    return a

import os,io
import zipfile
def ZipFolder(folderPath,projectDir):
    if os.path.exists(folderPath)==False:
        print(F"{folderPath}  don't exists")
        return
    print(F"{folderPath}  exists")
    ziplist=[]
    for dir,subdirs,files in os.walk(folderPath):
        if "logs" in subdirs:
            subdirs.remove("logs")
        if ".vs" in subdirs:
            subdirs.remove(".vs")
        
        
        if "config.ini" in files:
            files.remove("config.ini")
            
        for fileItem in files:
            if fileItem.endswith(".zip"):
                continue
            ziplist.append(os.path.join(dir,fileItem))            
        for dirItem in subdirs:
            ziplist.append(os.path.join(dir,dirItem))
    
    usePyminizip=False
    try:
        import pyminizip
        usePyminizip=True
    except:
        pass
    publishZipPath=os.path.join(folderPath,"publish.zip")
    pyminizipPath=os.path.join(folderPath,"output.zip")
    print("zip path:",publishZipPath)
    print("pyminizip path:",pyminizipPath)
    if usePyminizip==False:
        in_memory_zip = io.BytesIO()
    else:
        in_memory_zip=publishZipPath
    
    z = zipfile.ZipFile(in_memory_zip,'w',zipfile.ZIP_DEFLATED)
    
    for i in ziplist:
        z.write(i,i.replace(folderPath,''))
    z.close()

    if usePyminizip:    
        print("use pyminizip",pyminizipPath)
        try:
            pyminizip.compress(in_memory_zip,None, pyminizipPath, "GFG123456", 5)
            in_memory_zip = io.BytesIO(open(pyminizipPath,'rb').read())    
        except:
            pass

    
    if usePyminizip:
        pass
      #  os.remove(publishZipPath)
      #  os.remove(pyminizipPath)
    return publishZipPath

a =Build()
print("build result:",a )

if a==0:
    print('publish success')
    zipPath = ZipFolder(PublishDir,gitFolder)
    # python ./7niu/upload_7niu.py ./bin/$appName --filename=$appName
    import upload_7niu
    upload_7niu.upload(zipPath,key=f"{projectName}/{projectName}.zip")
         #  args.localfile,key=f"{args.filename}/{args.filename}.zip")
    print('send success')

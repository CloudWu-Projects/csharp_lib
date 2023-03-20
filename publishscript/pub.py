import os
def getSlnFile():
    currDir=os.path.dirname(__file__)
    slnPath=''
    while len(currDir)>4:        
        all_file = os.listdir(currDir)
        target_file = [x for x in all_file if x.endswith('.sln')]
        if len(target_file)>0:
            slnPath=os.path.join(currDir,target_file[0])
            break
        currDir=os.path.dirname(currDir)
    print(currDir)
    print(slnPath)    
    return slnPath,currDir

def getGitBaseFolder(startFolder):
    currDir=startFolder
    while len(currDir)>4:
        gitFolder = os.path.join(currDir, '.git')
        if os.path.exists(gitFolder):
            return currDir
        currDir=os.path.dirname(currDir)
        
        

slnName ,currDir=getSlnFile()
gitBaseFolder=getGitBaseFolder(currDir)
print(gitBaseFolder)

PublishDir=os.path.join(currDir,'bin','publish')

print(PublishDir)
dotnetCommand=f'dotnet publish {slnName} -c Release -p:PublishDir={PublishDir} -r win-x86 --self-contained false -p:PublishSingleFile=true'
a =os.system(dotnetCommand)
print(a)

if a==0:
    from sendMailBase import sendFolder
    #sendFolder(PublishDir,gitBaseFolder)

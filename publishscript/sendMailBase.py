from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from email.mime.application import MIMEApplication

import smtplib

try:
    import Emailconfig
    # 输入Email地址和口令:
    from_addr = Emailconfig.from_addr
    password = Emailconfig.password
    # 输入收件人地址:
    to_addr = Emailconfig.to_addr
    # 输入SMTP服务器地址:
    smtp_server = Emailconfig.smtp_server
except ImportError:
    import os
    Emailconfig = None
    # 输入Email地址和口令:
    from_addr = '13348926569@189.cn' #os.getenv('from_addr')
    password = os.getenv('password')
    # 输入收件人地址:
    to_addr = "daiybh@qq.com,3305295410@qq.com" #os.getenv('to_addr')
    # 输入SMTP服务器地址:
    smtp_server = 'smtp.189.cn' #os.getenv('smtp_server')


import os,io
import zipfile
import socket

def sendMail_FILE(title,content,file):
    msg = MIMEMultipart()
    part_text = MIMEText(content)
    msg.attach(part_text)

    part_attach = MIMEApplication(open(file,'rb').read())    
    part_attach.add_header('Content-Disposition','attachment',filename=file)
    msg.attach(part_attach)

    msg['Subject'] = title
    msg['From'] = from_addr
    server = smtplib.SMTP(smtp_server, 25) # SMTP协议默认端口是25
    #server.set_debuglevel(1)
    print(f"from_addr:{from_addr}  password:{ password}")
    server.login(from_addr, password)
    server.sendmail(from_addr, to_addr.split(','), msg.as_string())
    server.quit()
    print("send over")

from git import Repo
def sendMail_Zip(z,projectMsg,projectDir='./'):
    
    print("send starting....")

    import time
		
    repo=Repo(projectDir)
    headcommit = repo.head.commit

    mailText='url:{0}  \nbranch:{1}\nhash:{2}\nmessage:{3}'.format(repo.remotes.origin.url,repo.head.reference,repo.head.commit,headcommit.message)
	
    content='proect:\n{0}\n\n{1} \n\n\n\n{2}'.format(projectMsg,mailText,time.asctime())
	
    textApart = MIMEText(content)
	
    zipApart=MIMEApplication(z.getvalue())
    zip_name= str(repo.head.reference)
    zipApart.add_header('Content-Disposition','attachment',filename=zip_name+'.zip')

    m= MIMEMultipart()

    m.attach(textApart)
    m.attach(zipApart)
    m['Subject']='proect :{0} {1}'.format(headcommit.message,zip_name)

    server = smtplib.SMTP(smtp_server, 25) # SMTP协议默认端口是25
    #server.set_debuglevel(1)
    print(f"from_addr:{from_addr}  password:{ password}")
    server.login(from_addr, password)
    server.sendmail(from_addr, to_addr.split(','), m.as_string())
    server.quit()
    print("send over")


def sendMail_text(title,textContent):
    # 输入Email地址和口令:
    from_addr = Emailconfig.from_addr
    password = Emailconfig.password
    # 输入收件人地址:
    to_addr = Emailconfig.to_addr
    # 输入SMTP服务器地址:
    smtp_server = Emailconfig.smtp_server

    textApart = MIMEText(textContent)
    m= MIMEMultipart()

    m.attach(textApart)
    m['Subject']='revert '+title
    
    server = smtplib.SMTP(smtp_server, 25) # SMTP协议默认端口是25
    #server.set_debuglevel(1)
    server.login(from_addr, password)
    server.sendmail(from_addr, to_addr.split(','), m.as_string())
    server.quit()


def sendFolder(folderPath,projectDir):
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
            ziplist.append(os.path.join(dir,fileItem))            
        for dirItem in subdirs:
            ziplist.append(os.path.join(dir,dirItem))
    
    usePyminizip=False
    try:
        import pyminizip
        usePyminizip=True
    except:
        pass

    if usePyminizip==False:
        in_memory_zip = io.BytesIO()
    else:
        in_memory_zip="./publish.zip"
    
    z = zipfile.ZipFile(in_memory_zip,'w',zipfile.ZIP_DEFLATED)
    
    for i in ziplist:
        z.write(i,i.replace(folderPath,''))
    z.close()

    if usePyminizip:    
        pyminizip.compress(in_memory_zip,None, "./output.zip", "GFG123456", 5)
        in_memory_zip = io.BytesIO(open("./output.zip",'rb').read())    


    hostName=socket.gethostname()
    projectMsg=f"codesPath:{projectDir} \n codePC:{hostName} {socket.gethostbyname(hostName)}"
    projectMsg+=f'\n zip password : GFG123456'
    sendMail_Zip(in_memory_zip,projectMsg=projectMsg,projectDir=projectDir)

    if usePyminizip:
        os.remove("./output.zip")
        os.remove("./publish.zip")

# importing module
import pyminizip

# input file path
inpt = "F:\\Codes\\Simplylive_project\\VenueGateway_VEN\\deltacastframeconsumer\\CMakeLists.txt"



def Compress(fileList,zipName):
    pyminizip.compress_multiple(fileList, None, "./output.zip", "GFG", 5)
# prefix path
pre = None

# output zip file path
oupt = "./output.zip"

# set password value
password = "GFG"

# compress level
com_lvl = 5

# compressing file
pyminizip.compress(inpt, None, oupt,
				password, com_lvl)

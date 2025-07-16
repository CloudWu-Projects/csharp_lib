# -*- coding: utf-8 -*-
# flake8: noqa

from qiniu import Auth, put_file, etag,CdnManager
import qiniu.config

#需要填写你的 Access Key 和 Secret Key
access_key = 'z8OskufSjOkWbJt7j7asi-1uWp82_ed7l66MkJNT'
secret_key = 'DcXQn07x_qZU5B2h8CNopJwlv4ceosM3PTlep1gs'


def upload(localfile,key=None):
    #构建鉴权对象
    q = Auth(access_key, secret_key)

    #要上传的空间
    bucket_name = 'cloud-wu'

    #上传后保存的文件名
    if key is None:
        key = localfile.split('/')[-1]
    
   

    #生成上传 Token，可以指定过期时间等
    token = q.upload_token(bucket_name, key, 3600)

    #要上传文件的本地路径
    

    ret, info = put_file(token, key, localfile, version='v2')
    print("ret",ret)
    print("info",info)
    print("----")
    print(info.url)
    assert ret['key'] == key
    assert ret['hash'] == etag(localfile)
    urls = ['http://7niu.hyman.store/'+key]
    cdn_manager = CdnManager(q)
    refresh_url_result = cdn_manager.refresh_urls(urls)
    print(refresh_url_result[0])
    r=refresh_url_result[1]
    print(r)
    assert r.status_code==200
    # 刷新目录

import argparse
def upload_from_args():
    parser = argparse.ArgumentParser(description="Download files from Lanzou Cloud.")
    parser.add_argument(
        "localfile", 
        nargs="?",  # 可选参数
        default="",  # 默认值
        help="upload localfile to qiniu"
    )
    parser.add_argument(
        "--filename",
        type=str,
        default="yilingshequ_go",
        help="Custom filename to use in the storage key (default: yilingshequ_go)"
    )
    args = parser.parse_args()

    upload(args.localfile,key=f"{args.filename}/{args.filename}.zip")

if __name__ == '__main__':
    #upload_from_version()
    upload_from_args()
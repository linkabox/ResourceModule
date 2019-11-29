using System.Collections.Generic;

namespace ResourceModule
{
    public class PatchInfo
    {
        public string version; //当前版本
        public string nextVer; //升级版本
        public string hash;    //补丁包MD5值
        public long fileSize;  //补丁包文件大小
        public string fileName;

        public Dictionary<string, string> fileHash;
    }
}
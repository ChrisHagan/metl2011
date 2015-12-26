using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using MeTLLib.Providers.Connection;
using MeTLLib.Providers;
using System.Diagnostics;
//using Ninject;
using System;
using System.IO;

namespace MeTLLib.Providers.Connection
{
    public interface ResourceUploaderFactory
    {
        IResourceUploader get();
    }
    public class ProductionResourceUploaderFactory : ResourceUploaderFactory
    {
        protected HttpResourceProvider provider;
        protected MetlConfiguration config;
        public ProductionResourceUploaderFactory(MetlConfiguration _config, HttpResourceProvider _provider)
        {
            provider = _provider;
            config = _config;
        }
        public IResourceUploader get()
        {
            return new ProductionResourceUploader(config,provider);
        }
    }
    public interface IResourceUploader
    {
        string uploadResource(string path, string file);
        //string uploadResource(string path, string file, bool overwrite);
        string uploadResourceToPath(byte[] data, string file, string name);
        string uploadResourceToPath(byte[] data, string file, string name, bool overwrite);
        string uploadResourceToPath(string localFile, string remotePath, string name);
        string uploadResource(string jid, string preferredFilename, string file);
        string uploadResourceToPath(string localFile, string remotePath, string name, bool overwrite);
        //string getStemmedPathForResource(string path, string name);
    }
    public class ProductionResourceUploader : IResourceUploader
    {
        protected MetlConfiguration metlServerAddress;
        protected HttpResourceProvider _httpResourceProvider;
        public ProductionResourceUploader(MetlConfiguration _metlServerAddress,HttpResourceProvider provider)
        {
            metlServerAddress = _metlServerAddress;
            _httpResourceProvider = provider;
        }
        public string uploadResource(string path, string file)
        {
            return uploadResource(new Uri(path), file, false);
        }
        public string uploadResource(string jid, string preferredFilename,string file)
        {
            var fullPath = metlServerAddress.uploadResource(preferredFilename, jid);
            return uploadResource(fullPath, file,false);
        }
        protected string uploadResource(Uri path, string file, bool overwrite)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "Argument cannot be null");
            try
            {
                var fullPath = path;
                var res = _httpResourceProvider.securePutData(fullPath, File.ReadAllBytes(file));//  securePutFile(fullPath, file);
                return XElement.Parse(res).Value;
            }
            catch (WebException e)
            {
                Trace.TraceError("Cannot upload resource: " + e.Message);
                throw e;
            }
        }
        public string uploadResourceToPath(byte[] resourceData, string path, string name)
        {
            return uploadResourceToPath(resourceData, path, name, true);
        }
        public string uploadResourceToPath(byte[] resourceData, string path, string name, bool overwrite)
        {
            /*
            if (resourceData == null || resourceData.Length == 0) throw new ArgumentNullException("resourceData", "Argument cannot be null"); 
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path", "Argument cannot be null");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "Argument cannot be null");
            var url = string.Format("{0}?path={1}/{2}&overwrite={3}&filename={4}", RESOURCE_SERVER_UPLOAD, INodeFix.Stem(path),path, overwrite.ToString().ToLower(), name);
            var res = _httpResourceProvider.securePutData(new System.Uri(url), resourceData);
            return XElement.Parse(res).Attribute("url").Value;
            */
            return "";
        }
        /*
        public string getStemmedPathForResource(string path, string name)
        {
            return string.Format("{0}/{1}/{2}/{3}", metlServerAddress.resourceUrl, metlServerAddress.resourceDirectory, INodeFix.Stem(path), path, name);

//            return string.Format("{5}://{0}:{1}/{2}/{3}/{4}", metlServerAddress.host, metlServerAddress.port, INodeFix.Stem(path), path, name, metlServerAddress.protocol);
        }
        */
        public string uploadResourceToPath(string localFile, string remotePath, string name)
        {
            return uploadResourceToPath(localFile, remotePath, name, true);
        }
        public string uploadResourceToPath(string localFile, string remotePath, string name, bool overwrite)
        {
            return "";
            /*
            if (string.IsNullOrEmpty(localFile)) throw new ArgumentNullException("localFile", "Argument cannot be null");
            if (string.IsNullOrEmpty(remotePath)) throw new ArgumentNullException("remotePath", "Argument cannot be null");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "Argument cannot be null");
            var url = string.Format("{0}?path=Resource/{1}/{2}&overwrite={3}&filename={4}", RESOURCE_SERVER_UPLOAD, INodeFix.Stem(remotePath),remotePath, overwrite.ToString().ToLower(), name);
            var res = _httpResourceProvider.securePutFile(new System.Uri(url), localFile);
            return XElement.Parse(res).Attribute("url").Value;
            */
        }
        /*
         * \Resources
         * \Resources\1011\a.png
         * \Structure
         * \Structure\listing.xml
         * \Structure\myFirstConversation\details.xml
         */
    }
}
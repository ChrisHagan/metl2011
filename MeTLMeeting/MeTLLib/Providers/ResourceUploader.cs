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
        public IAuditor auditor { get; protected set; }
        public ProductionResourceUploaderFactory(MetlConfiguration _config, HttpResourceProvider _provider,IAuditor _auditor)
        {
            auditor = _auditor;
            provider = _provider;
            config = _config;
        }
        public IResourceUploader get()
        {
            return new ProductionResourceUploader(config, provider,auditor);
        }
    }
    public interface IResourceUploader
    {
        string uploadResource(string path, byte[] bytes);
        string uploadResource(string path, string file);
        //string uploadResource(string path, string file, bool overwrite);
        string uploadResourceToPath(byte[] data, string file, string name);
        string uploadResourceToPath(byte[] data, string file, string name, bool overwrite);
        string uploadResourceToPath(string localFile, string remotePath, string name);
        string uploadResource(string jid, string preferredFilename, string file);
        string uploadResource(string jid, string preferredFilename, byte[] bytes);
        string uploadResourceToPath(string localFile, string remotePath, string name, bool overwrite);
        //string getStemmedPathForResource(string path, string name);
    }
    public class ProductionResourceUploader : IResourceUploader
    {
        protected MetlConfiguration metlServerAddress;
        protected HttpResourceProvider _httpResourceProvider;
        public IAuditor auditor { get; protected set; }
        public ProductionResourceUploader(MetlConfiguration _metlServerAddress, HttpResourceProvider provider,IAuditor _auditor)
        {
            auditor = _auditor;
            metlServerAddress = _metlServerAddress;
            _httpResourceProvider = provider;
        }
        public string uploadResource(string path, byte[] bytes)
        {
            try
            {
                var returnId = "";
                var fullPath = new Uri(path);
                MeTLLib.DataTypes.MeTLStanzas.ImmutableResourceCache.cache(fullPath, (p) => {
                    returnId = _httpResourceProvider.securePutData(p, bytes);
                    return bytes;
                });
                return XElement.Parse(returnId).Value;
            }
            catch (WebException e)
            {
                auditor.error("uploadResource: "+path, "ProductionResourceUploader", e);
                throw e;
            }
        }
        public string uploadResource(string path, string file)
        {
            return uploadResource(new Uri(path), file, false);
        }
        public string uploadResource(string jid, string preferredFilename, string file)
        {
            var fullPath = metlServerAddress.uploadResource(preferredFilename, jid);
            return uploadResource(fullPath, file, false);
        }
        public string uploadResource(string jid, string preferredFilename, byte[] bytes)
        {
            var fullPath = metlServerAddress.uploadResource(preferredFilename, jid);
            return uploadResource(fullPath, bytes, false);
        }
        protected string uploadResource(Uri path, string file, bool overwrite)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "Argument cannot be null");
            try
            {
                var fileLocation = new Uri(file).LocalPath;
                var returnId = "";
                MeTLLib.DataTypes.MeTLStanzas.ImmutableResourceCache.cache(path, (fullPath) => {
                    var bytes = File.ReadAllBytes(fileLocation);
                    returnId = _httpResourceProvider.securePutData(fullPath, bytes);
                    return bytes;
                });
                return XElement.Parse(returnId).Value;
            }
            catch (WebException e)
            {
                auditor.error("uploadResource: " + path.ToString(), "ProductionResourceUploader", e);
                throw e;
            }
        }
        protected string uploadResource(Uri path, byte[] bytes, bool overwrite)
        {
            try
            {
                var returnId = "";
                MeTLLib.DataTypes.MeTLStanzas.ImmutableResourceCache.cache(path, (fullPath) => {
                    returnId = _httpResourceProvider.securePutData(fullPath, bytes);
                    return bytes;
                });
                return XElement.Parse(returnId).Value;
            }
            catch (WebException e)
            {
                auditor.error("uploadResource: " + path.ToString(), "ProductionResourceUploader", e);
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
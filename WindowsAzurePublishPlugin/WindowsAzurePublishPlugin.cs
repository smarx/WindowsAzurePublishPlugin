using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Expression.Encoder.Plugins.Publishing;
using System.Xml;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace WindowsAzurePublishPlugin
{
    [EncoderPlugin("Windows Azure Publisher", "Publishes videos to Windows Azure blob storage")]
    public class WindowsAzurePublishPlugin : PublishPlugin
    {
        private PublishData data;
        private bool cancelled;

        public WindowsAzurePublishPlugin()
        {
            data = new PublishData();
        }

        internal bool IsCancelled
        {
            get { return cancelled; }
        }

        public override object CreateStandardSettingsEditor()
        {
            return new SimpleUI(data);
        }

        public override object CreateAdvancedSettingsEditor()
        {
            return null;
        }

        public override void PerformPublish(string rootPath, string[] filesToPublish)
        {
            if (string.IsNullOrEmpty(data.AccountName) || string.IsNullOrEmpty(data.AccountKey) || string.IsNullOrEmpty(data.BlobPath))
            {
                PublishingHost.ShowMessageBox("You need to enter your Windows Azure storage account information before proceeding", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            cancelled = false;

            var blobs = new CloudStorageAccount(new StorageCredentialsAccountAndKey(data.AccountName, data.AccountKey), true).CreateCloudBlobClient();
            CreateClientAccessPolicy(blobs);
            var directory = blobs.GetBlobDirectoryReference(data.BlobPath);
            if (directory.Container.CreateIfNotExist())
            {
                directory.Container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            int count = 0;
            var files = Directory.EnumerateFiles(rootPath).ToList();
            Parallel.ForEach(files, (file) =>
            {
                var blob = directory.GetBlobReference(Path.GetFileName(file));
                if (file.Contains(".ismv"))
                {
                    blob.Properties.ContentType = "video/ismv";
                }
                blob.Properties.CacheControl = "max-age=7200";
                blob.UploadFile(file);
                OnProgress(string.Format("Uploaded {0} of {1} files", Interlocked.Increment(ref count), files.Count), (double)count/files.Count);
            });
        }

        public void CreateClientAccessPolicy(CloudBlobClient blobs)
        {
            var root = blobs.GetContainerReference("$root");
            try
            {
                root.Create();
                root.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            catch (StorageException e)
            {
                if (e.ErrorCode == StorageErrorCode.ContainerAlreadyExists)
                {
                    Debug.WriteLine("Root container already exists.  Not setting permissions on it.");
                }
                else
                {
                    throw;
                }
            }
            var cap = root.GetBlobReference("clientaccesspolicy.xml");
            cap.Properties.ContentType = "text/xml";
            try
            {
                cap.UploadText(@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <access-policy>
                            <cross-domain-access>
                                <policy>
                                    <allow-from>
                                        <domain uri=""*"" />
                                        <domain uri=""http://*"" />
                                    </allow-from>
                                    <grant-to>
                                        <resource path=""/"" include-subpaths=""true"" />
                                    </grant-to>
                                </policy>
                            </cross-domain-access>
                        </access-policy>", Encoding.ASCII, new BlobRequestOptions() { AccessCondition = AccessCondition.IfNoneMatch("*") });
                Debug.WriteLine("Wrote ClientAccessPolicy.xml.");
            }
            catch (StorageException e)
            {
                if (e.ErrorCode == StorageErrorCode.BlobAlreadyExists)
                {
                    Debug.WriteLine("ClientAccessPolicy.xml already exists.  Not overwriting.");
                }
                else
                {
                    throw;
                }
            }
        }

        public override void LoadJobSettings(XmlReader reader)
        {
            reader.ReadStartElement("WindowsAzurePublishPlugin");
            data.AccountName = reader.ReadElementString("AccountName");
            data.AccountKey = reader.ReadElementString("AccountKey");
            data.BlobPath = reader.ReadElementString("BlobPath");
            reader.ReadEndElement();
        }

        public override void SaveJobSettings(XmlWriter writer)
        {
            writer.WriteStartElement("WindowsAzurePublishPlugin");
            writer.WriteElementString("AccountName", data.AccountName);
            writer.WriteElementString("AccountKey", data.AccountKey);
            writer.WriteElementString("BlobPath", data.BlobPath);
            writer.WriteEndElement();
        }

        protected override void CancelPublish()
        {
            cancelled = true;
        }
    }
}
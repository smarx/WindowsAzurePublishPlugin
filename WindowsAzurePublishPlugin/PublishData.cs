using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace WindowsAzurePublishPlugin
{
    public class PublishData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string accountName = "";
        public string AccountName
        {
            get { return accountName; }
            set { accountName = value; OnPropChanged("AccountName"); }
        }

        private string accountKey = "";
        public string AccountKey
        {
            get { return accountKey; }
            set { accountKey = value; OnPropChanged("AccountKey"); }
        }

        private string blobPath = "";
        public string BlobPath
        {
            get { return blobPath; }
            set { blobPath = value; OnPropChanged("BlobPath"); }
        }

        internal void OnPropChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
WindowsAzurePublishPlugin is a publishing plugin for Expression Encoder that pushes video content to Windows Azure blog storage.

Installation
------------
To install the plugin, build the plugin with Visual Studio (or download the binaries) and copy `WindowsAzurePublishPlugin.dll` and
`Microsoft.WindowsAzure.StorageClient.dll` into `\Program Files\Microsoft Expression\Encoder 4\Plugins` (or `\Program Files (x86)\...`
on a 64-bit computer.

**NOTE**: If you download the DLLs instead of building the project yourself, you'll need to open their properties and "Unblock" them.
(They'll be marked as potentially unsafe, because they were downloaded from the internet.)

Usage
-----
Enter a Windows Azure storage account and key, along with a full path (container name and any path under that). For example, if you use `videos/test`,
your content will end up in the `videos` container, under paths like `test/*`. Note that container names in Windows Azure do not allow capital letters,
so the first part of the path needs to be all lowercase.

Notes
-----
If the specified blob container doesn't already exist, the plugin will create it and set its access control to public so the blobs can be directly shared
or referenced from, e.g., a Silverlight player. Similarly, the plugin will try to create a `$root` container and add a liberal `clientaccesspolicy.xml`
blob to allow Silverlight players to use the storage account.

All blobs uploaded by the plugin are marked with a `Cache-Control` header of `max-age=7200`, which makes the content cacheable for up to two hours.
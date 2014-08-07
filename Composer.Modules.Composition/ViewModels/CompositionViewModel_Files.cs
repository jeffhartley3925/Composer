using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Messaging;
using Composer.Modules.Composition.Views;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class CompositionViewModel: BaseViewModel, IEventCatcher
    {

        private string _rawSize;
        public string RawSize
        {
            get { return _rawSize; }
            set
            {
                _rawSize = value;
                OnPropertyChanged(() => RawSize);
            }
        }

        private string _compressedSize;
        public string CompressedSize
        {
            get { return _compressedSize; }
            set
            {
                _compressedSize = value;
                OnPropertyChanged(() => CompressedSize);
            }
        }

        private string _uploadResponse;
        public string UploadResponse
        {
            get { return _uploadResponse; }
            set
            {
                _uploadResponse = value;
                OnPropertyChanged(() => UploadResponse);
            }
        }

        private Visibility _uploadDetailsVisibility = Visibility.Collapsed;
        public Visibility UploadDetailsVisibility
        {
            get { return _uploadDetailsVisibility; }
            set
            {
                _uploadDetailsVisibility = value;
                OnPropertyChanged(() => UploadDetailsVisibility);
            }
        }

        public void SubscribeFilesEvents()
        {
            EA.GetEvent<CreateAndUploadImage>().Subscribe(OnCreateAndUploadImage, true);
            EA.GetEvent<CreateAndUploadFile>().Subscribe(OnCreateAndUploadFile, true);
        }

        public void OnCreateAndUploadFile(object obj)
        {
            SendFile();
        }

        public void OnCreateAndUploadImage(object obj)
        {
            EA.GetEvent<SetCompositionWidthHeight>().Publish(string.Empty);

            var document = HtmlPage.Document;
            var txtArea = document.GetElementById("MainContent_txtPNGBytes");

            if (txtArea != null)
            {
                try
                {
                    const double scale = 1;

                    var bmp = new WriteableBitmap(CompositionGrid, null);
                    var buffer = bmp.ToByteArray();

                    bmp = new WriteableBitmap(int.Parse(Width.ToString(CultureInfo.InvariantCulture)), int.Parse(Height.ToString(CultureInfo.InvariantCulture)));
                    bmp.FromByteArray(buffer);

                    var transform = new ScaleTransform { ScaleX = scale, ScaleY = scale };

                    bmp.Render(CompositionGrid, transform);
                    bmp.Invalidate();

                    var stream = bmp.GetStream();
                    var binaryData = new Byte[stream.Length];
                    var bytesRead = stream.Read(binaryData, 0, (int)stream.Length);
                    var base64 = Convert.ToBase64String(binaryData, 0, binaryData.Length);

                    RawSize = base64.Length.ToString(CultureInfo.InvariantCulture);
                    var message = Compression.Compress(base64);
                    base64 = message.Text;
                    CompressedSize = base64.Length.ToString(CultureInfo.InvariantCulture);
                    txtArea.SetProperty("value", base64);

                    UploadDetailsVisibility = Visibility.Visible;

                    SendImage();
                }
                catch (Exception ex)
                {
                    Exceptions.HandleException(ex, "Error in: OnCreateAndUploadImage");
                }
            }
        }

        private void SendFile()
        {
            _uri = new Uri(@"/composer/Home/CreateFile", UriKind.Relative);
            _client = new WebClient();

            // you MUST modify the header fields for this to work otherwise it will respond
            // with regular HTTP headers.
            _client.Headers["content-type"] = "application/json";

            // this will be fired after the upload is complete.
            _client.UploadStringCompleted += (sndr, evnt) =>
            {
                if (evnt.Error != null)
                {
                    UploadResponse = string.IsNullOrWhiteSpace(evnt.Error.Message)
                        ? @"An exception occurred."
                        : evnt.Error.Message;
                }
                else if (evnt.Cancelled)
                {
                    UploadResponse = "Operation was canceled.";
                }
            };

            var myObject = new Message { CompositionId = Composition.Id.ToString(), CollaborationId = Current.User.Index, Text = "", CompositionTitle = Composition.Provenance.TitleLine };
            string json = Serialization.ToJson(myObject);
            _client.UploadStringAsync(_uri, "POST", json);
        }

        private void SendImage()
        {
            _uri = new Uri(@"/composer/Home/PushMessage", UriKind.Relative);
            _client = new WebClient();

            // you MUST modify the header fields for this to work otherwise it will respond
            // with regular HTTP headers.
            _client.Headers["content-type"] = "application/json";

            // this will be fired after the upload is complete.
            _client.UploadStringCompleted += (sndr, evnt) =>
            {
                if (evnt.Error != null)
                {
                    UploadResponse = string.IsNullOrWhiteSpace(evnt.Error.Message)
                        ? @"An exception occurred."
                        : evnt.Error.Message;
                }
                else if (evnt.Cancelled)
                {
                    UploadResponse = "Operation was canceled.";
                }
                EA.GetEvent<CreateAndUploadFile>().Publish(string.Empty); // chain the uploads so only one is happening at a time for now.
            };

            var document = HtmlPage.Document;
            var txtArea1 = document.GetElementById("MainContent_txtPNGBytes");
            if (txtArea1 == null) return;
            var base64 = txtArea1.GetProperty("value").ToString();
            var myObject = new Message { CompositionId = Composition.Id.ToString(), CollaborationId = Current.User.Index, Text = base64 };
            var json = Serialization.ToJson(myObject);
            _client.UploadStringAsync(_uri, "POST", json);
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}
